import pandas as pd
import numpy as np
import os
from sklearn.preprocessing import StandardScaler
from sklearn.feature_selection import VarianceThreshold
from sklearn.cluster import FeatureAgglomeration
from sklearn.decomposition import PCA
from tslearn.utils import to_time_series_dataset


from userdata import UserData
from userdata import DataType

class DataManager:
    def __init__(self):
        self.subjects = []
        self.phasesMode = False

    def load(self, dataPath : str, numOfSubjects : int, excludedSubjects : list):
        print("Start Loading")
        self.subjects = []
        self.phasesMode = False

        # Read data for each subject
        for subjectId in range(0, numOfSubjects):
            if subjectId in excludedSubjects:
                print(f"Subject {subjectId} excluded!")
            else:
                print(f"Loading subject {subjectId} ...")
                csvPath = os.path.join(dataPath, f'D{subjectId}.csv')
                
                if os.path.exists(csvPath):
                    subjectDf = pd.read_csv(csvPath, header=0, delimiter=';')

                    user = UserData(subjectId)
                    user.DivideDatatrackerData(subjectDf)

                    self.subjects.append(user)
                else:
                    print(f"Warning: CSV file for Subject {subjectId} not found.")
        print("Loading completed !")

    def loadInPhaseForm(self, dataPath : str, numOfSubjects : int, dataMerged : bool, ignoreMissingDatatypes : bool, excludedSubjects : list):
        print("Start Loading")
        self.subjects = []
        self.phasesMode = True

        # Read data for each subject
        for subjectId in range(0, numOfSubjects):
            if subjectId in excludedSubjects:
                print(f"Subject {subjectId} excluded!")
            else: 
                print(f"Loading subject {subjectId} ...")
                data = {}
                user = UserData(subjectId)
                validUser = True
            
                if not dataMerged:
                    for dataType in DataType:
                        if dataType != DataType.ROOM and dataType != DataType.ALL:
                            csvPath = os.path.join(dataPath, f'D{subjectId}{dataType.value}.csv')
                        
                            if os.path.exists(csvPath):
                                data[dataType] = user.DivideConcatPhases(pd.read_csv(csvPath, header=0, delimiter=';'))
                            else:
                                print(f"Warning: CSV file for Subject {subjectId} datatype {dataType.value} not found.")

                                validUser = False
                else:
                    dataType = DataType.ALL
                    csvPath = os.path.join(dataPath, f'D{subjectId}{dataType.value}.csv')
                    
                    if os.path.exists(csvPath):
                        data[dataType] = user.DivideConcatPhases(pd.read_csv(csvPath, header=0, delimiter=';'))
                    else:
                        print(f"Warning: CSV file for Subject {subjectId} datatype {dataType.value} not found.")

                        validUser = False
                            
                
                if ignoreMissingDatatypes or validUser:
                    user.SetDataDict(data, self.phasesMode)
                    self.subjects.append(user)

                    if ignoreMissingDatatypes:
                        print(f"Subject {subjectId} loaded with some data missing.")
                else:
                    print(f"Warning: Subject {subjectId} not loaded correctly, some data are missing.")  
        print("Loading completed !")

    def saveInPhaseForm(self, dataPath : str):
        if(self.phasesMode):
            for subject in self.subjects:
                for dataType, _ in subject.data.items():
                    csvPath = os.path.join(dataPath, f'D{subject.id}{dataType.value}.csv')

                    catDf = subject.GetAllPhasesConcat(dataType)
                    catDf = catDf.fillna(0)
                    catDf.to_csv(csvPath, sep = ';', index=False)
        else:
            print("method not implemented without phase mode")

    def preprocessData(self):
        for user in self.subjects:
                print(f"Preprocessing subject {user.id}")
                user.PreprocessAllData()

    def splitIntoPhases(self):
        if(not self.phasesMode):
            for user in self.subjects:
                print(f"Splitting subject {user.id}")
                user.SplitAllIntoPhases()
            self.phasesMode = True
        else:
            print("already in phase mode")

    def MergeDatatypesInPhases(self):
        #merge all datatype into one (Datatype.ALL) preserving the different phases
        if self.phasesMode:
            for subject in self.subjects:
                subject.MergeDatatypesInPhases()
        else:
            print("method developed only for phase mode")

    def MergePhasesForeachDatatypeIntoOnePhase(self):
        #create a fake phase to apply methods without data splitted into phases
        if self.phasesMode:
            for subject in self.subjects:
                subject.MergePhasesForeachDatatypeIntoOnePhase()
        else:
            print("method developed only for phase mode")

    def __normalizeTs(self):
        if self.phasesMode:
            for subject in self.subjects:
                subject.NormalizePhasesTs()
        else:
            print("method developed only for phase mode")

    def ApplyTsPreprocessing(self, normalize, preprocesserName : str, seg = 50, alphSize = 20):
        if self.phasesMode:
            if normalize:
                self.Normalize(True)

            for subject in self.subjects:
                subject.ApplyTsPreprocessing(preprocesserName, seg, alphSize)
        else:
            print("method developed only for phase mode")

    def __normalizeData(self):
        #Normalization applied on all subjects

        scaler = StandardScaler()

        if(self.phasesMode):
            #Divide the normalization between the different datatypes and phases

            # Assuming all subjects have the same dataType and phases
            for dataType in self.subjects[0].data.keys():
                for phase in self.subjects[0].data[dataType].keys():
                    print(f"Normalization (all subjects) for datatype{dataType} phase {phase}")
                    
                    # Concatenate the dataframes for this dataType and phase across all subjects
                    phase_df = pd.concat([subject.data[dataType][phase] for subject in self.subjects])

                    # Normalize this dataframe
                    phase_df_normalized = pd.DataFrame(scaler.fit_transform(phase_df), columns=phase_df.columns)

                    # Split the normalized dataframe back into separate dataframes for each subject
                    start = 0
                    for subject in self.subjects:
                        end = start + len(subject.data[dataType][phase])
                        subject.data[dataType][phase] = phase_df_normalized.iloc[start:end, :]
                        start = end
        else:
            print("method developed only for phase mode")
                    
    def Normalize(self, isTs):
        if isTs:
            self.__normalizeTs()
        else:
            self.__normalizeData()
    
    def extractFeatures(self, nJobs : int, normalize : bool):
        
        if normalize:
            self.Normalize(True)
        
        #Applied using tsfresh library
        for user in self.subjects:
            print(f"Extraction features for subject {user.id}")
            user.extractFeatures(nJobs)

    def __correlationSelection(self, data, crThreshold):
        corr_matrix = data.corr().abs()
        upper = corr_matrix.where(np.triu(np.ones(corr_matrix.shape), k=1).astype(bool))
        to_drop = [column for column in upper.columns if any(upper[column] > crThreshold)]
        return data.loc[:, ~data.columns.isin(data.columns[to_drop])]

    def __pcaSelection(self, data, pcaComponent):
        nSamples, nFeatures = data.shape

        if pcaComponent > nSamples or pcaComponent > nFeatures:
            nComponents = min(pcaComponent, nSamples, nFeatures)
            if nSamples < nFeatures:
                print(f"Pca number reduced because of samples #{nSamples}")
            else:
                print(f"Pca number reduced because of features #{nFeatures}")
        else:
            nComponents = pcaComponent

        
        pca = PCA(n_components=nComponents)
        return pd.DataFrame(pca.fit_transform(data), columns = [f'PC{i+1}' for i in range(nComponents)])

    def __featureAgglomeration(self, data, nClusters, verbose: bool):

        _, nFeatures = data.shape

        if nClusters >= nFeatures:
            print(f"Agglomeration will not improve data because there are features {nFeatures} <= nClusters {nClusters}")
            return pd.DataFrame(data, columns = [f'Agglomerated{i+1}' for i in range(nFeatures)])     
        else:
            agglo = FeatureAgglomeration(n_clusters=nClusters)
            agglomerated = agglo.fit_transform(data)

            if verbose:
                # Create a mapping of new feature to old features
                feature_mapping = {}
                for i, label in enumerate(agglo.labels_):
                    if label not in feature_mapping:
                        feature_mapping[label] = [data.columns[i]]
                    else:
                        feature_mapping[label].append(data.columns[i])

                # Print the mapping
                for new_feature, old_features in feature_mapping.items():
                    print(f'New feature {new_feature} is composed of old features {old_features}')

            return pd.DataFrame(agglomerated, columns = [f'Agglomerated{i+1}' for i in range(nClusters)])

    def selectFeatures(self, normalize : bool, mergeDatatypes : bool, isTs : bool, varianceThreshold : float, selectedComponentsPCA : int, nClusters : int, verbose : bool):
        print("Starting feature selection")

        if verbose:
            if varianceThreshold < 0.0:
                print("Variance threshold disabled")
        
            if selectedComponentsPCA <= 0:
                print("PCA disabled")

            if nClusters <= 0:
                print("Agglomeration clustering disabled")

        if(mergeDatatypes):
            self.MergeDatatypesInPhases()

        if normalize:
            self.Normalize(isTs)

        selector = VarianceThreshold(threshold= varianceThreshold)

        if(self.phasesMode):
            for dataType in self.subjects[0].data.keys():
                for phase in self.subjects[0].data[dataType].keys():
                    print(f"Features selection (all subjects) for datatype{dataType} phase {phase}")

                    # Concatenate the dataframes for this dataType and phase across all subjects
                    phase_df = pd.concat([subject.data[dataType][phase] for subject in self.subjects])

                    print(f"Starting selection, samples: {phase_df.shape[0]}, features: {phase_df.shape[1]}")

                    # Apply the variance threshold
                    if varianceThreshold >= 0.0:
                        phase_df = pd.DataFrame(selector.fit_transform(phase_df), columns=phase_df.columns[selector.get_support()])
                        print(f"Variance threshold applied, samples: {phase_df.shape[0]}, features: {phase_df.shape[1]}")
                    
                    # Apply PCA
                    if selectedComponentsPCA > 0:
                        phase_df = self.__pcaSelection(phase_df, selectedComponentsPCA)
                        print(f"PCA applied, samples: {phase_df.shape[0]}, features: {phase_df.shape[1]}")

                    # Apply Agglomeration
                    if nClusters > 0:
                        phase_df = self.__featureAgglomeration(phase_df, nClusters, verbose)
                        print(f"Agglomeration applied, samples: {phase_df.shape[0]}, features: {phase_df.shape[1]}")

                    # Split the selected features back into separate dataframes for each subject
                    start = 0
                    for subject in self.subjects:
                        end = start + len(subject.data[dataType][phase])
                        subject.data[dataType][phase] = phase_df.iloc[start:end, :]
                        start = end
        else:
            print("Warning: method developed only for phase mode")
    
    def __ToPhaseOnly(self):
        #remove datatype from dictionary

        if(self.phasesMode):
            self.MergeDatatypesInPhases()
                
            for sub in self.subjects:
                sub.data = sub.data[DataType.ALL]
        else:
            print("Data not in phase mode, placeholder phase 0.0 applied")
            for sub in self.subjects:
                temp = sub.data.copy()
                sub.data = {}
                sub.data[0.0] = temp
            self.phasesMode = True

        return

    def GetDataForClustering(self, isTs : bool, normalize : bool, applyTsNormToTs : bool, prefix : str):
        print("Shaping data for clustering...")

        X = {}

        if normalize:
            self.Normalize(applyTsNormToTs)

        self.__ToPhaseOnly()

        for phase in self.subjects[0].data.keys():
            if isTs:
                #X[prefix + "Ts" + str(phase)] = np.concatenate(, axis=0)
                X[prefix + "Ts" + str(phase)] = to_time_series_dataset([subject.data[phase].values for subject in self.subjects])
            else:
                X[prefix + str(phase)] = pd.concat([subject.data[phase] for subject in self.subjects])

        return X 
    
    def RemoveDatatype(self, dataType : DataType):
        print(f"{dataType} removed")
        for sub in self.subjects:
            sub.RemoveDatatype(dataType)

    def RemovePhase(self, phase):
        print(f"phase {phase} removed")
        for sub in self.subjects:
            sub.RemovePhase(phase)