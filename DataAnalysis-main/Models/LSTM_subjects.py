# prompt: rewrite the code leaving some windows out for the validation, and add the validation to the plot
import os

import numpy as np
import torch
import torch.nn as nn
import pandas as pd
import time
import matplotlib.pyplot as plt
import threading

test_root = "/home/marco/gdrive/Test"

debug = False


def printd(*args, **kwargs):
    if debug:
        print(*args, **kwargs)


class StatefulLSTM(nn.Module):
    def __init__(self, input_size, hidden_size, dropout, num_layers=1):
        super(StatefulLSTM, self).__init__()
        self.lstm = nn.LSTM(input_size, hidden_size, num_layers, batch_first=True, dropout=dropout)
        self.fc = nn.Linear(hidden_size, 1)
        self.hidden_size = hidden_size
        self.num_layers = num_layers

    def forward(self, x, hidden=None):
        if hidden is None:
            h0 = torch.zeros(self.num_layers, 1, self.hidden_size).to(x.device)
            c0 = torch.zeros(self.num_layers, 1, self.hidden_size).to(x.device)
            hidden = (h0, c0)
        out, hidden = self.lstm(x, hidden)
        out = self.fc(out[:, -1, :])
        return out, hidden


def train(model, subject_data, subject_labels, optimizer, criterion, epochs, window_size, subject_id,
          split_partitions=3):
    print(f"Training with subject {subject_id}")
    train_losses = []
    val_losses = []
    for epoch in range(epochs):
        prep_init_time = time.time()
        val_split = ((epoch % (split_partitions - 1)) + 1) / split_partitions
        train_data, train_labels, val_data, val_labels = test_val_split(subject_data, subject_labels, val_split)
        prep_init_time = time.time() - prep_init_time
        epoch_train_loss = 0
        init_time = time.time()
        outputs = []
        targets = []
        hidden = None
        model.train()
        batch_times = np.array([])

        for i in range(0, len(train_data) - window_size, 1):
            batch_time = time.time()
            x = train_data[i:i + window_size]
            y = train_labels[i + window_size]
            # print(y)
            optimizer.zero_grad()
            x = torch.tensor(x, dtype=torch.float32).unsqueeze(0).to(device)
            y = torch.tensor(y, dtype=torch.float32).unsqueeze(0).to(device)
            output, hidden = model(x, hidden)
            hidden = (hidden[0].detach(), hidden[1].detach())
            outputs.append(output.item())
            targets.append(y.item())
            loss = criterion(output, y)
            loss.backward()
            optimizer.step()
            epoch_train_loss += loss.item()
            batch_times = np.append(batch_times, time.time() - batch_time)

        train_losses.append(epoch_train_loss / len(train_data))

        val_outputs = []
        epoch_val_loss = 0
        hidden = None
        model.eval()
        with torch.no_grad():
            for i in range(0, len(val_data) - window_size, 1):
                x = val_data[i:i + window_size]
                y = val_labels[i + window_size]
                x = torch.tensor(x, dtype=torch.float32).unsqueeze(0).to(device)
                y = torch.tensor(y, dtype=torch.float32).unsqueeze(0).to(device)
                output, hidden = model(x, hidden)
                val_outputs.append(output.item())
                targets.append(y.item())
                loss = criterion(output, y)
                epoch_val_loss += loss.item()
        val_losses.append(epoch_val_loss / len(val_data))
        val_indexes = [i + len(outputs) for i in range(len(val_outputs))]

        # plt.figure(figsize=(50, 10))
        # plt.title(f"Subject {subject_id}")
        # plt.plot(outputs, label="Train Predictions", color="red")
        # plt.plot(val_indexes, val_outputs, label="Val Predictions", color="orange")
        # plt.plot(targets, label="Targets", color="blue")
        # plt.xlabel("Time")
        # plt.ylabel("Stress Level")
        # plt.legend()
        # plt.savefig(f"{out_dir}/S{subject_id}/{epoch}_predictions.png")
        # plt.close()

        print(
            f"[S{subject_id} | Epoch {epoch + 1}] Train Loss: {train_losses[-1]:.4f} | Val Loss: {val_losses[-1]:.4f}"
            f"\n\tPrep: {prep_init_time:.3f} | Mean Batch: {batch_times.mean():.3f} | Tot: {(time.time() - init_time):.3f}s")

    # Evaluating with full subject data
    outputs = []
    targets = []
    hidden = None
    model.eval()
    with torch.no_grad():
        for i in range(0, len(subject_data) - window_size, 1):
            x = subject_data[i:i + window_size]
            y = subject_labels[i + window_size]
            x = torch.tensor(x, dtype=torch.float32).unsqueeze(0).to(device)
            y = torch.tensor(y, dtype=torch.float32).unsqueeze(0).to(device)
            output, hidden = model(x, hidden)
            outputs.append(output.item())
            targets.append(y.item())
        plt.figure(figsize=(50, 10))
        plt.title(f"Subject {subject_id} - Full Data")
        plt.plot(outputs, label="Predictions", color="red")
        plt.plot(targets, label="Targets", color="blue")
        plt.xlabel("Time")
        plt.ylabel("Stress Level")
        plt.legend()
        plt.savefig(f"{out_dir}/S{subject_id}_full_predictions.png")
        plt.close()

    plt.figure()
    plt.title(f"Subject {subject_id}")
    plt.plot(train_losses, label="Training Loss")
    plt.plot(val_losses, label="Validation Loss")
    plt.xlabel("Epoch")
    plt.ylabel("Loss")
    plt.legend()
    plt.savefig(f"{out_dir}/S{subject_id}_loss.png")
    plt.close()


