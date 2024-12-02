from __future__ import annotations
import os
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from scipy import signal
import math
from sklearn.preprocessing import MinMaxScaler

class DanteAnnotation:
    resampled_path='../Resampled'
    
    def __init__(self, path='D:/University-Masters/Thesis', samples_per_second=50, subjects_to_exclude=[]):
        self.path = path
        self.samples_per_second = samples_per_second
        self.subjects_to_exclude = subjects_to_exclude
        os.environ['TF_ENABLE_ONEDNN_OPTS'] = '0'

    def create_directory_if_not_exists(self, directory):
        if not os.path.exists(directory):
            os.makedirs(directory)

    def read_data(self, subject_folder, filenames, preprocess, add_time, freq, datatype):
        dataframes = []
        for file in filenames:
            dataframe = pd.read_csv(os.path.join(subject_folder, file), index_col=False)
            file = file.split(".")[0].upper()
            processed_df, time = preprocess(dataframe, datatype)
            timestamped_df = add_time(processed_df, time, datatype, freq)
            dataframes.append(timestamped_df)
        return dataframes

    def super_sampling(self, df, freq):
        # Super sampling
        values = df['Value']
        resampled_values = signal.resample(values, math.ceil((len(values) / self.samples_per_second) * freq))
        resampled_timestamp = np.linspace(df['TimeStamp'].iloc[0], df['TimeStamp'].iloc[-1], len(resampled_values))
        scaler = MinMaxScaler(feature_range=(0, 1))
        resampled_values = scaler.fit_transform(pd.DataFrame(resampled_values))
        resampled_values = resampled_values.flatten()
        return pd.DataFrame({'TimeStamp': resampled_timestamp, 'Value': resampled_values})

    def process_df(self, df):
        df.columns = df.columns.str.lstrip(' ').str.replace(' ', '_')
        return df

    def get_filenames(self, subject_folder):
        files = os.listdir(subject_folder)
        return sorted(list(filter(lambda f: f.endswith('.csv'), files)),
                      key=lambda x: int(x.replace('stress (', '').replace(').csv', '')))

    def process_file_on_path(self, path=None):
        path = path or self.path
        dirs = sorted(list(filter(lambda x: x[0] == 'S', os.listdir(path))), key=lambda x: int(x[1:]))
        dirs = [dr for dr in dirs if dr not in self.subjects_to_exclude]
        count = 0
        for dir in dirs:
            templist = list(filter(lambda f: f == 'stress.csv', os.listdir(os.path.join(path, dir))))
            f = templist[0] if len(templist) > 0 else None
            if f is None:
                continue
            df = pd.read_csv(os.path.join(path, dir, f), sep=';')
            print(f'Processed {dir} - Subject {count}')
            df2 = self.super_sampling(df, 90)
            self.save_dataframes_in_file(df2, os.path.join(path, dir), f'Resampled_{dir}.csv')
            for ext in ['Resempled', 'Resmpled', 'Resempld']:
                path_to_check = os.path.join(path, dir, f'{ext}_{dir}')
                if os.path.exists(path_to_check):
                    os.remove(path_to_check)
            count += 1
        print(f'Processed {count} files')

    def print_values(self, count, df, name, df2):
        fig, axs = plt.subplots(2, 1)
        axs[0].plot(df['TimeStamp'], df['Value'])
        axs[0].set_title(f'Original {name} soggetto {count}')
        axs[0].set_xlabel('Time')
        axs[0].set_ylabel('Value')
        axs[1].plot(df2['TimeStamp'], df2['Value'])
        axs[1].set_xlabel('Time')
        axs[1].set_ylabel('Value')
        axs[1].set_title(f'Resampled {name} - Subject {count}')
        fig.show()

    def save_dataframes_in_file(self, dataframe, subject_folder, name):
        self.create_directory_if_not_exists(subject_folder)
        dataframe.to_csv(os.path.join(subject_folder, name), index=False, sep=';')

    def process_file_on_disk(self, subject_folder):
        filenames = self.get_filenames(subject_folder)
        count = 0
        for file in filenames:
            f = open(os.path.join(subject_folder, file), "r")
            df = pd.read_csv(f, sep=';')
            print(f'Processed {file} - Subject {count}')
            df2 = self.super_sampling(df, 90)
            self.save_dataframes_in_file(df2, self.resampled_path, f'Resampled_{file}')
            count += 1
        print(f'Processed {count} files')

    def main(self):
        plt.rcParams.update({'figure.max_open_warning': 0})
        self.process_file_on_path(path=self.path)


if __name__ == "__main__":
    plt.rcParams.update({'figure.max_open_warning': 0})
    danteAnnotation = DanteAnnotation()
    danteAnnotation.process_file_on_path(path=danteAnnotation.path)
