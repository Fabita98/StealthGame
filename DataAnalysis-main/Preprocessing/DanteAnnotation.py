from __future__ import annotations
import os
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from scipy import signal
import math
from sklearn.preprocessing import MinMaxScaler
from scipy.interpolate import interp1d
from datetime import datetime as dt, timedelta
import os

os.environ['TF_ENABLE_ONEDNN_OPTS'] = '0'
AlienPath = 'F:/Data_Analysis'
drivePath = 'D:/University-Masters/Thesis'
savePath = 'data'
stress_survey_path = '../Annotation'
resampled_path = '../Resampled'

SAMPLES_PER_SECOND = 50


def create_directory_if_not_exists(directory):
    if not os.path.exists(directory):
        os.makedirs(directory)


def read_data(subject_folder, filenames, preprocess, add_time, freq, datatype):
    dataframes = []
    for file in filenames:
        dataframe = pd.read_csv(os.path.join(subject_folder, file), index_col=False)
        file = file.split(".")[0].upper()
        processed_df, time = preprocess(dataframe, datatype)
        timestamped_df = add_time(processed_df, time, datatype, freq)
        dataframes.append(timestamped_df)
    return dataframes


def superSampling(df, freq):
    #super campionamento
    values = df['Value']
    resampled_values = signal.resample(values, math.ceil((len(values) / SAMPLES_PER_SECOND) * freq))
    resampled_timestamp = np.linspace(df['TimeStamp'].iloc[0], df['TimeStamp'].iloc[-1], len(resampled_values))
    scaler = MinMaxScaler(feature_range=(0, 1))
    resampled_values = scaler.fit_transform(pd.DataFrame(resampled_values))
    resampled_values = resampled_values.flatten()
    return pd.DataFrame({'TimeStamp': resampled_timestamp, 'Value': resampled_values})


def process_df(df):
    df.columns = df.columns.str.lstrip(' ').str.replace(' ', '_')
    return df


def get_filenames(subject_folder):
    files = os.listdir(subject_folder)

    return sorted(list(filter(lambda f: f.endswith('.csv'), files)),
                  key=lambda x: int(x.replace('stress (', '').replace(').csv', '')))


def processFileOnPath(path=drivePath):
    drive = sorted(list(filter(lambda x: x[0] == 'S', os.listdir(path))), key=lambda x: int(x[1:]))
    count = 0
    for dir in drive:
        templist = list(filter(lambda f: f == 'stress.csv', os.listdir(os.path.join(path, dir))))
        f = templist[0] if len(templist) > 0 else None
        if f is None:
            continue
        df = pd.read_csv(os.path.join(path, dir, f), sep=';')
        # df = process_df(df)
        # df.to_csv(os.path.join(subject_folder, file), index = False)
        print(f'Processed {dir} soggetto {count}')
        df2 = superSampling(df, 90)
        #printValues(count, df, dir,df2)
        save_dataframes_in_file(df2, os.path.join(path, dir), f'Resampled_{dir}.csv')
        if os.path.exists(os.path.join(path, dir, f'Resempled_{dir}.csv')):
            os.remove(os.path.join(path, dir, f'Resempled_{dir}.csv'))
        if os.path.exists(os.path.join(path, dir, f'Resmpled_{dir}.csv')):
            os.remove(os.path.join(path, dir, f'Resmpled_{dir}.csv'))
        if os.path.exists(os.path.join(path, dir, f'Resempled_{dir}')):
            os.remove(os.path.join(path, dir, f'Resempled_{dir}'))
        count += 1
    print(f'Processed {count} files')


def printValues(count, df, name, df2):
    fig, axs = plt.subplots(2, 1)
    axs[0].plot(df['TimeStamp'], df['Value'])
    axs[0].set_title(f'Original {name} soggetto {count}')
    axs[0].set_xlabel('Time')
    axs[0].set_ylabel('Value')
    axs[1].plot(df2['TimeStamp'], df2['Value'])
    axs[1].set_xlabel('Time')
    axs[1].set_ylabel('Value')
    axs[1].set_title(f'Resempled {name} soggetto {count}')
    fig.show()


def save_dataframes_in_file(dataframe, subject_folder, name):
    if not os.path.exists(subject_folder):
        os.makedirs(subject_folder)

    dataframe.to_csv(os.path.join(subject_folder, name), index=False, sep=';')


def processFileOnDisk(subject_folder):
    filenames = get_filenames(subject_folder)
    count = 0
    for file in filenames:
        f = open(os.path.join(subject_folder, file), "r")
        df = pd.read_csv(f, sep=';')
        # df = process_df(df)
        # df.to_csv(os.path.join(subject_folder, file), index = False)
        print(f'Processed {file} soggetto {count}')
        df2 = superSampling(df, 90)
        #printValues(count, df, file, df2)
        save_dataframes_in_file(df2, resampled_path, f'Resempled_{file}')
        count += 1
    print(f'Processed {count} files')


if __name__ == "__main__":
    plt.rcParams.update({'figure.max_open_warning': 0})
    processFileOnPath(path = drivePath)
    #processFileOnDisk(subject_folder='Annotation')
    # main(drivePath)
    # main(savePath)
