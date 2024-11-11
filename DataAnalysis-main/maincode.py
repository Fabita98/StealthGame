from datamanager import DataManager, DataType
from enum import Enum
import pandas as pd
from sklearn.discriminant_analysis import LinearDiscriminantAnalysis
from sklearn.mixture import GaussianMixture
import os


class LoadType(Enum):
    RAW = "raw"
    RAWPHASES = "rawphases"
    EXTR = "extracted"
    EXTRNORM = "extractedN"
    SEL = "selected"
    TSSEL = "tsselected"
    EXTRONEPHASE = "extractedOnephase"
    EXTRNORMONEPHASE = "extractedNOnephase"

class FolderManager:

    def __init__(self, subNumber : int, attempt : int, excludedSubjects : list):
        #Gestisce tutte le cartelle e come vengono salvati i dati
        #path annotazioni
        self.aPath = "Data\\Annotations"
        #path di tutti i dati raw raccolti
        self.rawDPath = 'Data\\AllData'
        #path delle feature estratte
        self.extDPath = 'Data\\ExtractedData'
        #path delle feature estratte e normalizzate
        self.extDPathNorm = 'Data\\ExtractedDataNorm'
        #path delle feature selezionate
        self.selFPath = 'Data\\SelectedData'
        #path delle feature selezionate e concatenate
        self.selCFPath = 'Data\\SelectedDataConcat'
        self.tsselFPath = 'Data\\TsSelectedData'
        self.tsselCFPath = 'Data\\TsSelectedData'
        #risultato dei modelli
        self.modelPath = 'Data\\ModelsResults'

        self.__CreateFolderIfNotExists(self.extDPathNorm)
        self.__CreateFolderIfNotExists(self.selFPath)
        self.__CreateFolderIfNotExists(self.selCFPath)
        self.__CreateFolderIfNotExists(self.tsselFPath)
        self.__CreateFolderIfNotExists(self.tsselCFPath)
        self.__CreateFolderIfNotExists(self.modelPath)

        #numero dei soggetti totali
        self.subjectN = subNumber
        self.attempt = attempt
        self.tsSelected = None
        self.dataManager = DataManager()

        self.qCluster = pd.DataFrame()
        self.predictions = pd.DataFrame()

        #lista soggetti esclusi
        self.excludedSubjects = excludedSubjects

    def __CreateFolderIfNotExists(self, path):
        if not os.path.exists(path): 
            os.makedirs(path) 

    def ExtractFeaturesAndSave(self, normalize : bool):
        self.dataManager.extractFeatures(4, normalize)
        self.__Save(self.extDPath)

    def __SaveSelected(self, mergedDatatypes : bool):
        if self.tsSelected:
            if(mergedDatatypes):
                self.__Save(self.tsselCFPath)
            else:
                self.__Save(self.tsselFPath)
        else:
            if(mergedDatatypes):
                self.__Save(self.selCFPath)
            else:
                self.__Save(self.selFPath)

    def SelectFeaturesAndSave(self, normalize : bool, mergeDatatypes : bool, pcaComponents : int, varT : float, nCluster : int, verbose : bool):       
        self.dataManager.selectFeatures(normalize=normalize, mergeDatatypes=mergeDatatypes, isTs=self.tsSelected , varianceThreshold=varT, selectedComponentsPCA=pcaComponents, nClusters= nCluster, verbose= verbose)
        self.__SaveSelected(mergeDatatypes)

    def TsPreprocess(self, normalize, preprocesserName, seg, alphSize):
        self.dataManager.ApplyTsPreprocessing(normalize, preprocesserName, seg, alphSize)

    def InitaAnnotationManager(self, useMinMaxNorm):
        #Da implementare AnnotationManager per i dati di DANTE
        #self.qManager = AnnotationManager(self.qPath, useMinMaxNorm, self.excludedSubjects)
        print("to-do")

    def ComputeModelPerformances(self):
        print("to-do")
        #csvPath = os.path.join(self.modelPath, f'{self.attempt}_{name}.csv')
        #df.to_csv(csvPath, sep = ';', index=False)

    def LoadData(self, type : LoadType, allDatatypes : bool, ignoreMissingDatatypes : bool):
        self.dataManager = DataManager()

        print(f"Loading data from {type}")
        match type:
            case LoadType.RAW:
                self.dataManager.load(self.rawDPath, self.subjectN, self.excludedSubjects)
                self.dataManager.preprocessData()
                self.dataManager.splitIntoPhases()
                self.dataManager.MergePhasesForeachDatatypeIntoOnePhase()
                self.RemoveDatatype(DataType.LIPSYNC) #lipsinc data needs to be removed
                self.tsSelected = True

            case LoadType.RAWPHASES:
                self.dataManager.load(self.rawDPath, self.subjectN, self.excludedSubjects)
                self.dataManager.preprocessData()
                self.dataManager.splitIntoPhases()
                self.RemoveDatatype(DataType.LIPSYNC) #lipsinc data needs to be removed
                self.tsSelected = True
                
            case LoadType.EXTR:
                self.dataManager.loadInPhaseForm(self.extDPath, self.subjectN, False, ignoreMissingDatatypes, self.excludedSubjects)
                self.RemoveDatatype(DataType.LIPSYNC) #lipsinc data needs to be removed
                self.tsSelected = False

            case LoadType.EXTRNORM:
                self.dataManager.loadInPhaseForm(self.extDPathNorm, self.subjectN, False, ignoreMissingDatatypes, self.excludedSubjects)
                self.tsSelected = False

            case LoadType.EXTRONEPHASE:
                self.dataManager.loadInPhaseForm(self.extDPath, self.subjectN, False, ignoreMissingDatatypes, self.excludedSubjects)
                self.RemoveDatatype(DataType.LIPSYNC) #lipsinc data needs to be removed
                self.dataManager.MergePhasesForeachDatatypeIntoOnePhase()
                self.tsSelected = False

            case LoadType.EXTRNORMONEPHASE:
                self.dataManager.loadInPhaseForm(self.extDPathNorm, self.subjectN, False, ignoreMissingDatatypes, self.excludedSubjects)
                self.dataManager.MergePhasesForeachDatatypeIntoOnePhase()
                self.tsSelected = False 
            
            case LoadType.SEL:
                if(allDatatypes):
                    print("merged datatypes")
                    self.dataManager.loadInPhaseForm(self.selCFPath, self.subjectN, True, ignoreMissingDatatypes, self.excludedSubjects)
                else:
                    print("separed datatypes")
                    self.dataManager.loadInPhaseForm(self.selFPath, self.subjectN, False, ignoreMissingDatatypes, self.excludedSubjects)
                
                self.tsSelected = False
            
            case LoadType.TSSEL:
                if(allDatatypes):
                    print("merged datatypes")
                    self.dataManager.loadInPhaseForm(self.tsselCFPath, self.subjectN, True, ignoreMissingDatatypes, self.excludedSubjects)
                else:
                    print("separed datatypes")
                    self.dataManager.loadInPhaseForm(self.tsselFPath, self.subjectN, False, ignoreMissingDatatypes, self.excludedSubjects)

                self.tsSelected = True
            case _:
                print("Type of data not available")

    def NormalizeAndSave(self, path):
        self.dataManager.Normalize(self.tsSelected)     
        self.__Save(path)

    def __Save(self, path):
        self.dataManager.saveInPhaseForm(path)

    def RemoveDatatype(self, dataType : DataType):
        self.dataManager.RemoveDatatype(dataType)

    def RemovePhase(self, phase : float):
        self.dataManager.RemovePhase(phase)

