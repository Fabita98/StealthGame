import os
import pandas as pd
import numpy as np
from sklearn.preprocessing import MinMaxScaler, StandardScaler
from tsfresh import extract_features
from tsfresh.feature_extraction import MinimalFCParameters
import time


class FeatureExtraction:
    default_discrete_data =  ['HeartBeatRate', 'IsInStressfulArea', 'Deaths', 'LastCheckpoint']

    def __init__(self, path='D:/University-Masters/Thesis', dim_seconds=0.5,
                 shift_seconds=0.25, norm=False, stand=True, sampling_rate=90, discrete_data=default_discrete_data, subjects_to_exclude=[]):
        self.path = path
        self.dim_seconds = dim_seconds
        self.shift_seconds = shift_seconds
        self.norm = norm
        self.stand = stand
        self.sampling_rate = sampling_rate
        self.discrete_data = discrete_data
        self.subjects_to_exclude = subjects_to_exclude

    def create_window(self, df, subject_folder, file_name):
        num_points = len(df)
        dim_points = int(self.dim_seconds * self.sampling_rate)
        shift_points = int(self.shift_seconds * self.sampling_rate)
        start_index = 0
        index = 0

        while start_index + dim_points <= num_points:
            window = df.iloc[start_index:start_index + dim_points]
            window['W_Index'] = np.zeros(len(window))
            features = self.extract_features_data(window)
            if index == 0:
                if os.path.exists(os.path.join(subject_folder, file_name)):
                    os.remove(os.path.join(subject_folder, file_name))
                features.to_csv(os.path.join(subject_folder, file_name), mode='a', index=False, sep=';')
                index += 1
            else:
                features.to_csv(os.path.join(subject_folder, file_name), header=False, mode='a', index=False, sep=';')
            start_index += shift_points

    def create_window_external(self, df, is_discrete):
        windows = pd.DataFrame()
        num_points = len(df)
        dim_points = int(self.dim_seconds * self.sampling_rate)
        shift_points = int(self.shift_seconds * self.sampling_rate)

        start_index = 0
        while start_index + dim_points <= num_points:
            window = df.iloc[start_index:start_index + dim_points]
            features = self.extract_feature_external_data(window, is_discrete)
            windows = windows.append(features, ignore_index=True)
            start_index += shift_points

        return windows

    @staticmethod
    def extract_features_data(w_data):
        extracted_features = extract_features(w_data, default_fc_parameters=MinimalFCParameters(), column_id='W_Index',
                                              n_jobs=1)
        return extracted_features

    def create_dante_window(self, df_dante):
        windows = pd.DataFrame()
        num_points = len(df_dante)
        dim_points = int(self.dim_seconds * self.sampling_rate)
        shift_points = int(self.shift_seconds * self.sampling_rate)

        start_index = 0
        while start_index + dim_points <= num_points:
            window = df_dante.iloc[start_index:start_index + dim_points]
            features = window.mean()
            windows = windows.append(features, ignore_index=True)
            start_index += shift_points

        return windows

    @staticmethod
    def extract_feature_external_data(w_data, is_discrete):
        if is_discrete:
            extracted_features = w_data.apply(pd.value_counts).idxmax()
        else:
            extracted_features = w_data.mean()
        return extracted_features

    @staticmethod
    def normalize_data(data):
        scaler = MinMaxScaler()
        normalized_data = scaler.fit_transform(data)
        return pd.DataFrame(normalized_data)

    @staticmethod
    def standardize_data(data):
        scaler = StandardScaler()
        standardized_data = scaler.fit_transform(data)
        return pd.DataFrame(standardized_data)

    @staticmethod
    def save_dataframes_in_file(dataframe, subject_folder, name):
        if not os.path.exists(subject_folder):
            os.makedirs(subject_folder)
        dataframe.to_csv(os.path.join(subject_folder, name), index=False, sep=';')

    def process_dante(self, path, dr):
        df_dante = pd.read_csv(f"{path}/{dr}/Resampled_{dr}.csv", sep=';')
        df_dante.drop(columns=[col for col in df_dante.columns if "TimeStamp" in col or "timestamp" in col], inplace=True)
        df_dante = df_dante.iloc[100:]
        win_dante_headers = [col + "_mean" for col in df_dante.columns]
        win_dante = self.create_dante_window(df_dante)
        win_dante.columns = win_dante_headers
        self.save_dataframes_in_file(win_dante, f"{path}/{dr}/WindowedCsv_{self.dim_seconds}_{self.shift_seconds}_stand{self.stand}_norm{self.norm}", f"{dr}_Dante.csv")

    def main(self):
        path = self.path
        dirs = sorted(list(filter(lambda x: x[0] == 'S', os.listdir(path))), key=lambda x: int(x[1:]))
        dirs = [dr for dr in dirs if dr not in self.subjects_to_exclude]
        import warnings
        warnings.filterwarnings("ignore")
        for dr in dirs:
            features_subject_folder = f"{path}/{dr}/WindowedCsv_{self.dim_seconds}_{self.shift_seconds}_stand{self.stand}_norm{self.norm}"
            if not os.path.exists(features_subject_folder):
                os.makedirs(features_subject_folder)
            self.process_dante(path, dr)

            file_folder = list(filter(lambda x: x[0] == 'S', os.listdir(f"{path}/{dr}/ResampledCsv")))
            for file in file_folder:
                start_time = time.time()
                df = pd.read_csv(f"{path}/{dr}/ResampledCsv/{file}", sep=';')
                df.drop(columns=[col for col in df.columns if "TimeStamp" in col or "timestamp" in col], inplace=True)
                if "External" in file:
                    discrete_data = self.discrete_data
                    discrete_data.extend([col for col in df.columns if "SemanticTag" in col or "SemanticObj" in col])
                    discrete_data.extend([col for col in df.columns if "checkpoint" in col])
                    df_discrete = df[discrete_data]
                    df_continuous = df.drop(columns=discrete_data)
                    win_discrete = self.create_window_external(df_discrete, True)
                    win_continuous = self.create_window_external(df_continuous, False)
                    wdf = pd.concat([win_discrete, win_continuous], axis=1)
                    self.save_dataframes_in_file(wdf, features_subject_folder, file)
                else:
                    if self.norm:
                        df = self.normalize_data(df)
                    if self.stand:
                        df = self.standardize_data(df)
                    self.create_window(df, features_subject_folder, file)
                print(f"Windowed data for {file} saved")
                print(f"--- {time.time() - start_time} seconds ---")


if __name__ == "__main__":
    featureExtraction = FeatureExtraction()
    featureExtraction.main()
