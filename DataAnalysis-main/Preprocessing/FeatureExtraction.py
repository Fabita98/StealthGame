import os
import pandas as pd
import tsfresh as tsf
import numpy as np
from sklearn.preprocessing import MinMaxScaler, StandardScaler
from tsfresh import extract_features
from tsfresh.feature_extraction import EfficientFCParameters, MinimalFCParameters
import time
from DataSynchronization import synchronize

drivePath = 'D:/University-Masters/Thesis'
AlienPath = 'F:/Data_Analysis'
# Definisci dim e shift per le sliding window
dim_seconds = 0.5
shift_seconds = 0.25

norm = False
stand = False

# subjects_to_exclude = ([f"S{i}" for i in range(0,21)] +
#                        [f"S{i}" for i in range(22,29)] + ['S25',"S100", "S101"])
#sampling rate dei dati
sampling_rate = 90


def createWindow(df, subject_folder, file_name):
    # Creare finestre temporali con dimensione e shift variabile
    global dim_seconds, shift_seconds, sampling_rate
    windows = []
    num_points = len(df)
    dim_points = int(dim_seconds * sampling_rate)
    shift_points = int(shift_seconds * sampling_rate)
    start_index = 0
    index = 0

    while start_index + dim_points <= num_points:

        window = df.iloc[start_index:start_index + dim_points]
        # Estrai le feature
        window['W_Index'] = np.zeros(len(window))
        features = extractFeaturesData(window)
        if index == 0:
            #open(os.path.join(subject_folder, file_name), 'w').write('sep=;\n') # write the header
            if os.path.exists(os.path.join(subject_folder, file_name)):
                os.remove(os.path.join(subject_folder, file_name))
            features.to_csv(os.path.join(subject_folder, file_name), mode='a', index=False, sep=';')
            index += 1
        else:
            features.to_csv(os.path.join(subject_folder, file_name), header=False, mode='a', index=False, sep=';')
        #windows.append(features)
        start_index += shift_points

    #qui va restituito un dataframe
    #return windows


def createWindowExternal(df, isDiscrete):
    # Creare finestre temporali con dimensione e shift variabile
    global dim_seconds, shift_seconds, sampling_rate
    windows = pd.DataFrame()
    num_points = len(df)
    dim_points = int(dim_seconds * sampling_rate)
    shift_points = int(shift_seconds * sampling_rate)

    start_index = 0

    while start_index + dim_points <= num_points:
        window = df.iloc[start_index:start_index + dim_points]
        # Estrai le feature
        features = extractFeatureExternalData(window, isDiscrete)
        windows = windows.append(features, ignore_index=True)
        start_index += shift_points

    #qui va restituito un dataframe
    return windows


def extractFeaturesData(w_data):
    # Estrarre le feature da ogni finestra tramite tsfresh
    # DA RIVEDERE
    # https://tsfresh.readthedocs.io/en/latest/api/tsfresh.feature_extraction.html#module-tsfresh.feature_extraction.extraction
    extracted_features = extract_features(w_data, default_fc_parameters=MinimalFCParameters(), column_id='W_Index',
                                          n_jobs=1)
    return extracted_features


def createDanteWindow(dfDante):
    global dim_seconds, shift_seconds, sampling_rate
    windows = pd.DataFrame()
    num_points = len(dfDante)
    dim_points = int(dim_seconds * sampling_rate)
    shift_points = int(shift_seconds * sampling_rate)

    start_index = 0

    while start_index + dim_points <= num_points:
        window = dfDante.iloc[start_index:start_index + dim_points]
        # Estrai le feature
        features = window.mean()
        windows = windows.append(features, ignore_index=True)
        start_index += shift_points

    # qui va restituito un dataframe
    return windows


def extractFeatureExternalData(w_data, isDiscrete):
    #qui c'è da fare la distinzione se i dati sono discreti o continui.
    # se sono discreti?
    # esempio: conteggio delle occorrenze di ciascun valore
    #w_data = w_data.to_frame()
    if isDiscrete:
        extracted_features = w_data.apply(pd.value_counts).idxmax()
    # se sono continui prenderei la media, ma dobbiamo capire se ha senso
    else:
        extracted_features = w_data.mean()
    return extracted_features


def normalizeData(data):
    # dati normalizzati tra 0 e 1
    scaler = MinMaxScaler()
    normalized_data = scaler.fit_transform(data)
    return normalized_data


