import torch
import torch.nn as nn
import torch.optim as optim
from torch.utils.data import TensorDataset, DataLoader
from sklearn.model_selection import train_test_split, TimeSeriesSplit
from sklearn.preprocessing import StandardScaler
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from ax.service.managed_loop import optimize
from statistics import mean
from pykalman import KalmanFilter

import logging
from ax.utils.common.logger import get_logger

# Set device
device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
print("Using device: ", device)

# Only show log messages of ERROR while testing.
logger = get_logger(__name__, level=logging.ERROR)
if logger.parent is not None and hasattr(logger.parent, "handlers"):
    logger.parent.handlers[0].setLevel(logging.ERROR)

# Load and preprocess data
data = pd.read_csv('OCULUS_dataset.csv')
data.fillna(data.mean(), inplace=True)

features = data.drop(["game_section", "stress_label", "subject"], axis=1)
labels = data["stress_label"]

scaler = StandardScaler()
features = scaler.fit_transform(features)

features_tensor = torch.tensor(features, dtype=torch.float32).to(device)
labels_tensor = torch.tensor(labels.values, dtype=torch.float32).to(device)


class StressLSTM(nn.Module):
    def __init__(self, input_size, hidden_size, num_layers, dropout_rate_1, dropout_rate_2, dropout_rate_fc):
        super(StressLSTM, self).__init__()
        self.lstm1 = nn.LSTM(input_size, hidden_size, num_layers, batch_first=True, dropout=dropout_rate_1)
        self.lstm2 = nn.LSTM(hidden_size, hidden_size, num_layers, batch_first=True, dropout=dropout_rate_2)
        self.dropout = nn.Dropout(dropout_rate_fc)
        self.fc = nn.Linear(hidden_size, 1)

    def forward(self, x):
        out, _ = self.lstm1(x)
        out, _ = self.lstm2(out)
        out = self.dropout(out)
        out = self.fc(out).squeeze(-1)
        return out


parameters = [
    {"name": "lr", "type": "range", "bounds": [1e-5, 1e-3], "value_type": "float"},
    {"name": "hidden_size", "type": "range", "bounds": [50, 256], "value_type": "int"},
    {"name": "num_layers", "type": "fixed", "value": 2, "value_type": "int"},
    {"name": "num_epochs", "type": "range", "bounds": [50, 200], "value_type": "int"},
    {"name": "dropout_rate_1", "type": "range", "bounds": [0.2, 0.5], "value_type": "float"},
    {"name": "dropout_rate_2", "type": "range", "bounds": [0.2, 0.5], "value_type": "float"},
    {"name": "dropout_rate_fc", "type": "range", "bounds": [0.2, 0.5], "value_type": "float"},
    {"name": "weight_decay", "type": "range", "bounds": [1e-5, 1e-3], "value_type": "float"},
    {"name": "batch_size", "type": "range", "bounds": [32, 128], "value_type": "int"}
]

criterion = nn.MSELoss().to(device)
mae_criterion = nn.L1Loss().to(device)


def evaluate_full_dataset(parameters, features, labels):
    print("Evaluating with parameters: " + str(parameters))

    # Split data
    X_train, X_val, y_train, y_val = train_test_split(features, labels, test_size=0.2, shuffle=False)

    # Create DataLoader
    train_dataset = TensorDataset(X_train, y_train)
    train_dataloader = DataLoader(train_dataset, batch_size=int(parameters['batch_size']), shuffle=False)

    # Model initialization
    model = StressLSTM(input_size=features.shape[1],
                       hidden_size=int(parameters['hidden_size']),
                       num_layers=int(parameters['num_layers']),
                       dropout_rate_1=parameters['dropout_rate_1'],
                       dropout_rate_2=parameters['dropout_rate_2'],
                       dropout_rate_fc=parameters['dropout_rate_fc']).to(device)

    # Optimizer and loss function
    optimizer = optim.Adam(model.parameters(), lr=parameters['lr'], weight_decay=parameters['weight_decay'])

    best_val_loss = float('inf')
    patience, patience_threshold = 0, 10  # Early stopping
    actual_epochs = 0
    for epoch in range(int(parameters['num_epochs'])):
        actual_epochs += 1
        model.train()
        running_loss = 0.0

        # Training loop
        for batch_features, batch_labels in train_dataloader:
            batch_features, batch_labels = batch_features.unsqueeze(0).to(device), batch_labels.view(-1).to(device)

            optimizer.zero_grad()
            outputs = model(batch_features)
            loss = criterion(outputs, batch_labels)
            loss.backward()
            optimizer.step()

            running_loss += loss.item()

        # Validation
        model.eval()
        with torch.no_grad():
            val_outputs = model(X_val.unsqueeze(0).to(device))
            val_loss = criterion(val_outputs.view(-1), y_val.view(-1).to(device))

        print(
            f'Epoch [{epoch + 1}/{int(parameters["num_epochs"])}], Loss: {running_loss / len(train_dataloader)}, Val Loss: {val_loss.item()}')

        # Early stopping logic
        if val_loss.item() < best_val_loss:
            best_val_loss = val_loss.item()
            patience = 0
        else:
            patience += 1
            if patience >= patience_threshold:
                print(f"Early stopping at epoch {epoch + 1}")
                break

    return best_val_loss, actual_epochs


