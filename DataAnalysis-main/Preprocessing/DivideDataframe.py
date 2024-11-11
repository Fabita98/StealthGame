from __future__ import annotations
import os
import sys

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
AlienPath = 'F:/Data_Analysis'
savePath = 'data'
stress_survey_path = 'Annotation'
resampled_path = 'Resampled'
buttonPrefix = 'OVRInput+Raw'
'''
buttonsToKeep = ['OVRInput+RawAxis2DLThumbstickY', 'OVRInput+RawAxis2DLThumbstickX', 'OVRInput+RawAxis1DRThumbRestForce',OVRInput+RawAxis1DRIndexTriggerSlide,OVRInput+RawAxis1DRIndexTriggerCurl,OVRInput+RawAxis1DLThumbRestForce,OVRInput+RawAxis1DRIndexTriggerCurl,OVRInput+RawAxis1DLThumbRestForce,OVRInput+RawAxis1DLIndexTriggerSlide,OVRInput+RawAxis1DLIndexTriggerCurl,
OVRInput+RawAxis1DRHandTrigger,OVRInput+RawAxis1DLHandTrigger,OVRInput+RawAxis1DRIndexTrigger,OVRInput+RawAxis1DLIndexTrigger,OVRInput+RawNearTouchRThumbButtons,OVRInput+RawNearTouchRIndexTrigger,OVRInput+RawNearTouchLThumbButtons,OVRInput+RawNearTouchLIndexTrigger,OVRInput+RawTouchLIndexTrigger,
OVRInput+RawTouchLThumbRest,OVRInput+RawTouchLThumbstick,OVRInput+RawTouchRIndexTrigger,OVRInput+RawTouchRThumbRest,OVRInput+RawTouchRThumbstick,OVRInput+RawButtonLHandTrigger,OVRInput+RawButtonLIndexTrigger,OVRInput+RawButtonRHandTrigger,
OVRInput+RawButtonRIndexTrigger,OVRInput+RawButtonLThumbstick,OVRInput+RawButtonLThumbstickRight,OVRInput+RawButtonLThumbstickLeft,OVRInput+RawButtonLThumbstickDown,OVRInput+RawButtonLThumbstickUp,
'''

class DataType(int):
    def __str__(self):
        if self == 0:
            return 'Eye'
        elif self == 1:
            return 'Button'
        elif self == 2:
            return 'Movement'
        elif self == 3:
            return 'External'
        elif self == 4:
            return 'Total'
        else:
            return 'Unknown'
        #~ if self == 0:
        #~     return 'Face'
        #~ elif self == 1:
        #~     return 'Eye'
        #~ elif self == 2:
        #~     return 'Button'
        #~ elif self == 3:
        #~     return 'Movement'
        #~ elif self == 4:
        #~     return 'External'
        #~ elif self == 5:
        #~     return 'Total'
        #~ else:
        #~     return 'Unknown'


def processFileOnPath(hasHeader=True, path = drivePath):
    dirs = sorted(list(filter(lambda x: x[0] == 'S', os.listdir(path))), key=lambda x: int(x[1:]))
    count = 0
    for dir in dirs:
        templist = list(filter(lambda f: f == f"S{count}.csv", os.listdir(os.path.join(path, dir))))
        f = templist[0] if len(templist) > 0 else None
        if f is None:
            continue
        if hasHeader:
            df = pd.read_csv(os.path.join(path, dir, f), sep=';', low_memory=False, skiprows=1)
        else:
            df = pd.read_csv(os.path.join(path, dir, f), sep=';', low_memory=False)

        dfs = DivideDatatrackerData(df)

        # Per stampare i grafici 
        '''
        for df in dfs:
            if df is not dfExternData:
                plot_column_graphs(df)
        '''

        n = 0
        for i in dfs:

            save_dataframes_in_file(i, os.path.join(path, os.path.join(dir, "ProcessedCsv")),
                                    f"{dir}_{DataType(n)}.csv")
            #print(i)
            print(f"{dir}_{DataType(n)}.csv")

            n += 1
        count += 1
    print(f'Processed {count} files')


