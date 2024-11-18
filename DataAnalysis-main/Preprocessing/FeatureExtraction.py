import os
import pandas as pd
import numpy as np
from sklearn.preprocessing import MinMaxScaler, StandardScaler
from tsfresh import extract_features
from tsfresh.feature_extraction import MinimalFCParameters
import time
from DataSynchronization import synchronize

drivePath = 'D:/University-Masters/Thesis'
AlienPath = 'F:/Data_Analysis'

# Sliding window parameters
dim_seconds = 0.5
shift_seconds = 0.25

norm = False
stand = False
sampling_rate = 90

# Optimization: Pre-compute sliding window sizes
dim_points = int(dim_seconds * sampling_rate)
shift_points = int(shift_seconds * sampling_rate)

# Function to create windows and extract features
def createWindow(df, subject_folder, file_name):
    num_points = len(df)
    start_index = 0
    windows_list = []

    while start_index + dim_points <= num_points:
        window = df.iloc[start_index:start_index + dim_points].copy()
        window['W_Index'] = 0  # Dummy column for tsfresh `column_id`
        
        features = extractFeaturesData(window)
        windows_list.append(features)
        start_index += shift_points

    # Concatenate all windows and save in one operation
    all_windows = pd.concat(windows_list, ignore_index=True)
    all_windows.to_csv(os.path.join(subject_folder, file_name), index=False, sep=';')

def createWindowExternal(df, isDiscrete):
    num_points = len(df)
    start_index = 0
    windows_list = []

    while start_index + dim_points <= num_points:
        window = df.iloc[start_index:start_index + dim_points]
        features = extractFeatureExternalData(window, isDiscrete)
        windows_list.append(features)
        start_index += shift_points

    return pd.concat(windows_list, ignore_index=True)

# Optimized extractFeaturesData function with tsfresh
def extractFeaturesData(w_data):
    return extract_features(
        w_data,
        default_fc_parameters=MinimalFCParameters(),
        column_id='W_Index',
        column_value='W_Index',
        n_jobs=1
    )

def createDanteWindow(dfDante):
    num_points = len(dfDante)
    start_index = 0
    windows_list = []

    while start_index + dim_points <= num_points:
        window = dfDante.iloc[start_index:start_index + dim_points]
        features = window.mean()
        windows_list.append(features)
        start_index += shift_points

    return pd.DataFrame(windows_list)

def extractFeatureExternalData(w_data, isDiscrete):
    if isDiscrete:
        extracted_features = w_data.apply(pd.value_counts).idxmax()
    else:
        extracted_features = w_data.mean()
    return pd.DataFrame([extracted_features])

def normalizeData(data):
    scaler = MinMaxScaler()
    return scaler.fit_transform(data)

def standardizeData(data):
    scaler = StandardScaler()
    return scaler.fit_transform(data)

def save_dataframes_in_file(dataframe, subject_folder, name):
    if not os.path.exists(subject_folder):
        os.makedirs(subject_folder)
    dataframe.to_csv(os.path.join(subject_folder, name), index=False, sep=';')

def processDante(path, dr):
    dfDante = pd.read_csv(os.path.join(path, dr, f"Resampled_{dr}.csv"), sep=';')
    dfDante.drop(columns=[col for col in dfDante.columns if "TimeStamp" in col or "timestamp" in col], inplace=True)
    dfDante = dfDante.iloc[100:]
    winDante = createDanteWindow(dfDante)
    win_dante_headers = [col + "_mean" for col in dfDante.columns]
    winDante.columns = win_dante_headers
    save_dataframes_in_file(winDante, os.path.join(path, dr, f"WindowedCsv_{dim_seconds}_{shift_seconds}_stand{stand}_norm{norm}"), f"{dr}_Dante.csv")

if __name__ == "__main__":
    path = drivePath
    dirs = sorted([dr for dr in os.listdir(path) if dr.startswith('S')], key=lambda x: int(x[1:]))
    
    import warnings
    warnings.filterwarnings("ignore")

    for dr in dirs:
        features_subject_folder = os.path.join(path, dr, f"WindowedCsv_{dim_seconds}_{shift_seconds}_stand{stand}_norm{norm}")
        if not os.path.exists(features_subject_folder):
            os.makedirs(features_subject_folder)
        
        processDante(path, dr)

        fileFolder = [file for file in os.listdir(os.path.join(path, dr, "ResampledCsv")) if file.startswith('S')]

        for file in fileFolder:
            start_time = time.time()
            df = pd.read_csv(os.path.join(path, dr, "ResampledCsv", file), sep=';')
            df.drop(columns=[col for col in df.columns if "TimeStamp" in col or "timestamp" in col], inplace=True)

            if norm:
                df = normalizeData(df)
            if stand:
                df = standardizeData(df)

            if "External" in file:
                discrete_data = ['HeartBeatRate', 'MaxHeartBeatRate', 'MinHeartBeatRate', 'AverageHeartBeatRate', 'IsInStressfulArea', 'Deaths', 'LastCheckpoint']
                discrete_data.extend([col for col in df.columns if "SemanticTag" in col or "SemanticObj" in col])
                discrete_data.extend([col for col in df.columns if "checkpoint" in col])

                dfDiscrete = df[discrete_data]
                dfContinuous = df.drop(columns=discrete_data)

                winDiscrete = createWindowExternal(dfDiscrete, True)
                winContinuous = createWindowExternal(dfContinuous, False)

                winDiscrete.columns = [col + "_valuesCount" for col in dfDiscrete.columns]
                winContinuous.columns = [col + "_mean" for col in dfContinuous.columns]

                wdf = pd.concat([winDiscrete, winContinuous], axis=1)
                save_dataframes_in_file(wdf, features_subject_folder, file)
            else:
                createWindow(df, features_subject_folder, file)

            print(f"Windowed data for {file} saved")
            print("--- %s seconds ---" % (time.time() - start_time))

