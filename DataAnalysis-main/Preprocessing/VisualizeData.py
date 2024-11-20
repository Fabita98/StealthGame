from ProcessSingleDataframe import processMovement, processEye, processFace, processExternal
from DataSynchronization import synchronize
import os
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
drivePath = 'D:/University-Masters/Thesis'
AlienPath = 'F:/Data_Analysis'

def saveColumns(process_movement=True, process_eye=True, process_face=True, process_button=True, process_external = True,
                path=drivePath):
    dirs = sorted(list(filter(lambda x: x[0] == 'S', os.listdir(path))), key=lambda x: int(x[1:]))
    for dr in dirs:
        if process_movement:
            df = processMovement(path + '/' + dr + "/ProcessedCsv/" + f"{dr}_Movement.csv")
            plot_column_graphs(df,path + '/' + dr)
        if process_button:
            df = processMovement(path + '/' + dr + "/ProcessedCsv/" + f"{dr}_Button.csv")
            plot_column_graphs(df,path + '/' + dr)
        if process_eye:
            df = processEye(path + '/' + dr + "/ProcessedCsv/" + f"{dr}_Eye.csv")
            plot_column_graphs(df,path + '/' + dr)
        if process_face:
            df = processFace(path + '/' + dr + "/ProcessedCsv/" + f"{dr}_Face.csv")
            plot_column_graphs(df,path + '/' + dr)
        
        if process_external:
            df = processExternal(path + '/' + dr + "/ProcessedCsv/" + f"{dr}_External.csv")
            plot_column_graphs(df,path + '/' + dr)
        print(f"{dr} done")

def plot_column_graphs(df, path):
    for column in df.columns:
        if column != 'timestampUnityTime':
            plt.figure(figsize=(10, 6))
            plt.plot(df.index, df[column])
            plt.title(f'{column}')
            plt.xlabel('Index')
            plt.ylabel('Value')
            if not os.path.exists(path + '/Columns_plots'):
                os.makedirs(path + '/Columns_plots')
            plt.savefig(f'{path}/Columns_plots/{column}.png')
            plt.close()


if __name__ == "__main__":
    saveColumns(process_movement=True, process_eye=True, process_face=True,process_button=False,path=drivePath)