def save_dataframes_in_file(dataframe, subject_folder, name):
    if not os.path.exists(subject_folder):
        os.makedirs(subject_folder)

    dataframe.to_csv(os.path.join(subject_folder, name), index=False, sep=';')




            # Vanno salvate nel drive


def DivideDatatrackerData(inputData):
    # Togliere dati timestamp inutili
    inputData= inputData.drop(columns=[col for col in inputData.columns
                                        if
                                        ("Frame" in col or "timestamp" in col or col == 'timestampUnityTimeDifference')
                                        and "timestampUnityTime" not in col])
    #drop if all values in columns are zeros or NaN


    # Divide input DataFrame into different categories/dataType based on prefixes, columns name, etc
    faceColumnNames = ['BrowLowererL', 'BrowLowererR', 'CheekPuffL', 'CheekPuffR', 'CheekRaiserL', 'CheekRaiserR',
                       'CheekSuckL', 'CheekSuckR', 'ChinRaiserB', 'ChinRaiserT', 'DimplerL', 'DimplerR',
                       'EyesClosedL', 'EyesClosedR', 'EyesLookDownL', 'EyesLookDownR', 'EyesLookLeftL',
                       'EyesLookLeftR', 'EyesLookRightL', 'EyesLookRightR', 'EyesLookUpL', 'EyesLookUpR',
                       'InnerBrowRaiserL', 'InnerBrowRaiserR', 'JawDrop', 'JawSidewaysLeft', 'JawSidewaysRight',
                       'JawThrust', 'LidTightenerL', 'LidTightenerR', 'LipCornerDepressorL', 'LipCornerDepressorR',
                       'LipCornerPullerL', 'LipCornerPullerR', 'LipFunnelerLB', 'LipFunnelerLT', 'LipFunnelerRB',
                       'LipFunnelerRT', 'LipPressorL', 'LipPressorR', 'LipPuckerL', 'LipPuckerR', 'LipStretcherL',
                       'LipStretcherR', 'LipSuckLB', 'LipSuckLT', 'LipSuckRB', 'LipSuckRT', 'LipTightenerL',
                       'LipTightenerR', 'LipsToward', 'LowerLipDepressorL', 'LowerLipDepressorR', 'MouthLeft',
                       'MouthRight', 'NoseWrinklerL', 'NoseWrinklerR', 'OuterBrowRaiserL', 'OuterBrowRaiserR',
                       'UpperLidRaiserL', 'UpperLidRaiserR', 'UpperLipRaiserL', 'UpperLipRaiserR']

    dfTime = pd.DataFrame(
        inputData.loc[:, inputData.columns.str.startswith('Frame') | inputData.columns.str.startswith('timestamp')])
    #~ dfFace = pd.DataFrame(inputData.loc[:, inputData.columns.isin(faceColumnNames)])
    #~ dfFace = dfFace.loc[:, ~(dfFace.fillna(0) == 0).all(axis=0)]
    dfEye = pd.DataFrame(inputData.loc[:, (inputData.columns.str.startswith('Eye') & ~inputData.columns.str.startswith(
        'Eyes'))])
    dfEye = dfEye.loc[:, ~(dfEye.fillna(0) == 0).all(axis=0)]
    dfButtons = pd.DataFrame(inputData.loc[:, inputData.columns.str.startswith(buttonPrefix)])
    dfButtons = dfButtons.loc[:, ~(dfButtons.fillna(0) == 0).all(axis=0)]
    dfMovements = pd.DataFrame(inputData.loc[:, (inputData.columns.str.startswith(
        "OVR") & ~inputData.columns.str.startswith(buttonPrefix))])
    dfMovements = dfMovements.loc[:, ~(dfMovements.fillna(0) == 0).all(axis=0)]
    dfExternData = ExternalData(inputData)
    dfs = [pd.concat([dfTime, df], axis='columns') for df in [dfEye, dfButtons, dfMovements, dfExternData]]
    
    #~ dfs = [pd.concat([dfTime, df], axis='columns') for df in [dfFace, dfEye, dfButtons, dfMovements, dfExternData]]
    #dfTot = pd.concat([dfTime, dfFace, dfEye, dfButtons, dfMovements, dfExternData], axis='columns')
    #dfs.append(dfTot)

    return dfs