def evaluate_with_objective(parameters):
    mse_loss, actual_epochs = evaluate_full_dataset(parameters, features_tensor, labels_tensor)
    print(f"MSE Loss: {mse_loss}, Actual Epochs: {actual_epochs}")
    return {"loss": mse_loss, "actual_epochs": actual_epochs}  # Return only MSE as the optimization target


best_parameters, values, experiment, model = optimize(
    parameters=parameters,
    evaluation_function=evaluate_with_objective,
    objective_name='loss'
)
print(values)
print(best_parameters)

input_size = features.shape[1]
output_size = 1
learning_rate = best_parameters['lr']
hidden_size = int(best_parameters['hidden_size'])
num_layers = int(best_parameters['num_layers'])
num_epochs = int(best_parameters['num_epochs'])
dropout_rate_1 = best_parameters['dropout_rate_1']
dropout_rate_2 = best_parameters['dropout_rate_2']
dropout_rate_fc = best_parameters['dropout_rate_fc']
weight_decay = best_parameters['weight_decay']
batch_size = int(best_parameters['batch_size'])

tscv = TimeSeriesSplit(n_splits=5)
kf = KalmanFilter(initial_state_mean=0, n_dim_obs=1)


def calculate_loss_and_mae(model, criterion, mae_criterion, inputs, targets, use_kalman_filter=False):
    outputs = model(inputs.unsqueeze(0).to(device))
    if use_kalman_filter:
        (filtered_state_means, filtered_state_covariances) = kf.filter(outputs.cpu().detach().numpy())
        outputs = torch.tensor(filtered_state_means, dtype=torch.float32).view(1, -1).to(device)
    loss = criterion(outputs, targets.unsqueeze(0).to(device))
    mae = mae_criterion(outputs, targets.unsqueeze(0).to(device))
    return loss, mae


grouped = data.groupby('subject')


