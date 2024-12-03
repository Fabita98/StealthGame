from __future__ import annotations
import os
import pandas as pd
import os

buttonPrefix = 'Controller_'
'''
buttonsToKeep = ['OVRInput+RawAxis2DLThumbstickY', 'OVRInput+RawAxis2DLThumbstickX', 'OVRInput+RawAxis1DRThumbRestForce',OVRInput+RawAxis1DRIndexTriggerSlide,OVRInput+RawAxis1DRIndexTriggerCurl,OVRInput+RawAxis1DLThumbRestForce,OVRInput+RawAxis1DRIndexTriggerCurl,OVRInput+RawAxis1DLThumbRestForce,OVRInput+RawAxis1DLIndexTriggerSlide,OVRInput+RawAxis1DLIndexTriggerCurl,
OVRInput+RawAxis1DRHandTrigger,OVRInput+RawAxis1DLHandTrigger,OVRInput+RawAxis1DRIndexTrigger,OVRInput+RawAxis1DLIndexTrigger,OVRInput+RawNearTouchRThumbButtons,OVRInput+RawNearTouchRIndexTrigger,OVRInput+RawNearTouchLThumbButtons,OVRInput+RawNearTouchLIndexTrigger,OVRInput+RawTouchLIndexTrigger,
OVRInput+RawTouchLThumbRest,OVRInput+RawTouchLThumbstick,OVRInput+RawTouchRIndexTrigger,OVRInput+RawTouchRThumbRest,OVRInput+RawTouchRThumbstick,OVRInput+RawButtonLHandTrigger,OVRInput+RawButtonLIndexTrigger,OVRInput+RawButtonRHandTrigger,
OVRInput+RawButtonRIndexTrigger,OVRInput+RawButtonLThumbstick,OVRInput+RawButtonLThumbstickRight,OVRInput+RawButtonLThumbstickLeft,OVRInput+RawButtonLThumbstickDown,OVRInput+RawButtonLThumbstickUp,
'''

class DataType(int):
    def __str__(self):
        if self == 0:
            return 'Face'
        elif self == 1:
            return 'Eye'
        elif self == 2:
            return 'Movement'
        elif self == 3:
            return 'External'
        elif self == 4:
            return 'Button'
        elif self == 5:
            return 'Total'
        else:
            return 'Unknown'



class DivideDataframe:
    defaultExternalData = ['HeartBeatRate', 'MaxHeartBeatRate', 'MinHeartBeatRate', 'AverageHeartBeatRate', 'IsInStressfulArea', 'Deaths', 'LastCheckpoint']

    def __init__(self, path, external_data=defaultExternalData, subjects_to_exclude=[]):
        self.path = path
        self.external_data = external_data
        self.subjects_to_exclude = subjects_to_exclude


    def processFileOnPath(self, path, hasHeader=True):
        dirs = sorted(list(filter(lambda x: x[0] == 'S', os.listdir(path))), key=lambda x: int(x[1:]))
        dirs = [dr for dr in dirs if dr not in self.subjects_to_exclude]
        count = 0
        for dir in dirs:
            templist = list(filter(lambda f: f == f"{dir}.csv", os.listdir(os.path.join(path, dir))))
            f = templist[0] if len(templist) > 0 else None
            if f is None:
                continue
            if hasHeader:
                df = pd.read_csv(os.path.join(path, dir, f), sep=';', low_memory=False, skiprows=1)
            else:
                df = pd.read_csv(os.path.join(path, dir, f), sep=';', low_memory=False)
            dfs = self.DivideDatatrackerData(df)
            n = 0
            for i in dfs:
                self.SaveDataframesInFile(i, os.path.join(path, os.path.join(dir, "ProcessedCsv")),
                                        f"{dir}_{DataType(n)}.csv")
                print(f"{dir}_{DataType(n)}.csv")
                n += 1
            count += 1
        print(f'Processed {count} files')


    def SaveDataframesInFile(self, dataframe, subject_folder, name):
        if not os.path.exists(subject_folder):
            os.makedirs(subject_folder)
        dataframe.to_csv(os.path.join(subject_folder, name), index=False, sep=';')


    def DivideDatatrackerData(self, inputData):
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
        dfFace = pd.DataFrame(inputData.loc[:, inputData.columns.isin(faceColumnNames)])
        dfFace = dfFace.loc[:, ~(dfFace.fillna(0) == 0).all(axis=0)]
        dfEye = pd.DataFrame(inputData.loc[:, (inputData.columns.str.startswith('Eye') & ~inputData.columns.str.startswith(
            'Eyes'))])
        dfEye = dfEye.loc[:, ~(dfEye.fillna(0) == 0).all(axis=0)]
        dfButtons = pd.DataFrame(inputData.loc[:, inputData.columns.str.startswith(buttonPrefix)])
        dfButtons = dfButtons.loc[:, ~(dfButtons.fillna(0) == 0).all(axis=0)]
        dfMovements = pd.DataFrame(inputData.loc[:, (inputData.columns.str.startswith(
            "OVR") & ~inputData.columns.str.startswith(buttonPrefix))])
        dfMovements = dfMovements.loc[:, ~(dfMovements.fillna(0) == 0).all(axis=0)]
        dfExternData = self.ExternalData(inputData)
        dfs = [pd.concat([dfTime, df], axis='columns') for df in [dfFace, dfEye, dfMovements, dfExternData, dfButtons]]
        return dfs


    def ExternalData(self, inputData):
        # Extract external data from the input DataFrame
        df = pd.DataFrame(inputData.loc[:, inputData.columns.isin(self.external_data) | 
                                        #~ inputData.columns.str.startswith('Semantic') | 
                                        inputData.columns.str.startswith('OVRNodeHeadPosition')])
        #~ rays = range(0, 10)
        #~ SemanticTags = ["SemanticTag" + str(i) for i in rays]
        #~ for tag in SemanticTags:
        #~     df[tag] = df[tag].apply(TagtoValue)
        columns_to_drop = [col for col in df.columns if "OVRNode" in col]
        df = df.drop(columns=columns_to_drop)
        return df

    def main(self):
        self.processFileOnPath(self.path)
    

if __name__ == "__main__":
    
    drivePath = "D:/University-Masters/Thesis"
    externalData = ['HeartBeatRate', 'MaxHeartBeatRate', 'MinHeartBeatRate', 'AverageHeartBeatRate', 'IsInStressfulArea', 'Deaths', 'LastCheckpoint']
    divideDataframe = DivideDataframe(path=drivePath, external_data=externalData)
    divideDataframe.main()