class ModelType(Enum):
    print("to-do")

def NormalizeEXTData():
    flManager = FolderManager(21)
    flManager.LoadData(LoadType.EXTR, False)
    flManager.NormalizeAndSave(flManager.extDPathNorm)

def TimeSeriesTest():
    flManager = FolderManager(21, 0, [])
    flManager.LoadData(LoadType.RAWPHASES, False, True)
    flManager.RemoveDatatype(DataType.TIME)

    #flManager.TsPreprocess(True, "PAA", 400, None)
    #flManager.TsPreprocess(False, "SAX", 400, 100)
    flManager.SelectFeaturesAndSave(False, True, 5, 0.1, 0, False)

    #QUI CALCOLI PERFORMANCE DEL MODELLO

def TimeSeriesStatsTest():
    flManager = FolderManager(21, 0, [])
    flManager.LoadData(LoadType.RAWPHASES, False, True)
    flManager.RemoveDatatype(DataType.TIME)

    #flManager.TsPreprocess(True, "PAA", 200, 50)
    #flManager.TsPreprocess(False, "SAX", 200, 50)
    flManager.SelectFeaturesAndSave(False, True, 10, 0.3, 0, False)

    #QUI CALCOLI PERFORMANCE DEL MODELLO

def main():
    TimeSeriesTest()
    input("Waiting...")
    
if __name__ == "__main__":
    main()