def do_stuff(i):
    all_X_train, all_y_train, all_X_val, all_y_val, all_X_test, all_y_test = [], [], [], [], [], []
    for name, group in grouped:
        print(f"Subject: {name}")
        features = group.drop(["game_section", "stress_label", "subject"], axis=1)
        labels = group["stress_label"]

        scaler = StandardScaler()
        features = scaler.fit_transform(features)

        features_tensor = torch.tensor(features, dtype=torch.float32).to(device)
        labels_tensor = torch.tensor(labels.values, dtype=torch.float32).to(device)

        for train_index, test_index in tscv.split(features_tensor):
            X_train_val, X_test = features_tensor[train_index], features_tensor[test_index]
            y_train_val, y_test = labels_tensor[train_index], labels_tensor[test_index]

            X_train, X_val, y_train, y_val = train_test_split(X_train_val, y_train_val, test_size=0.2, shuffle=False)

            all_X_train.append(X_train)
            all_y_train.append(y_train)
            all_X_val.append(X_val)
            all_y_val.append(y_val)
            all_X_test.append(X_test)
            all_y_test.append(y_test)

    X_train = torch.cat(all_X_train).to(device)
    y_train = torch.cat(all_y_train).to(device)
    X_val = torch.cat(all_X_val).to(device)
    y_val = torch.cat(all_y_val).to(device)
    X_test = torch.cat(all_X_test).to(device)
    y_test = torch.cat(all_y_test).to(device)

    window_sizes = [2, 4, 6]

    all_train_losses, all_val_losses, all_train_maes, all_val_maes, losses, maes = [], [], [], [], [], []
    all_filtered_train_losses, all_filtered_val_losses, all_filtered_train_maes, all_filtered_val_maes, filtered_losses, filtered_maes = [], [], [], [], [], []

    for window_size in window_sizes:
        print(f"Window Size: {window_size}")
        if len(X_train) < window_size or len(y_train) < window_size:
            continue

        train_dataset = TensorDataset(X_train[:len(X_train) // window_size * window_size],
                                      y_train[:len(y_train) // window_size * window_size])
        train_dataloader = DataLoader(train_dataset, batch_size=window_size, shuffle=False)

        model = StressLSTM(input_size, hidden_size, num_layers, dropout_rate_1, dropout_rate_2, dropout_rate_fc).to(
            device)
        optimizer = optim.Adam(model.parameters(), lr=learning_rate, weight_decay=weight_decay)

        model.eval()
        with torch.no_grad():
            training_outputs = model(X_train.unsqueeze(0).to(device))
            kf.em(training_outputs.cpu().detach().numpy(), n_iter=10)

        train_losses, val_losses, train_maes, val_maes = [], [], [], []
        filtered_train_losses, filtered_val_losses, filtered_train_maes, filtered_val_maes = [], [], [], []
        for epoch in range(num_epochs):
            print(f"\rEpoch {epoch + 1}/{num_epochs}", flush=True)
            running_loss = 0.0
            running_mae = 0.0
            running_filtered_loss = 0.0
            running_filtered_mae = 0.0
            num_batches = 0

            for batch_features, batch_labels in train_dataloader:
                model.train()
                loss, mae = calculate_loss_and_mae(model, criterion, mae_criterion, batch_features, batch_labels)
                filtered_loss, filtered_mae = calculate_loss_and_mae(model, criterion, mae_criterion, batch_features,
                                                                     batch_labels, use_kalman_filter=True)

                optimizer.zero_grad()
                loss.backward()
                optimizer.step()

                running_loss += loss.item()
                running_mae += mae.item()
                running_filtered_loss += filtered_loss.item()
                running_filtered_mae += filtered_mae.item()
                num_batches += 1

            average_train_loss = running_loss / num_batches
            average_train_mae = running_mae / num_batches
            average_filtered_loss = running_filtered_loss / num_batches
            average_filtered_mae = running_filtered_mae / num_batches
            train_losses.append(average_train_loss)
            train_maes.append(average_train_mae)
            filtered_train_losses.append(average_filtered_loss)
            filtered_train_maes.append(average_filtered_mae)

            model.eval()
            with torch.no_grad():
                val_loss, val_mae = calculate_loss_and_mae(model, criterion, mae_criterion, X_val, y_val)
                val_filtered_loss, val_filtered_mae = calculate_loss_and_mae(model, criterion, mae_criterion, X_val,
                                                                             y_val, use_kalman_filter=True)

            val_losses.append(val_loss.item())
            val_maes.append(val_mae.item())
            filtered_val_losses.append(val_filtered_loss.item())
            filtered_val_maes.append(val_filtered_mae.item())

            model.eval()
            with torch.no_grad():
                test_loss, test_mae = calculate_loss_and_mae(model, criterion, mae_criterion, X_test, y_test)
                test_filtered_loss, test_filtered_mae = calculate_loss_and_mae(model, criterion, mae_criterion, X_test,
                                                                               y_test, use_kalman_filter=True)

            losses.append(test_loss.item())
            maes.append(test_mae.item())
            filtered_losses.append(test_filtered_loss.item())
            filtered_maes.append(test_filtered_mae.item())

        fig, axs = plt.subplots(2, 2, figsize=(15, 10))

        maxy = max(max(train_losses), max(val_losses), max(filtered_train_losses), max(filtered_val_losses),
                   max(train_maes), max(val_maes), max(filtered_train_maes), max(filtered_val_maes))

        axs[0, 0].plot(train_losses, label='Training MSE')
        axs[0, 0].plot(val_losses, label='Validation MSE')
        axs[0, 0].set_xlabel('Epoch')
        axs[0, 0].set_ylabel('MSE')
        axs[0, 0].set_ylim(0, maxy)
        axs[0, 0].legend()
        axs[0, 0].set_title('Training and Validation MSE')

        axs[0, 1].plot(filtered_train_losses, label='Filtered Training MSE')
        axs[0, 1].plot(filtered_val_losses, label='Filtered Validation MSE')
        axs[0, 1].set_xlabel('Epoch')
        axs[0, 1].set_ylabel('MSE')
        axs[0, 1].set_ylim(0, maxy)
        axs[0, 1].legend()
        axs[0, 1].set_title('Filtered Training and Validation MSE')

        axs[1, 0].plot(train_maes, label='Training MAE')
        axs[1, 0].plot(val_maes, label='Validation MAE')
        axs[1, 0].set_xlabel('Epoch')
        axs[1, 0].set_ylabel('MAE')
        axs[1, 0].set_ylim(0, maxy)
        axs[1, 0].legend()
        axs[1, 0].set_title('Training and Validation MAE')

        axs[1, 1].plot(filtered_train_maes, label='Filtered Training MAE')
        axs[1, 1].plot(filtered_val_maes, label='Filtered Validation MAE')
        axs[1, 1].set_xlabel('Epoch')
        axs[1, 1].set_ylabel('MAE')
        axs[1, 1].set_ylim(0, maxy)
        axs[1, 1].legend()
        axs[1, 1].set_title('Filtered Training and Validation MAE')

        fig.suptitle(f'Window Size: {window_size}')
        plt.tight_layout()
        plt.savefig(f"_subject_{str(i)}window_size_{str(window_size)}.png")

        print("Average test MSE :", mean(losses))
        print("Average test MAE :", mean(maes))

        print("Average filtered test MSE :", mean(filtered_losses))
        print("Average filtered test MAE :", mean(filtered_maes))


for i in range(10):
    do_stuff(i)
