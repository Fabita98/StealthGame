import math
import os
import numpy as np
import pandas as pd
from scipy import signal
from sklearn.preprocessing import MinMaxScaler
import matplotlib.pyplot as plt
from datetime import time, timedelta

drivePath = r'C:\Users\sosan\OneDrive\Desktop\NewData'

stress_survey_path = 'Annotation'
resampled_path = 'Resampled'
savePath = 'data'
buttonPrefix = 'OVRInput+Raw'

# Funzione per leggere i dati relativi ai soggetti dal drive
def read_subject_data(drive_path):
    subject_data = {}
    for subject_folder in sorted(os.listdir(drive_path)):
        if subject_folder.startswith('S'):
            subject_path = os.path.join(drive_path, subject_folder)
            subject_files = os.listdir(subject_path)
            annotation_file = next((file for file in subject_files if 'stress' in file.lower()), None)
            oculus_file = next((file for file in subject_files if 'oculus' in file.lower()), None)
            if annotation_file and oculus_file:
                annotation_df = pd.read_csv(os.path.join(subject_path, annotation_file), sep=';')
                oculus_df = pd.read_csv(os.path.join(subject_path, oculus_file), sep=';', skiprows=1)
                subject_data[subject_folder] = {'annotation': annotation_df, 'oculus': oculus_df}
    return subject_data

# Funzione per processare i dati di oculus e annotazioni
def process_data(subject_data):
    for subject, data in subject_data.items():
        oculus_df = data['oculus']
        annotation_df = data['annotation']

        print(oculus_df)
        # Preprocessamento dei dati di oculus
        oculus_processed = DivideDatatrackerDataAndPreprocess(oculus_df)

        # Sovracampionamento dei dati delle annotazioni
        annotation_resampled = superSampling(annotation_df, freq=90)

        # Unione dei due DataFrame basata sull'indice
        merged_df = pd.concat([oculus_processed, annotation_resampled], axis=1)

        # Ridimensionamento dei DataFrame per avere la stessa lunghezza
        #merged_df = merged_df.iloc[:min(len(oculus_processed), len(annotation_resampled))]

        # Eliminazione delle prime 100 righe
        merged_df = merged_df.iloc[100:].reset_index(drop = True)

        # Calcolare le correlazioni di Pearson tra tutte le colonne del DataFrame combinato e i dati delle annotazioni
        correlations = merged_df.corrwith(merged_df['Value'], method='pearson')  # Calcola le correlazioni di Pearson con i dati delle annotazioni

        # Selezionare solo le colonne che hanno una correlazione di Pearson maggiore del 50%
        high_correlation_columns = correlations[correlations.abs() > 0.6].index.tolist()

        print(merged_df)

        merged_df['timestampStartFixed'] = (merged_df['timestampStartFixed'] - merged_df['timestampStartFixed'].min()).dt.total_seconds()

        # Iterare sulle colonne con alta correlazione e tracciare i grafici
        for column in high_correlation_columns:
            if column != 'timestampStartFixed' and column != 'Timestamp':  # Ignora la colonna del timestamp
                # Creazione di un nuovo plot
                plt.figure(figsize=(10, 6))

                # Grafico della colonna del DataFrame combinato
                plt.plot(merged_df['timestampStartFixed'], merged_df[column], label=f'{column} (Oculus)')
                
                # Grafico delle annotazioni sovracampionate
                plt.plot(merged_df['timestampStartFixed'], merged_df['Value'], label='Stress')
                
                # Personalizzazione del plot
                plt.title(f'Comparison between {column} (Oculus) e Stress annotation')
                plt.xlabel('Time')
                plt.ylabel('Value')
                plt.legend()
                plt.grid(True)
                
                # Mostrare il plot
                plt.show()


class DataType(int):
    def __str__(self):
        if self == 0:
            return 'Face'
        elif self == 1:
            return 'Eye'
        elif self == 2:
            return 'Button'
        elif self == 3:
            return 'Movement'
        elif self == 4:
            return 'Total'
        else:
            return 'Unknown'


def DivideDatatrackerDataAndPreprocess(inputData):
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
    dfTime = pd.DataFrame(inputData.loc[:, inputData.columns.str.startswith('Frame') | inputData.columns.str.startswith('timestamp')])
    dfFace = pd.DataFrame(inputData.loc[:, inputData.columns.isin(faceColumnNames)])
    dfEye = pd.DataFrame(inputData.loc[:, (inputData.columns.str.startswith('Eye') & ~inputData.columns.str.startswith('Eyes')) | inputData.columns.str.startswith('Semantic')])
    dfButtons = pd.DataFrame(inputData.loc[:, inputData.columns.str.startswith(buttonPrefix)])
    dfMovements = pd.DataFrame(inputData.loc[:, (inputData.columns.str.startswith("OVR") & ~inputData.columns.str.startswith(buttonPrefix)) | inputData.columns.str.startswith("Tracker")])
    #dfExternData = ExternalData(inputData)
    #dfs = [pd.concat([dfTime, df], axis='columns') for df in [dfFace, dfEye, dfButtons, dfMovements, dfExternData]]
    dfTot = preprocessData(pd.concat([dfTime, dfFace, dfEye, dfButtons, dfMovements], axis='columns'))
    #dfs.append(dfTot)
    return dfTot

