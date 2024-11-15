from ProcessSingleDataframe import processMovement, processEye, processFace, processExternal
import os
import pandas as pd
from DataSynchronization import synchronize
drivePath = 'D:/University-Masters/Thesis'
AlienPath = 'F:/Data_Analysis'
#~ subjects_to_exclude = [f"S{i}" for i in range(0, 2)] + [f"S{i}" for i in range(3, 29)] + ['S25','S100','S101']
def process_dataframes(process_movement=True, process_eye=True, process_face=False, process_external=True,process_button=True,
                       path=AlienPath):
    dirs = sorted(list(filter(lambda x: x[0] == 'S', os.listdir(path))), key=lambda x: int(x[1:]))
    #~ dirs = [dr for dr in dirs if dr not in subjects_to_exclude]
    for dr in dirs:
        if process_movement:
            df = processMovement(path + '/' + dr + "/ProcessedCsv/" + f"{dr}_Movement.csv").iloc[100:]
            # dfDante = pd.read_csv(path + '/' + dr + f"/Resampled_{dr}.csv", sep=';')
            # df = synchronize(df, dfDante)
            save_dataframes_in_file(df, path + '/' + dr + "/ResampledCsv", f"{dr}_Movement.csv")
        if process_button:
            df = processMovement(path + '/' + dr + "/ProcessedCsv/" + f"{dr}_Button.csv").iloc[100:]
            #dfDante = pd.read_csv(path + '/' + dr + f"/Resampled_{dr}.csv", sep=';')
            #df = synchronize(df, dfDante)
            save_dataframes_in_file(df, path + '/' + dr + "/ResampledCsv", f"{dr}_Button.csv")
        if process_eye:
            df = processEye(path + '/' + dr + "/ProcessedCsv/" + f"{dr}_Eye.csv").iloc[100:]
            #dfDante = pd.read_csv(path + '/' + dr + f"/Resampled_{dr}.csv", sep=';')
            #df = synchronize(df, dfDante)
            save_dataframes_in_file(df, path + '/' + dr + "/ResampledCsv", f"{dr}_Eye.csv")
        if process_face:
            df = processFace(path + '/' + dr + "/ProcessedCsv/" + f"{dr}_Face.csv").iloc[100:]
            #dfDante = pd.read_csv(path + '/' + dr + f"/Resampled_{dr}.csv", sep=';')
            #df = synchronize(df, dfDante)
            save_dataframes_in_file(df, path + '/' + dr + "/ResampledCsv", f"{dr}_Face.csv")
        if process_external:
            df = processExternal(path + '/' + dr + "/ProcessedCsv/" + f"{dr}_External.csv").iloc[100:]
            #dfDante = pd.read_csv(path + '/' + dr + f"/Resampled_{dr}.csv", sep=';')
            #df = synchronize(df, dfDante)
            save_dataframes_in_file(df, path + '/' + dr + "/ResampledCsv", f"{dr}_External.csv")
        print(f"{dr} done")
    # Aggiungi altre condizioni per altri tipi di dataframe se necessario

    return

def save_dataframes_in_file(dataframe, subject_folder, name):
    if not os.path.exists(subject_folder):
        os.makedirs(subject_folder)

    dataframe.to_csv(os.path.join(subject_folder, name), index=False, sep=';')



if __name__ == "__main__":

    process_dataframes(process_movement=True, process_eye=True, process_face=True, process_external=True, process_button=True, path=drivePath)
