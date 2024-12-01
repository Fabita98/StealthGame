from Preprocessing.ProcessSingleDataframe import ProcessSingleDataframe
import os

class ProcessDataFrames:
    default_discrete_data = ['HeartBeatRate', 'MaxHeartBeatRate', 'MinHeartBeatRate', 'AverageHeartBeatRate', 'IsInStressfulArea', 'Deaths', 'LastCheckpoint']

    def __init__(self, drive_path='D:/University-Masters/Thesis', discrete_data=default_discrete_data, target_samples_per_second=90,
                 process_movement=True, process_eye=True, process_face=True, process_external=True, process_button=True):
        self.drive_path = drive_path
        self.discrete_data = discrete_data
        self.target_samples_per_second = target_samples_per_second
        self.process_movement = process_movement
        self.process_eye = process_eye
        self.process_face = process_face
        self.process_external = process_external
        self.process_button = process_button

    def process_dataframes(self):
        path = self.drive_path + '/' + dr + "/ProcessedCsv/"
        processSingleDataframe = ProcessSingleDataframe(path=path, discrete_data=self.discrete_data, target_samples_per_second=self.target_samples_per_second)
        dirs = sorted(list(filter(lambda x: x[0] == 'S', os.listdir(path))), key=lambda x: int(x[1:]))
        
        for dr in dirs:
            if self.process_movement:
                df = processSingleDataframe.processMovement(unique_path = f"{dr}_Movement.csv").iloc[100:]
                self.save_dataframes_in_file(df, path + '/' + dr + "/ResampledCsv", f"{dr}_Movement.csv")
            if self.process_button:
                df = processSingleDataframe.process_button(unique_path = f"{dr}_Button.csv").iloc[100:]
                self.save_dataframes_in_file(df, path + '/' + dr + "/ResampledCsv", f"{dr}_Button.csv")
            if self.process_eye:
                df = processSingleDataframe.processEye(unique_path = f"{dr}_Eye.csv").iloc[100:]
                self.save_dataframes_in_file(df, path + '/' + dr + "/ResampledCsv", f"{dr}_Eye.csv")
            if self.process_face:
                df = processSingleDataframe.processFace(unique_path = f"{dr}_Face.csv").iloc[100:]
                self.save_dataframes_in_file(df, path + '/' + dr + "/ResampledCsv", f"{dr}_Face.csv")
            if self.process_external:
                df = processSingleDataframe.processExternal(unique_path = f"{dr}_External.csv").iloc[100:]
                self.save_dataframes_in_file(df, path + '/' + dr + "/ResampledCsv", f"{dr}_External.csv")
            print(f"{dr} done")
    
    def save_dataframes_in_file(self, dataframe, subject_folder, name):
        if not os.path.exists(subject_folder):
            os.makedirs(subject_folder)
        dataframe.to_csv(os.path.join(subject_folder, name), index=False, sep=';')

    def main(self):
        self.process_dataframes(process_movement=True, process_eye=True, process_face=True, process_external=True, process_button=False, path='D:/University-Masters/Thesis')


if __name__ == "__main__":
    processor = ProcessDataFrames(drive_path='D:/University-Masters/Thesis')
    processor.process_dataframes(process_movement=True, process_eye=True, process_face=True, process_external=True, process_button=False, path='D:/University-Masters/Thesis')
