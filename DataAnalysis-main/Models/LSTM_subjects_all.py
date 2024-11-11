import os
import torch
import torch.nn as nn
import pandas as pd
import time
import matplotlib.pyplot as plt
from torch.nn.utils.rnn import pad_sequence, pack_padded_sequence, pad_packed_sequence

test_root = "G:/My Drive/Test"
debug = False


def printd(*args, **kwargs):
    if debug:
        print(*args, **kwargs)

def save_parameters(out_file, hidden_size, dropout, layers, weight_decay, learning_rate, epochs, window_size):
    with open(out_file, "w") as f:
        f.write(f"Hidden Size: {hidden_size}\n")
        f.write(f"Dropout: {dropout}\n")
        f.write(f"Layers: {layers}\n")
        f.write(f"Weight Decay: {weight_decay}\n")
        f.write(f"Learning Rate: {learning_rate}\n")
        f.write(f"Epochs: {epochs}\n")
        f.write(f"Window Size: {window_size}\n")

class StatefulLSTM(nn.Module):
    def __init__(self, input_size, hidden_size, dropout, num_layers=1):
        super(StatefulLSTM, self).__init__()
        self.lstm = nn.LSTM(input_size, hidden_size, num_layers, batch_first=True, dropout=dropout)
        self.fc = nn.Linear(hidden_size, 1)
        self.hidden_size = hidden_size
        self.num_layers = num_layers

    def forward(self, x, lengths, hidden=None):
        if hidden is None:
            h0 = torch.zeros(self.num_layers, x.size(0), self.hidden_size).to(x.device)
            c0 = torch.zeros(self.num_layers, x.size(0), self.hidden_size).to(x.device)
            hidden = (h0, c0)

        # Pack the sequence
        x_packed = pack_padded_sequence(x, lengths, batch_first=True, enforce_sorted=False)
        out_packed, hidden = self.lstm(x_packed, hidden)

        # Unpack the sequence
        out, _ = pad_packed_sequence(out_packed, batch_first=True)

        # Apply the fully connected layer to the last valid time step of each sequence
        idx = (lengths - 1).view(-1, 1).expand(len(lengths), out.size(2)).unsqueeze(1)
        out_last = out.gather(1, idx).squeeze(1)

        out = self.fc(out_last)
        return out, hidden


def prepare_padded_sequences(data):
    # Convert to tensor and sort by length
    data_tensors = [torch.tensor(seq, dtype=torch.float32) for seq in data]
    lengths = torch.tensor([len(seq) for seq in data_tensors])

    # Pad sequences
    padded_data = pad_sequence(data_tensors, batch_first=True)
    return padded_data, lengths


def train(model, all_subject_data, all_subject_labels, optimizer, criterion, epochs, window_size, split_partitions=3):
    print(f"Training with batch of subjects")
    train_losses = []
    val_losses = []

    batch_size = len(all_subject_data)  # Set batch size to the number of subjects

    for epoch in range(epochs):
        val_split = ((epoch % (split_partitions - 1)) + 1) / split_partitions

        # Split data and labels for each subject
        all_train_data, all_train_labels, all_val_data, all_val_labels = [], [], [], []
        for subject_data, subject_labels in zip(all_subject_data, all_subject_labels):
            train_data, train_labels, val_data, val_labels = test_val_split(subject_data, subject_labels, val_split)
            all_train_data.append(train_data)
            all_train_labels.append(train_labels)
            all_val_data.append(val_data)
            all_val_labels.append(val_labels)

        # Prepare padded sequences for training
        train_data_padded, train_lengths = prepare_padded_sequences(all_train_data)
        val_data_padded, val_lengths = prepare_padded_sequences(all_val_data)

        epoch_train_loss = 0
        init_time = time.time()
        outputs = []
        targets = []
        hidden = None
        model.train()

        for i in range(0, train_data_padded.size(1) - window_size, 1):
            x_batch = train_data_padded[:, i:i + window_size]
            y_batch = torch.tensor([train_labels[i + window_size] for train_labels in all_train_labels],
                                   dtype=torch.float32).unsqueeze(1).to(device)

            optimizer.zero_grad()
            x_batch = x_batch.to(device)

            output, hidden = model(x_batch, train_lengths, hidden)
            hidden = (hidden[0].detach(), hidden[1].detach())
            outputs.append(output.detach().cpu().numpy())

            loss = criterion(output, y_batch)
            loss.backward()
            optimizer.step()
            epoch_train_loss += loss.item()

        train_losses.append(epoch_train_loss / batch_size)

        # Validation
        val_outputs = []
        epoch_val_loss = 0
        hidden = None
        model.eval()
        with torch.no_grad():
            for i in range(0, val_data_padded.size(1) - window_size, 1):
                x_batch = val_data_padded[:, i:i + window_size]
                y_batch = torch.tensor([val_labels[i + window_size] for val_labels in all_val_labels],
                                       dtype=torch.float32).unsqueeze(1).to(device)

                x_batch = x_batch.to(device)
                output, hidden = model(x_batch, val_lengths, hidden)
                val_outputs.append(output.detach().cpu().numpy())

                loss = criterion(output, y_batch)
                epoch_val_loss += loss.item()

            val_losses.append(epoch_val_loss / batch_size)

        # Plotting and saving results...
        # Your plotting code remains unchanged


def test_val_split(subject_data, subject_labels, val_split):
    split_point = int(len(subject_data) * (1 - val_split))
    part1_data = subject_data[:split_point]
    part1_labels = subject_labels[:split_point]
    part2_data = subject_data[split_point:]
    part2_labels = subject_labels[split_point:]
    if val_split <= 0.5:
        return part1_data, part1_labels, part2_data, part2_labels
    else:
        return part2_data, part2_labels, part1_data, part1_labels


def main_training_loop():
    all_subject_data = []
    all_subject_labels = []

    for i in subjects:
        df = pd.read_csv(f'{test_root}/S{i}/WindowedCsv_0.5_0.25_standFalse_normFalse/S{i}_Movement.csv', skiprows=1,
                         sep=";")
        dante = pd.read_csv(f'{test_root}/S{i}/WindowedCsv_0.5_0.25_standFalse_normFalse/S{i}_Dante.csv', skiprows=1,
                            sep=";")
        subject_data = df.values
        subject_labels = dante.values

        # Align data lengths
        min_data_len = min(len(subject_data), len(subject_labels))
        subject_data = subject_data[:min_data_len]
        subject_labels = subject_labels[:min_data_len]

        all_subject_data.append(subject_data)
        all_subject_labels.append(subject_labels)

    input_size = all_subject_data[0].shape[1]  # Assuming all subjects have the same number of features

    model = StatefulLSTM(input_size, hidden_size, dropout, num_layers=layers).to(device)
    optimizer = torch.optim.Adam(model.parameters(), lr=learning_rate, weight_decay=weight_decay)
    criterion = nn.MSELoss()

    # Train all subjects in parallel
    train(model, all_subject_data, all_subject_labels, optimizer, criterion, epochs, window_size)


if __name__ == "__main__":
    # Hyperparameters and settings
    hidden_size = 150
    epochs = 35
    window_size = 1
    dropout = 0.6
    layers = 2
    weight_decay = 1e-3
    learning_rate = 1e-05
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
    subjects = [i for i in range(28) if i != 25 and i != 0]
    out_dir = "Outputs"
    os.makedirs(out_dir, exist_ok=True)
    save_parameters(f"{out_dir}/parameters.txt", hidden_size, dropout, layers, weight_decay, learning_rate, epochs,
                    window_size)

    main_training_loop()