def test_val_split(subject_data, subject_labels, val_split):
    split_point = int(len(subject_data) * (1 - val_split))
    printd(f"train_data_len:{split_point}")
    part1_data = subject_data[:split_point]
    part1_labels = subject_labels[:split_point]
    part2_data = subject_data[split_point:]
    part2_labels = subject_labels[split_point:]
    printd(f"labels_train:{part1_labels.shape}")
    printd(f"labels_val:{part2_labels.shape}")
    printd(f"data_train:{part1_data.shape}")
    printd(f"data_val:{part2_data.shape}")

    if val_split <= 0.5:
        return part1_data, part1_labels, part2_data, part2_labels
    else:
        return part2_data, part2_labels, part1_data, part1_labels


device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
print(f"Using device {device}: {torch.cuda.get_device_name(0)}")
subjects = [i for i in range(28) if i != 25 and i != 0]
out_dir = "/home/marco/Outputs_Selected_Face"
os.makedirs(out_dir)


def save_parameters(out_file, hidden_size, dropout, layers, weight_decay, learning_rate, epochs, window_size):
    with open(out_file, "w") as f:
        f.write(f"Hidden Size: {hidden_size}\n")
        f.write(f"Dropout: {dropout}\n")
        f.write(f"Layers: {layers}\n")
        f.write(f"Weight Decay: {weight_decay}\n")
        f.write(f"Learning Rate: {learning_rate}\n")
        f.write(f"Epochs: {epochs}\n")
        f.write(f"Window Size: {window_size}\n")


hidden_size = 150
epochs = 35
window_size = 1
dropout = 0.6
layers = 2
weight_decay = 1e-3
learning_rate = 1e-5




def analize_subject(i):
    init_time = time.time()
    df = pd.read_csv(f'{test_root}/S{i}/WindowedCsv_0.5_0.25_standFalse_normFalse/FeatureSelected/S{i}_Face.csv',
                     skiprows=1,
                     sep=";")
    dante = pd.read_csv(f'{test_root}/S{i}/WindowedCsv_0.5_0.25_standFalse_normFalse/S{i}_Dante.csv', skiprows=1,
                        sep=";")
    subject_data = df.values
    subject_labels = dante.values
    # align data lengths
    min_data_len = min(len(subject_data), len(subject_labels))
    subject_data = subject_data[:min_data_len]
    subject_labels = subject_labels[:min_data_len]
    input_size = subject_data.shape[1]
    model = StatefulLSTM(input_size, hidden_size, dropout, num_layers=layers).to(device)
    optimizer = torch.optim.Adam(model.parameters(), lr=learning_rate, weight_decay=weight_decay)
    criterion = nn.MSELoss()
    os.makedirs(out_dir + "/S" + str(i))
    train(model, subject_data, subject_labels, optimizer, criterion, epochs, window_size, i, split_partitions=10)
    print(f"Subject {i} took {(time.time() - init_time):.3f} seconds")

if __name__ == '__main__':
    save_parameters(f"{out_dir}/parameters.txt", hidden_size, dropout, layers, weight_decay, learning_rate, epochs,
                    window_size)
    #replace loop with multithreading

    start_time = time.time()
    for i in subjects:
        analize_subject(i)

    print(f"All subjects finished. Total time : {(time.time() - start_time):.3f}s")