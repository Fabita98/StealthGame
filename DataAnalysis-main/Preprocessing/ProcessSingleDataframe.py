from __future__ import annotations
import os
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from enum import Enum
from scipy import signal
import math
from sklearn.preprocessing import MinMaxScaler

from scipy.interpolate import interp1d
from datetime import datetime as dt, timedelta
import os

drivePath = 'D:/University-Masters/Thesis'

#drivePath = "G:/.shortcut-targets-by-id/1wNGSxajmNG6X6ORVLNxGRZkrJJVw02FA/Test"
def processMovement(path):
    df = pd.read_csv(path,sep=';')
    #print(df.columns)# Sostituisci con il percorso giusto
    return preprocessData(df)

def processButton(path):
    df = pd.read_csv(path,sep=';')
    return preprocessData(df)


def processEye(path):
    df = pd.read_csv(path,sep=';')  # Sostituisci con il percorso giusto
    return preprocessData(df)

def processFace(path):
    df = pd.read_csv(path,sep=';')  # Sostituisci con il percorso giusto
    return preprocessData(df)


def processExternal(path):
    df = pd.read_csv(path,sep=';')
    return preprocessData(df, has_Discrete=True)


def preprocessData(df, has_Discrete=False, target_samples_per_second=90):
    # Rimuovo le colonne contenenti "Frame" o "timestamp" nel nome
    # tutte tranne quella relativa al timestamp che ci serve (NON SONO SICURA SIA QUELLA GIUSTA)
    '''
    columns_to_drop = [col for col in df.columns if
                       "Frame" in col or "timestamp" in col and "timestampUnityTime" not in col]
    df = df.drop(columns=columns_to_drop)
    df = df.drop(columns=['timestampUnityTimeDifference'])
    '''
    time_column = "timestampUnityTime"

    # Calcolo del campionamento in base al numero di campioni desiderati al secondo
    sample_interval = pd.Timedelta(seconds=1 / target_samples_per_second)
    # Ricampionamento dei dati
    if has_Discrete:
        #print(df.columns)
        # SIAMO SICURI CHE TUTTI I VALORI DEI DATI ESTERNI SIANO DISCRETI???

        # start_time = pd.to_datetime(df[time_column].iloc[0])
        # end_time = pd.to_datetime(df[time_column].iloc[-1])
        # new_index = pd.date_range(start=start_time, end=end_time, freq=sample_interval)
        df[time_column] = pd.to_datetime(df[time_column], unit='s')
        discrete_data = ['HeartBeatRate', 'MaxHeartBeatRate', 'MinHeartBeatRate', 'AverageHeartBeatRate', 'IsInStressfulArea', 'Deaths', 'LastCheckpoint']

        #~ discrete_data = ["LastCheckpoint", "Health", "IsGunGrabbed", "Deaths", "Oxygen", 'Frogger', 'Walker', 'Shield','BlackHole', 'BossWalker']
        discrete_data.extend([col for col in df.columns if "SemanticTag" in col or "SemanticObj" in col])
        discrete_data.extend([col for col in df.columns if "checkpoint" in col])

        df_resampled = df.set_index(time_column)
        for col in df_resampled.columns:
            if col in discrete_data:
                #print(col + " discrete")
                df_resampled[col] = df_resampled[col].ffill()
            else:
                #print(col + " continuous")
                df_resampled[col] = df_resampled[col].interpolate(method='linear', limit_direction='forward')

        df_resampled = df_resampled.reset_index()

    else:
        df[time_column] = pd.to_datetime(df[time_column], unit='s')
        df = df.set_index(time_column)
        df_resampled = df.resample(sample_interval).mean()
        # df_resampled.to_csv('/mnt/c/Users/franc/PycharmProjects/DataAnalysis/Preprocessing/data/S4'
        #                     '/S4_Button_middle_resampled.csv', index=False, sep=';')
        # Per gestire evenutali valori mancanti
        df_resampled = df_resampled.interpolate(method='linear', limit_direction='forward')
        df_resampled = df_resampled.reset_index()
    return df_resampled


if __name__ == "__main__":
    dfmain = preprocessData(pd.read_csv(drivePath + '/S0/ProcessedCsv/S0_External.csv', sep=';'), has_Discrete=True)
    dfmain.to_csv(drivePath + '/S0/S0_external_resampled.csv',
                  index=False, sep=';')