def EnemiesTracker(df, inputData, threshold=75):
    enemies_names = [x for x in inputData.columns.values if x.startswith('Tracker_') and x.endswith('_PositionX')]
    enemies_names = sorted([x.replace('Tracker_', '').replace('_PositionX', '') for x in enemies_names])
    Enemytags = ['Frogger', 'Walker', 'Shield', 'BlackHole', 'BossWalker']
    for enemy in enemies_names:
        df[enemy + '_distance'] = np.sqrt(
            (inputData[f'Tracker_{enemy}_PositionX'] - inputData['OVRNodeHeadPositionX']) ** 2 +
            (inputData[f'Tracker_{enemy}_PositionY'] - inputData['OVRNodeHeadPositionY']) ** 2 +
            (inputData[f'Tracker_{enemy}_PositionZ'] - inputData['OVRNodeHeadPositionZ']) ** 2
        )
        df[enemy + '_distance'] = df[enemy + '_distance'].where(df[enemy + '_distance'] < threshold, np.nan)

    observedObjects = [x for x in inputData.columns.values if x.startswith('SemanticObj')]
    for tag in Enemytags:

        if tag == 'Walker':
            # df[tag] =  somma del numero di volte in cui il solo tag appare nelle colonne di observedObjects
            df[tag] = inputData[observedObjects].apply(lambda x: sum([1 for cell in set(x) if pd.notna(
                cell) and tag in cell and 'Bosswalker' not in cell and 'Shield' not in cell]), axis=1)
        else:
            df[tag] = inputData[observedObjects].apply(lambda x: pd.Series([str(i) for i in set(x.astype(str))]).str.contains(tag).sum(), axis=1)





def ExternalData(inputData):
    def TagtoValue(x):
        tag_to_value = {
            # 'IsInStressfulArea': -1,
            # 'Ammo': 1,
            # 'Oxygen': 1,
            # 'Gun': 1,
            # 'Enemies': -1
        }
        return tag_to_value.get(x, 0)

    # Extract external data from the input DataFrame
    #~ externalData = ['EyesClosedL', 'EyesClosedR', 'IsGunGrabbed', 'Health', 'Oxygen', 'Deaths', 'LastCheckpoint']
    externalData = ['HeartBeatRate', 'MaxHeartBeatRate', 'MinHeartBeatRate', 'AverageHeartBeatRate', 'IsInStressfulArea']
    #~ df = pd.DataFrame(inputData.loc[:, inputData.columns.isin(externalData) | inputData.columns.str.startswith(
    #     'Semantic') | inputData.columns.str.startswith('OVRNodeHeadPosition')])
    df = pd.DataFrame(inputData.loc[:, inputData.columns.isin(externalData)])
    
    #~ rays = range(0, 10)
    #~ SemanticTags = ["SemanticTag" + str(i) for i in rays]
    #~ for tag in SemanticTags:
    #~     df[tag] = df[tag].apply(TagtoValue)

    """
    for ray in rays:
        df[f'Distance{ray}'] = np.sqrt(np.square(df[f'SemanticPoint{ray}X'] - df['OVRNodeHeadPositionX']) + np.square(
            df[f'SemanticPoint{ray}Y'] - df['OVRNodeHeadPositionY']) + np.square(
            df[f'SemanticPoint{ray}Z'] - df['OVRNodeHeadPositionZ']))
        df.loc[~df[f'SemanticTag{ray}'] == -1, f'Distance{ray}'] = float('nan')
    """
    # convertire checkPoint o da zero a n o livello di stress nell'area,
    #~ convertCheckpointToNumber(df, useStressLevel=True)
    #~ df.drop(columns=['LastCheckpoint'], inplace=True)
    #df['LastCheckpoint'] = checkpoint_numeric

    # convertire le morti in valore logaritmico o valore esponenziale
    #~ deaths_converted = convertDeaths(df, useLogarithmic=False, keepOriginal=True)
    #~ df['Deaths'] = deaths_converted

    # convertire vita e ossigeno in soglie circa di 25% da 0 a 1
    #~ ox_converted = convertOxygenAndHealth(df['Oxygen'], threshold=25, convertToThreshold=True)
    #~ df['Oxygen'] = ox_converted
    #~ health_converted = convertOxygenAndHealth(df['Health'], threshold=25, convertToThreshold=True)
    #~ df['Health'] = health_converted
    #~ df['IsGunGrabbed'] = df['IsGunGrabbed'].apply(lambda x: 1 if x == 'True' else 0)
    #~ EnemiesTracker(df, inputData)
    # Rimuovere colonne non necessarie
    #~ columns_to_drop = [col for col in df.columns if "SemanticPoint" in col or "SemanticObj" in col or "OVRNode" in col]
    columns_to_drop = [col for col in df.columns if "OVRNode" in col]
    df = df.drop(columns=columns_to_drop)

    return df


