from Preprocessing.ProcessSingleDataframe import ProcessSingleDataframe
import os

class ProcessDataFrames:
    default_discrete_data = ['HeartBeatRate', 'MaxHeartBeatRate', 'MinHeartBeatRate', 'AverageHeartBeatRate', 'IsInStressfulArea', 'Deaths', 'LastCheckpoint']

    def __init__(self, path='D:/University-Masters/Thesis', discrete_data=default_discrete_data, target_samples_per_second=90,
                 process_movement:bool=True, process_eye:bool=True, process_face:bool=True, process_external:bool=True, process_button:bool=True, subjects_to_exclude=[]):
        self.path = path
        self.discrete_data = discrete_data
        self.target_samples_per_second = target_samples_per_second
        self.process_movement = process_movement
        self.process_eye = process_eye
        self.process_face = process_face
        self.process_external = process_external
        self.process_button = process_button
        self.subjects_to_exclude = subjects_to_exclude

    def process_dataframes(self):
        dirs = sorted(list(filter(lambda x: x[0] == 'S', os.listdir(self.path))), key=lambda x: int(x[1:]))
        dirs = [dr for dr in dirs if dr not in self.subjects_to_exclude]
        
        for dr in dirs:
            print(f"Processing {dr}...")
            processedPath = self.path + '/' + dr + "/ProcessedCsv/"
            ResampledPath = self.path + '/' + dr + "/ResampledCsv/"
            processSingleDataframe = ProcessSingleDataframe(path=processedPath, discrete_data=self.discrete_data, target_samples_per_second=self.target_samples_per_second)
            if self.process_movement[0]:
                df = processSingleDataframe.processMovement(unique_path = f"{dr}_Movement.csv").iloc[100:]
                self.save_dataframes_in_file(df, ResampledPath, f"{dr}_Movement.csv")
            if self.process_button[0]:
                df = processSingleDataframe.processButton(unique_path = f"{dr}_Button.csv").iloc[100:]
                self.save_dataframes_in_file(df, ResampledPath, f"{dr}_Button.csv")
            if self.process_eye[0]:
                df = processSingleDataframe.processEye(unique_path = f"{dr}_Eye.csv").iloc[100:]
                self.save_dataframes_in_file(df, ResampledPath, f"{dr}_Eye.csv")
            if self.process_face[0]:
                df = processSingleDataframe.processFace(unique_path = f"{dr}_Face.csv").iloc[100:]
                self.save_dataframes_in_file(df, ResampledPath, f"{dr}_Face.csv")
            if self.process_external[0]:
                df = processSingleDataframe.processExternal(unique_path = f"{dr}_External.csv").iloc[100:]
                self.save_dataframes_in_file(df, ResampledPath, f"{dr}_External.csv")
            print(f"{dr} done")
    
    def save_dataframes_in_file(self, dataframe, subject_folder, name):
        if not os.path.exists(subject_folder):
            os.makedirs(subject_folder)
        dataframe.to_csv(os.path.join(subject_folder, name), index=False, sep=';')

    def main(self):
        self.process_dataframes()


if __name__ == "__main__":
    drivePath = "D:/University-Masters/Thesis"
    discreteData = ['HeartBeatRate', 'MaxHeartBeatRate', 'MinHeartBeatRate', 'AverageHeartBeatRate', 'IsInStressfulArea', 'Deaths', 'LastCheckpoint']
    processDataframes = ProcessDataFrames(path=drivePath, 
                                          discrete_data=discreteData, 
                                          target_samples_per_second=90,
                                          process_movement=True, 
                                          process_eye=True, 
                                          process_face=True, 
                                          process_external=True, 
                                          process_button=True)
    processDataframes.main()
