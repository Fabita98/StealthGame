from Preprocessing.ProcessSingleDataframe import ProcessSingleDataframe
import os
import matplotlib.pyplot as plt

class VisualizeData:
    default_discrete_data = ['HeartBeatRate', 'MaxHeartBeatRate', 'MinHeartBeatRate', 'AverageHeartBeatRate', 'IsInStressfulArea', 'Deaths', 'LastCheckpoint']

    def __init__(self, path='D:/University-Masters/Thesis', process_movement=True, process_eye=True, process_face=True, process_button=True, process_external=True,
                 discrete_data=default_discrete_data, target_samples_per_second=90, subjects_to_exclude=[]):
        self.path = path
        self.process_movement = process_movement
        self.process_eye = process_eye
        self.process_face = process_face
        self.process_button = process_button
        self.process_external = process_external
        self.subjects_to_exclude = subjects_to_exclude
        self.discrete_data = discrete_data
        self.target_samples_per_second = target_samples_per_second

    def save_columns(self):
        processSingleDataframe = ProcessSingleDataframe(path=self.path, discrete_data=self.discrete_data, target_samples_per_second=self.target_samples_per_second)
        dirs = sorted(list(filter(lambda x: x[0] == 'S', os.listdir(self.path))), key=lambda x: int(x[1:]))
        dirs = [dr for dr in dirs if dr not in self.subjects_to_exclude]
        for dr in dirs:
            if self.process_movement:
                df = processSingleDataframe.processMovement(unique_path = f"/{dr}/ProcessedCsv/{dr}_Movement.csv")
                self.plot_column_graphs(df, f"{self.path}/{dr}")
            if self.process_button:
                df = processSingleDataframe.processButton(unique_path =f"/{dr}/ProcessedCsv/{dr}_Button.csv")
                self.plot_column_graphs(df, f"{self.path}/{dr}")
            if self.process_eye:
                df = processSingleDataframe.processEye(unique_path =f"/{dr}/ProcessedCsv/{dr}_Eye.csv")
                self.plot_column_graphs(df, f"{self.path}/{dr}")
            if self.process_face:
                df = processSingleDataframe.processFace(unique_path =f"/{dr}/ProcessedCsv/{dr}_Face.csv")
                self.plot_column_graphs(df, f"{self.path}/{dr}")
            if self.process_external:
                df = processSingleDataframe.processExternal(unique_path =f"/{dr}/ProcessedCsv/{dr}_External.csv")
                self.plot_column_graphs(df, f"{self.path}/{dr}")
            print(f"{dr} done")

    @staticmethod
    def plot_column_graphs(df, path):
        for column in df.columns:
            if column != 'timestampUnityTime':
                plt.figure(figsize=(10, 6))
                plt.plot(df.index, df[column])
                plt.title(f'{column}')
                plt.xlabel('Index')
                plt.ylabel('Value')
                if not os.path.exists(f"{path}/Columns_plots"):
                    os.makedirs(f"{path}/Columns_plots")
                plt.savefig(f"{path}/Columns_plots/{column}.png")
                plt.close()
    def main(self):
        self.save_columns()

if __name__ == "__main__":
    visualizeData = VisualizeData(process_movement=True, process_eye=True, process_face=True, process_button=False)
    visualizeData.save_columns()