def ExternalData(inputData):
    def TagtoValue(x):
        match x:
            case 'Ammo': return 1
            case 'Oxygen': return 1
            case 'Gun': return 1
            case 'Enemies': return -1
            case _: return 0
    # Extract external data from the input DataFrame
    externalData = ['EyesClosedL', 'EyesClosedR', 'IsGunGrabbed', 'Health', 'Oxygen', 'Deaths', 'LastCheckpoint']
    df = pd.DataFrame(inputData.loc[:, inputData.columns.isin(externalData) | inputData.columns.str.startswith('Semantic') | inputData.columns.str.startswith('OVRNodeHeadPosition')])
    rays = range(0, 10)

    for column in df.columns.values:
        if column.startswith('SemanticTag'):
            df[column] = df[column].apply(lambda x: TagtoValue(x))
    for ray in rays:
        df[f'Distance{ray}'] = np.sqrt(np.square(df[f'SemanticPoint{ray}X'] - df['OVRNodeHeadPositionX']) + np.square(df[f'SemanticPoint{ray}Y'] - df['OVRNodeHeadPositionY']) + np.square(df[f'SemanticPoint{ray}Z'] - df['OVRNodeHeadPositionZ']))
        df.loc[~df[f'SemanticTag{ray}'] == -1, f'Distance{ray}'] = float('nan')
    
    # convertire checkPoint o da zero a n o livello di stress nell'area,
    checkpoint_numeric = convertCheckpointToNumber(df, useStressLevel = False)
    df['LastCheckpoint'] = checkpoint_numeric

    # convertire le morti in valore logaritmico o valore esponenziale
    deaths_converted = convertDeaths(df, useLogarithmic=True, keepOriginal=False)
    df['Deaths'] = deaths_converted

    # convertire vita e ossigeno in soglie circa di 25% da 0 a 1
    ox_life_converted = convertOxygenAndLife(df['Oxygen', 'Life'], threshold = 25, convertToThreshold = True)
    df['Oxygen', 'Life'] = ox_life_converted

    return df

def convertCheckpointToNumber(df: pd.DataFrame, useStressLevel: bool) -> pd.Series:
    # Definizione dei checkpoint e dei rispettivi valori di stress (da decidere)
        checkpoint_to_stress = {
                "baseline_button": 1,
                "startroom_completed": 2,
                "room2_completed": 3,
                "room2": 4
            }
            
        last_checkpoint_series = df["LastCheckpoint"]
        
        if useStressLevel:
            # Mappa il livello di stress
            return last_checkpoint_series.map(checkpoint_to_stress).fillna(0)  # Riempi i checkpoint mancanti con 0
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
    
def convertOxygenAndLife(df: pd.DataFrame, threshold: float, convertToThreshold: bool) -> pd.DataFrame:
    if convertToThreshold:
        # converti vita e ossigeno seguendo una soglia
        return df.apply(lambda x: np.floor(x / threshold))
    else:
        # converti vita e ossigeno in valori tra 0 e 1
        return df.apply(lambda x: x / 100)

def preprocessData(df, isDiscrete=False, target_samples_per_second=90):
    # Rimuovo le colonne contenenti "Frame" o "timestamp" nel nome
    # tutte tranne quella relativa al timestamp che ci serve (NON SONO SICURA SIA QUELLA GIUSTA)
    columns_to_drop = [col for col in df.columns if "Frame" in col or "timestamp" in col and col != "timestampStartFixed"]
    df = df.drop(columns=columns_to_drop)
    
    time_column = "timestampStartFixed"
    
    # Calcolo del campionamento in base al numero di campioni desiderati al secondo
    sample_interval = pd.to_timedelta(1 / target_samples_per_second, unit="s")

    # Ricampionamento dei dati
    if isDiscrete:
        # SIAMO SICURI CHE TUTTI I VALORI DEI DATI ESTERNI SIANO DISCRETI???

        start_time = pd.to_datetime(df[time_column].iloc[0])
        end_time = pd.to_datetime(df[time_column].iloc[-1])
        new_index = pd.date_range(start=start_time, end=end_time, freq=sample_interval)
        
        df_resampled = df.set_index(time_column).reindex(new_index)
        # Ripetizione dei valori precedenti per i campioni mancanti
        df_resampled = df_resampled.ffill()

    else:
        df[time_column] = pd.to_datetime(df[time_column])  
        df = df.set_index(time_column)
        df_resampled = df.resample(sample_interval).mean()
        # Per gestire eventuali valori mancanti, puoi utilizzare il metodo interpolate
        df_resampled = df_resampled.interpolate(method='time', limit_direction='forward')
        # Reset dell'indice temporale
        df_resampled = df_resampled.reset_index()
        
        # Normalizzazione min-max
        scaler = MinMaxScaler()
        df_resampled.iloc[:, 1:] = scaler.fit_transform(df_resampled.iloc[:, 1:])

        return df_resampled
    
def superSampling(df,freq):
    #super campionamento
    values = df['Value']
    resampled_values = signal.resample(values, math.ceil((len(values)/50)*freq))
    resampled_timestamp = np.linspace(df['TimeStamp'].iloc[0], df['TimeStamp'].iloc[-1], len(resampled_values))
    scaler = MinMaxScaler(feature_range=(0, 1))
    resampled_values = scaler.fit_transform(pd.DataFrame(resampled_values))
    resampled_values = resampled_values.flatten()
    return pd.DataFrame({'TimeStamp':resampled_timestamp, 'Value':resampled_values})

def main():
    subject_data = read_subject_data(drivePath)

    process_data(subject_data)

if __name__ == "__main__":
    main()
