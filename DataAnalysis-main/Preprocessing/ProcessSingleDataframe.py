from __future__ import annotations
import pandas as pd

class ProcessSingleDataframe:
    default_discrete_data = ['HeartBeatRate', 'MaxHeartBeatRate', 'MinHeartBeatRate', 'AverageHeartBeatRate', 'IsInStressfulArea', 'Deaths', 'LastCheckpoint']

    def __init__(self, path, discrete_data=default_discrete_data, target_samples_per_second=90):
        self.path = path
        self.target_samples_per_second = target_samples_per_second
        self.discrete_data = discrete_data

    def processMovement(self, unique_path):
        df = pd.read_csv(self.path + unique_path, sep=';')
        return self.preprocessData(df)

    def processButton(self, unique_path):
        df = pd.read_csv(self.path + unique_path, sep=';')
        return self.preprocessData(df)

    def processEye(self, unique_path):
        df = pd.read_csv(self.path + unique_path, sep=';')
        return self.preprocessData(df)

    def processFace(self, unique_path):
        df = pd.read_csv(self.path + unique_path, sep=';')
        return self.preprocessData(df)
    
    def processHeartBeat(self, unique_path):
        df = pd.read_csv(self.path + unique_path, sep=';')
        if 'HeartBeatRate' in df.columns:
            non_60_idx = df[df['HeartBeatRate'] != 60].index.min()
            if pd.notnull(non_60_idx):
                non_60_value = df.loc[non_60_idx, 'HeartBeatRate']
                df.loc[:non_60_idx - 1, 'HeartBeatRate'] = non_60_value
        return self.preprocessData(df)
    
    def processExternal(self, unique_path):
        df = pd.read_csv(self.path + unique_path, sep=';')
        if 'HeartBeatRate' in df.columns:
            non_60_idx = df[df['HeartBeatRate'] != 60].index.min()
            if pd.notnull(non_60_idx):
                non_60_value = df.loc[non_60_idx, 'HeartBeatRate']
                df.loc[:non_60_idx - 1, 'HeartBeatRate'] = non_60_value
        return self.preprocessData(df)

    def preprocessData(self, df, has_Discrete=False, target_samples_per_second=None):
        if target_samples_per_second is None:
            target_samples_per_second = self.target_samples_per_second
        
        time_column = "timestampUnityTime"
        sample_interval = pd.Timedelta(seconds=1 / target_samples_per_second)
        if has_Discrete:
            df[time_column] = pd.to_datetime(df[time_column], unit='s')
            self.discrete_data.extend([col for col in df.columns if "SemanticTag" in col or "SemanticObj" in col])
            self.discrete_data.extend([col for col in df.columns if "checkpoint" in col])

            df_resampled = df.set_index(time_column)
            for col in df_resampled.columns:
                if col in self.discrete_data:
                    df_resampled[col] = df_resampled[col].ffill()
                else:
                    df_resampled[col] = df_resampled[col].interpolate(method='linear', limit_direction='forward')

            df_resampled = df_resampled.reset_index()
        else:
            df[time_column] = pd.to_datetime(df[time_column], unit='s')
            df = df.set_index(time_column)
            df_resampled = df.resample(sample_interval).mean()
            df_resampled = df_resampled.interpolate(method='linear', limit_direction='forward')
            df_resampled = df_resampled.reset_index()
        return df_resampled

    def saveProcessedData(self, df, output_path):
        df.to_csv(output_path, index=False, sep=';')


if __name__ == "__main__":
    drivePath = 'D:/University-Masters/Thesis'
    processor = ProcessSingleDataframe(drivePath + '/S0/ProcessedCsv/S0_External.csv', target_samples_per_second=90)
    dfmain = processor.preprocessData(pd.read_csv(drivePath + '/S0/ProcessedCsv/S0_External.csv', sep=';'), has_Discrete=True)
    processor.saveProcessedData(dfmain, drivePath + '/S0/S0_external_resampled.csv')