def convertCheckpointToNumber(df: pd.DataFrame, useStressLevel: bool) -> pd.Series:
    # Definizione dei checkpoint e dei rispettivi valori di stress (da decidere)
    checkpoint_to_stress = {
        "baseline_button": 1,
        "startroom_completed": 2,
        "room3_completed": 3,
        "room2_completed": 4,
        "room2": 4,
        "map_puzzle_start": 5,
        "MAP_PUZZLE_COMPLETED": 6,
        "room5_completed": 7,
        "EscapeRoom": 8,
        "room9_completed": 9,
        "bossfight_start": 10,
        "bossfight_completed": 11
    }

    last_checkpoint_series = df["LastCheckpoint"]

    if useStressLevel:
        # Mappa il livello di stress
        allcheckpoints = last_checkpoint_series.unique()

        allcheckpoints = [x for x in allcheckpoints if pd.notnull(x)]
        for checkpoint in allcheckpoints:
            df[checkpoint + "_checkpoint"] = last_checkpoint_series.apply(lambda x: 1 if x == checkpoint else 0)

    else:
        checkpoint_to_number = {}
        current_number = 1

        for checkpoint in last_checkpoint_series.unique():
            if pd.notnull(checkpoint):
                checkpoint_to_number[checkpoint] = current_number
                current_number += 1
        return last_checkpoint_series.map(checkpoint_to_number).fillna(0)


def convertDeaths(df: pd.DataFrame, useLogarithmic: bool, keepOriginal: bool) -> pd.Series:
    deaths = df["Deaths"]

    if keepOriginal:
        # Mantieni i valori originali del numero di morti
        return deaths
    elif useLogarithmic:
        # Converti le morti in valore logaritmico
        return np.log(deaths)
    else:
        # Converti le morti in valore esponenziale
        return np.exp(deaths)


def convertOxygenAndHealth(df: pd.DataFrame, threshold: float, convertToThreshold: bool) -> pd.DataFrame:
    if convertToThreshold:
        # converti vita e ossigeno seguendo una soglia
        return (df / threshold).apply(np.floor)
    else:
        # converti vita e ossigeno in valori tra 0 e 1
        return df / 100


def printValues(count, df, name):
    fig, axs = plt.subplots(2, 1)
    axs[0].plot(df['TimeStamp'], df['Value'])
    axs[0].set_title(f'Original {name} soggetto {count}')
    axs[0].set_xlabel('Time')
    axs[0].set_ylabel('Value')
    fig.show()


if __name__ == "__main__":
    processFileOnPath()
    # processFileOnDrive()