def standardizeData(data):
    # dati standardizzati: std = 1, mean = 0
    scaler = StandardScaler()
    standardized_data = scaler.fit_transform(data)
    return standardized_data


def save_dataframes_in_file(dataframe, subject_folder, name):
    if not os.path.exists(subject_folder):
        os.makedirs(subject_folder)

    dataframe.to_csv(os.path.join(subject_folder, name), index=False, sep=';')


def processDante():
    dfDante = pd.read_csv(path + '/' + dr + f"/Resampled_{dr}.csv", sep=';')
    dfDante.drop(columns=[col for col in dfDante.columns if "TimeStamp" in col or "timestamp" in col], inplace=True)
    dfDante = dfDante.iloc[100:]
    win_dante_headers = [col + "_mean" for col in dfDante.columns]
    winDante = createDanteWindow(dfDante)
    winDante.columns = win_dante_headers
    save_dataframes_in_file(winDante,
                            path + '/' + dr + f"/WindowedCsv_{dim_seconds}_{shift_seconds}_stand{stand}_norm{norm}",
                            f"{dr}_Dante.csv")


if __name__ == "__main__":

    path = drivePath
    dirs = sorted(list(filter(lambda x: x[0] == 'S', os.listdir(path))), key=lambda x: int(x[1:]))
    # dirs = [dr for dr in dirs if dr not in subjects_to_exclude]
    import warnings

    warnings.filterwarnings("ignore")
    for dr in dirs:
        #create folder for single subject
        features_subject_folder = path + '/' + dr + f"/WindowedCsv_{dim_seconds}_{shift_seconds}_stand{stand}_norm{norm}"
        if not os.path.exists(features_subject_folder):
            os.makedirs(features_subject_folder)
        processDante()

        fileFolder = list(filter(lambda x: x[0] == 'S', os.listdir(path + '/' + dr + "/ResampledCsv")))

        for file in fileFolder:
            start_time = time.time()
            #create empty csv file to save extracted features into

            if "External" in file:
                df = pd.read_csv(path + '/' + dr + "/ResampledCsv/" + file, sep=';')
                df.drop(columns=[col for col in df.columns if "TimeStamp" in col or "timestamp" in col], inplace=True)
                discrete_data = ['HeartBeatRate', 
                                #  'MaxHeartBeatRate', 'MinHeartBeatRate', 'AverageHeartBeatRate', 
                                 'IsInStressfulArea', 'Deaths', 'LastCheckpoint']
                discrete_data.extend([col for col in df.columns if "SemanticTag" in col or "SemanticObj" in col])
                discrete_data.extend([col for col in df.columns if "checkpoint" in col])
                dfDiscrete = df[discrete_data]
                dfContinuous = df.drop(columns=discrete_data)
                win_discrete_headers = [col + "_valuesCount" for col in dfDiscrete.columns]
                winDiscrete = createWindowExternal(dfDiscrete, True)
                print(winDiscrete.columns)
                winDiscrete.columns = win_discrete_headers
                win_continuous_headers = [col + "_mean" for col in dfContinuous.columns]
                winContinuous = createWindowExternal(dfContinuous, False)
                winContinuous.columns = win_continuous_headers
                #wdfDiscrete = pd.concat(winDiscrete, axis=0, ignore_index=True)

                #wdfContinuous = pd.concat(winContinuous, axis=0, ignore_index=True)

                wdf = pd.concat([winDiscrete, winContinuous], axis=1)

                save_dataframes_in_file(wdf,
                                        path + '/' + dr + f"/WindowedCsv_{dim_seconds}_{shift_seconds}_stand{stand}_norm{norm}",
                                        f"{file}")
            else:

                df = pd.read_csv(path + '/' + dr + "/ResampledCsv/" + file, sep=';')
                df.drop(columns=[col for col in df.columns if "TimeStamp" in col or "timestamp" in col], inplace=True)
                if norm:
                    df = normalizeData(df)
                if stand:
                    df = standardizeData(df)
                createWindow(df, features_subject_folder, f"{file}")

                #wdf = pd.concat(win, axis=0, ignore_index=True)
            #save_dataframes_in_file(wdf, path + '/' + dr + f"/WindowedCsv_{dim_seconds}_{shift_seconds}_stand{stand}_norm{norm}", f"{file}")
            print(f"Windowed data for {file} saved")
            print("--- %s seconds ---" % (time.time() - start_time))