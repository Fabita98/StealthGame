import os
import pandas as pd
import tsfresh as tsf
import numpy as np
from sklearn.preprocessing import MinMaxScaler, StandardScaler
from tsfresh import extract_features
from tsfresh.feature_extraction import MinimalFCParameters
import time

# Paths
drivePath = 'D:/University-Masters/Thesis'
AlienPath = 'F:/Data_Analysis'

# Parameters for sliding windows
dim_seconds = 0.5
shift_seconds = 0.25
sampling_rate = 90

# Normalization/Standardization flags
norm = False
stand = False

# Excluded subjects (optional)
# subjects_to_exclude = [f"S{i}" for i in range(0,21)] + [f"S{i}" for i in range(22,29)] + ['S25', "S100", "S101"]

def create_sliding_windows(df, dim_points, shift_points):
    """Generate sliding windows for feature extraction."""
    start_index = 0
    windows = []
    num_points = len(df)

    while start_index + dim_points <= num_points:
        window = df.iloc[start_index:start_index + dim_points]
        windows.append(window)
        start_index += shift_points

    return windows

def extract_features_from_window(window, is_discrete=False):
    """Extract features from a window of data using tsfresh or basic statistics."""
    if is_discrete:
        return window.apply(pd.value_counts).idxmax().to_frame().T
    else:
        return window.mean().to_frame().T

def normalize_or_standardize(df, normalize=False, standardize=False):
    """Apply normalization or standardization if specified."""
    if normalize:
        scaler = MinMaxScaler()
        df = pd.DataFrame(scaler.fit_transform(df), columns=df.columns)
    if standardize:
        scaler = StandardScaler()
        df = pd.DataFrame(scaler.fit_transform(df), columns=df.columns)
    return df

def save_dataframe(df, folder, filename):
    """Save a DataFrame to a specified path."""
    if not os.path.exists(folder):
        os.makedirs(folder)
    df.to_csv(os.path.join(folder, filename), index=False, sep=';')

def process_file(subject_folder, file, df, is_external, discrete_columns):
    """Process a single file, extracting features based on type (external or regular)."""
    dim_points = int(dim_seconds * sampling_rate)
    shift_points = int(shift_seconds * sampling_rate)

    if is_external:
        df_discrete = df[discrete_columns]
        df_continuous = df.drop(columns=discrete_columns)

        win_discrete = pd.concat(
            [extract_features_from_window(win, is_discrete=True) for win in create_sliding_windows(df_discrete, dim_points, shift_points)],
            ignore_index=True
        )
        win_continuous = pd.concat(
            [extract_features_from_window(win, is_discrete=False) for win in create_sliding_windows(df_continuous, dim_points, shift_points)],
            ignore_index=True
        )
        win_discrete.columns = [col + "_valuesCount" for col in df_discrete.columns]
        win_continuous.columns = [col + "_mean" for col in df_continuous.columns]

        combined_features = pd.concat([win_discrete, win_continuous], axis=1)
        save_dataframe(combined_features, subject_folder, file)

    else:
        # For non-external data, set up W_Index and Time columns for tsfresh
        windows = create_sliding_windows(df, dim_points, shift_points)
        feature_dfs = []
        for i, win in enumerate(windows):
            # Add identifier and sort columns for tsfresh
            win = win.copy()
            win['W_Index'] = i  # Each window is uniquely identified by its index
            win['Time'] = range(len(win))  # Sequential time index within each window
            
            # Extract features for the current window
            features = extract_features(
                win,
                default_fc_parameters=MinimalFCParameters(),
                column_id='W_Index',
                column_sort='Time',
                column_value=win.columns[0],  # Assuming first column holds the values; adjust if needed
                n_jobs=1
            )
            feature_dfs.append(features)

        # Concatenate all windows' features into a single DataFrame
        feature_df = pd.concat(feature_dfs, ignore_index=True)
        save_dataframe(feature_df, subject_folder, file)


def process_subject_folder(path, dr, discrete_columns):
    """Process all files in a given subject's folder."""
    subject_folder = os.path.join(path, dr, f"WindowedCsv_{dim_seconds}_{shift_seconds}_stand{stand}_norm{norm}")
    if not os.path.exists(subject_folder):
        os.makedirs(subject_folder)

    fileFolder = [file for file in os.listdir(os.path.join(path, dr, "ResampledCsv")) if file.startswith("S")]

    for file in fileFolder:
        start_time = time.time()
        file_path = os.path.join(path, dr, "ResampledCsv", file)
        df = pd.read_csv(file_path, sep=';')
        df.drop(columns=[col for col in df.columns if "TimeStamp" in col or "timestamp" in col], inplace=True)

        df = normalize_or_standardize(df, norm, stand)
        is_external = "External" in file

        process_file(subject_folder, file, df, is_external, discrete_columns)
        print(f"Processed {file} in {time.time() - start_time:.2f} seconds")

if __name__ == "__main__":
    path = drivePath
    dirs = sorted([dr for dr in os.listdir(path) if dr.startswith("S")], key=lambda x: int(x[1:]))
    # dirs = [dr for dr in dirs if dr not in subjects_to_exclude]  # Optional exclusion

    discrete_columns = ['HeartBeatRate', 'MaxHeartBeatRate', 'MinHeartBeatRate', 'AverageHeartBeatRate', 'IsInStressfulArea']

    for dr in dirs:
        process_subject_folder(path, dr, discrete_columns)
