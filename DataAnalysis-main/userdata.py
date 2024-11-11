import pandas as pd
import numpy as np
from enum import Enum
import tsfresh
from tsfresh.feature_extraction import EfficientFCParameters
from tslearn.preprocessing import TimeSeriesScalerMeanVariance
from tslearn.piecewise import PiecewiseAggregateApproximation, SymbolicAggregateApproximation, OneD_SymbolicAggregateApproximation
    

class DataType(Enum):
    TIME = "time"
    EYE = "eye"
    FACE = "face"
    LIPSYNC = "lipsync"
    POSITION = "position"
    BUTTON = "button"
    ENTITY = "entity"
    ROOM = "room"
    ALL = "all"

class UserData:
    def __init__(self, id):

        self.id = id
        self.phasesMode = False

        #init data dictionary
        self.data = {}

        # Initialize empty DataFrames for each category/dataType
        self.data[DataType.TIME] = pd.DataFrame() # Data related to time (unity and realtime)
        self.data[DataType.FACE] = pd.DataFrame() # Data related to face tracking (weights in FACS format)
        self.data[DataType.EYE] = pd.DataFrame() # Data related to eye tracking (in head, tracking and world space for position and rotation of eyes + point observed and object semantic)
        self.data[DataType.LIPSYNC] = pd.DataFrame() # Data related to lipsync (viseme and laught)
        self.data[DataType.POSITION] = pd.DataFrame() # Data related to user positions (world/tracker and local/ovr data about vr headset/head and controller, including position, rotation, velocity, acceleration, angular velocity and angular acceleration)
        self.data[DataType.BUTTON] = pd.DataFrame() # Data related to buttons on controller (pression, touch, proximity/near, 1D like and 2D like button data)
        self.data[DataType.ENTITY] = pd.DataFrame() # Data related to entities inside the vr scene (world positions)
        self.data[DataType.ROOM] = pd.DataFrame() # Data related to the actual room

        #prefix for buttons
        self.buttonPrefix = 'OVRInput+Raw'

    def SetDataDict(self, dictData, phaseMode = True):
        self.data = dictData.copy()
        self.phasesMode = phaseMode

    def DivideDatatrackerData(self, inputData):
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

        lipsyncColumnNames = ['sil', 'pp', 'ff', 'th', 'dd', 'kk', 'ch', 'ss', 'nn', 'rr', 'aa', 'e', 'ih', 'oh', 'ou', 'laughter probability']

        self.data[DataType.TIME] = inputData.loc[:, inputData.columns.str.startswith('Frame') | inputData.columns.str.startswith('timestamp')]
        self.data[DataType.FACE] = inputData.loc[:, inputData.columns.isin(faceColumnNames)]
        self.data[DataType.EYE] = inputData.loc[:, (inputData.columns.str.startswith('Eye') & ~inputData.columns.str.startswith('Eyes')) | inputData.columns.str.startswith('Semantic')]
        self.data[DataType.LIPSYNC] = inputData.loc[:, inputData.columns.isin(lipsyncColumnNames)]
        self.data[DataType.BUTTON] = inputData.loc[:, inputData.columns.str.startswith(self.buttonPrefix)]      
        self.data[DataType.POSITION] = inputData.loc[:, (inputData.columns.str.startswith("OVR") & ~inputData.columns.str.startswith(self.buttonPrefix)) | inputData.columns.str.startswith("Tracker")]
        self.data[DataType.ENTITY] = inputData.loc[:, inputData.columns.str.startswith('Entity')]
        self.data[DataType.ROOM] = inputData.loc[:, inputData.columns.str.startswith('actualRoom')]

        self.phasesMode = False
    
    def __GetVectorNames(self, df, removeNonVectors = False):
        #remove non vector columns, removeNonVectors can be set to True if content of columns is not only made of vector componenst
        if(removeNonVectors):
            df = df.loc[:, df.columns.str.endswith('X') | df.columns.str.endswith('Y') | df.columns.str.endswith('Z')]

        #return the name of the vectors based on vector components
        return list(set([col[:-1] for col in df.columns]))

    def GetAllDatatypeConcat(self):
        if(self.phasesMode):
            return pd.concat([self.GetAllPhasesConcat(dataType) for dataType in self.data.keys()], axis=1)
        else:
            return pd.concat([df for df in self.data.values()], axis=1)

    def GetAllPhasesConcat(self, dataType):
        if self.phasesMode:
            lst = []
            for phase, df in self.data[dataType].copy().items():
                df["phase"] = phase
                lst.append(df)
            
            return pd.concat(lst, axis=0)
        else:
            print(f"Warning: Method return an empty dataframe if not in phase mode (subject {self.id})")
            return pd.DataFrame
    
    def GetPhases(self):
        if self.phasesMode:
            return list(self.data[self.GetDatatypes()[0]].keys())
        else:
            print(f"Warning: Method return an empty list if not in phase mode (subject {self.id})")
            return []

    def GetDatatypes(self):
        return list(self.data.keys())
    
    def GetSampleNumber(self, phase):
        return len(self.data[self.GetDatatypes()[0]][phase]) #Assume that all features have the same length

    def GetAllDatatypeConcat(self, phase):
        if self.phasesMode:
            return pd.concat([self.data[dataType][phase] for dataType in self.data.keys()], axis=1)
        else:
            return pd.concat([df for df in self.data.values()], axis=1)
    
    def DivideConcatPhases(self, concatPhases):       
        return {phase: data.loc[:, concatPhases.columns != 'phase'] for phase, data in concatPhases.groupby("phase")}

    def __preprocessTimeData(self):
        #Take only times in ms and unity time (avoid frames and readable timestamp)
        selected_columns = ['timestampStartFixedMillisecond', 'timestampUnityTime']
        self.data[DataType.TIME] = self.data[DataType.TIME][selected_columns]

        # Use interpolate to fill NaN values
        self.data[DataType.TIME] = self.data[DataType.TIME].interpolate()

        #make time start from 0 ms (sub first value)
        self.data[DataType.TIME] = self.data[DataType.TIME].apply(lambda x: x - x.iloc[0])

    def __preprocessFaceTrackingData(self):
        # Use interpolate to fill NaN values
        self.data[DataType.FACE] = self.data[DataType.FACE].interpolate()

    def __preprocessEyeTrackingData(self):
        #remove semantic object name(not needed)
        self.data[DataType.EYE] = self.data[DataType.EYE].drop(columns = self.data[DataType.EYE].columns[self.data[DataType.EYE].columns.str.startswith('SemanticObj')])

        # Replace semantic tags with numbers, representing the level of danger of the tagged object observed
        semantic_columns = self.data[DataType.EYE].columns[self.data[DataType.EYE].columns.str.startswith('SemanticTag')]
        #TODO: DA Rivedere e modificare dati relativi al contesto vanno separati
        def EvDanger(x):
            if x == "Spider":
                return 1.0
            elif x == "Web":
                return 0.5
            else:
                return 0.0

        # QUESTO VA MODIFICATO
        for col in semantic_columns:
            self.data[DataType.EYE][col] = self.data[DataType.EYE][col].map(lambda x: float(EvDanger(x)))

        # Use interpolate to fill NaN values
        self.data[DataType.EYE] = self.data[DataType.EYE].interpolate()

    def __preprocessLipSyncData(self):
        self.data[DataType.LIPSYNC] = self.data[DataType.LIPSYNC].interpolate()

    def __preprocessButtonData(self):
        #QUI DROP DI TUTTI I BUTTONS NON UTILIZZATI 

        # Remove button prefix for better management
        self.data[DataType.BUTTON] = self.data[DataType.BUTTON].rename(columns={col: col.replace(self.buttonPrefix, '') for col in self.data[DataType.BUTTON].columns})

        # Drop not used Button Columns
        self.data[DataType.BUTTON] = self.data[DataType.BUTTON].drop(columns=['ButtonNone', 'ButtonBack', 'ButtonLShoulder', 'ButtonRShoulder', 'ButtonDpadUp', 'ButtonDpadDown', 'ButtonDpadLeft', 'ButtonDpadRight'])

        # Drop not used Touch Columns
        self.data[DataType.BUTTON] = self.data[DataType.BUTTON].drop(columns=['TouchNone', 'TouchLTouchpad', 'TouchRTouchpad'])

        # Drop not used NearTouch Columns
        self.data[DataType.BUTTON] = self.data[DataType.BUTTON].loc[:, self.data[DataType.BUTTON].columns != 'NearTouchNone']

        # Drop not used Axis1D Columns
        self.data[DataType.BUTTON] = self.data[DataType.BUTTON].drop(columns=['Axis1DNone', 'Axis1DLStylusForce', 'Axis1DRStylusForce'])

        # Drop not used Axis2D Columns
        self.data[DataType.BUTTON] = self.data[DataType.BUTTON].drop(columns=['Axis2DNoneX', 'Axis2DNoneY', 'Axis2DLTouchpadX','Axis2DLTouchpadY','Axis2DRTouchpadX','Axis2DRTouchpadY'])

        # Add prefix to every column
        self.data[DataType.BUTTON].columns = [self.buttonPrefix + col for col in self.data[DataType.BUTTON].columns]

        # Use interpolate to fill NaN values
        self.data[DataType.BUTTON] = self.data[DataType.BUTTON].interpolate()

    def __preprocessPositionData(self):
        # Remove local acceleration
        # Between the position vectors, the Tracker is the only type of data with global acceleration
        self.data[DataType.POSITION] = self.data[DataType.POSITION].loc[:, ~self.data[DataType.POSITION].columns.str.contains('Acceleration') | self.data[DataType.POSITION].columns.str.contains('Tracker')]

        # Use interpolate to fill NaN values
        self.data[DataType.POSITION] = self.data[DataType.POSITION].interpolate()

    def __preprocessEntityPosData(self):
        #Change NaN as last valid value
        self.data[DataType.ENTITY] = self.data[DataType.ENTITY].ffill()

        #TODO:QUESTO VA MODIFICATO E DI CONTESTO
        #Rename entities columns to remove repetitions and errors
        self.data[DataType.ENTITY] = self.data[DataType.ENTITY].rename(columns={col: col.replace('EntityPosition', '') for col in self.data[DataType.ENTITY].columns})
        self.data[DataType.ENTITY] = self.data[DataType.ENTITY].rename(columns={col: col.replace('SpiderSpider', 'Spider') for col in self.data[DataType.ENTITY].columns})
        self.data[DataType.ENTITY] = self.data[DataType.ENTITY].rename(columns={col: col.replace('WebWeb', 'Web') for col in self.data[DataType.ENTITY].columns})
        self.data[DataType.ENTITY] = self.data[DataType.ENTITY].rename(columns={col: col.replace('Web(Clone) (1)', 'Web25') for col in self.data[DataType.ENTITY].columns})
        self.data[DataType.ENTITY] = self.data[DataType.ENTITY].rename(columns={col: col.replace('Web(Clone) (2)', 'Web26') for col in self.data[DataType.ENTITY].columns})
      
        #Calculate distance between each entity(danger) and the Tracker Head Position
        self.data[DataType.ENTITY] = pd.DataFrame({'EntityDistance' + dangerName: np.sqrt((self.data[DataType.ENTITY][[f'{dangerName}X', f'{dangerName}Y', f'{dangerName}Z']].sub(self.data[DataType.POSITION][['TrackerHeadPositionX', 'TrackerHeadPositionY', 'TrackerHeadPositionZ']].values) ** 2).sum(axis=1))
                                     for dangerName in self.__GetVectorNames(self.data[DataType.ENTITY])})

    def __preprocessRoomData(self):
        # Set to 0 NaN values
        self.data[DataType.ROOM] = self.data[DataType.ROOM].fillna(0)

        #Drop column actualRoomName, the method drop gives warning in this case
        #Is better to explicitly indicate that you are modifying the original DataFrame thorugh loc method
        self.data[DataType.ROOM] = self.data[DataType.ROOM].loc[:, self.data[DataType.ROOM].columns != 'actualRoomName']

        # process hallway room number
        # hallway (0) is changed to match the current phase
        #   x.0 if the user remaing in the same phase
        #   x.5 if the user is changing phase
        
        self.data[DataType.ROOM]['actualRoom'] = self.data[DataType.ROOM]['actualRoom'].astype(float)

        check = 0
        lst = []
        for i, value in enumerate(self.data[DataType.ROOM]['actualRoom']):
            if(value > check):
                self.data[DataType.ROOM].loc[lst, 'actualRoom'] = float(check) + 0.5
                lst = []
                check = value
            elif value == check:
                 self.data[DataType.ROOM].loc[lst, 'actualRoom'] = float(check) #+ 0.1
                 lst = []
            elif check != 0 and value == 0:
                lst.append(i)
        self.data[DataType.ROOM].loc[lst, 'actualRoom'] = float(check) + 0.5
    
    def __preprocessKeyData(self):
        # Set to 0 NaN values (correspond to key not in end)
        self.data[DataType.KEY] = self.data[DataType.KEY].fillna(0)

    def NormalizePhasesTs(self):
        scaler = TimeSeriesScalerMeanVariance(mu=0., std=1.)

        for dataType, data in self.data.items():
            for phase, df in data.items():
                print(f"Subject {self.id}: Normalizing {dataType} in phase {phase} (ts normalization)...")

                # Convert the dataframe to a 3D numpy array
                X = df.values.reshape((1, -1, df.shape[1]))

                # Apply the tslearn scaler
                X_scaled = scaler.fit_transform(X)

                # Convert the scaled data back to a dataframe
                df_scaled = pd.DataFrame(X_scaled[0, :, :], columns=df.columns)

                # Store the scaled dataframe back in the user's data
                self.data[dataType][phase] = df_scaled

    def ApplyTsPreprocessing(self, preprocesserName, seg = 50, alphSize = 20):
        #QUI è DA VEDERE
        preprocesser = None

        if preprocesserName == "SAX":
            preprocesser = SymbolicAggregateApproximation(n_segments = seg, alphabet_size_avg = alphSize)
        elif preprocesserName == "PAA":
            preprocesser = PiecewiseAggregateApproximation(n_segments = seg)
        else:
            print("Preprocesser not available")


        if preprocesser is not None:
            for dataType, data in self.data.items():
                for phase, df in data.items():
                    print(f"Subject {self.id}: {preprocesserName} {dataType} in phase {phase}...")
                    # Convert the dataframe to a 3D numpy array
                    X = df.values.reshape((1, -1, df.shape[1]))
                    # Apply the tslearn scaler
                    X_processed = preprocesser.fit_transform(X)
                    # Convert the scaled data back to a dataframe
                    X_processed = pd.DataFrame(X_processed[0, :, :], columns=df.columns)

                    # Store the scaled dataframe back in the user's data
                    self.data[dataType][phase] = X_processed

    def MergeDatatypesInPhases(self):
        if self.phasesMode:
            tempDict = {}
            tempDict[DataType.ALL] = {}

            for phase in self.GetPhases():
                tempDict[DataType.ALL][phase] = pd.concat([self.data[dataType][phase] for dataType in self.data.keys()], axis=1)

            self.data = tempDict 
        else:
            print("method not created for not phases mode (subject {self.id})")

    def MergePhasesForeachDatatypeIntoOnePhase(self):
        if self.phasesMode:
            for dataType, phases in self.data.items():
                temp = {}
                temp[5.0] = pd.concat([self.data[dataType][phase] for phase, _ in phases.items()], axis=0)
                self.data[dataType] = temp
        else:
            print("method not created for not phases mode (subject {self.id})")

    def PreprocessAllData(self):
        if(not self.phasesMode):
            self.__preprocessTimeData()
            self.__preprocessFaceTrackingData()
            self.__preprocessEyeTrackingData()
            self.__preprocessLipSyncData()
            self.__preprocessButtonData()
            self.__preprocessPositionData()
            self.__preprocessEntityPosData()
            self.__preprocessRoomData()
        else:
            print("method not created for phases mode (subject {self.id})")

    def SplitAllIntoPhases(self):
        if(not self.phasesMode):
            if DataType.ROOM in self.data.keys():
                # Split all data into phases based on the actual room
                self.data[DataType.TIME] = self.__splitIntoPhases(self.data[DataType.TIME])
                self.data[DataType.FACE] = self.__splitIntoPhases(self.data[DataType.FACE])
                self.data[DataType.EYE] = self.__splitIntoPhases(self.data[DataType.EYE])
                self.data[DataType.LIPSYNC] = self.__splitIntoPhases(self.data[DataType.LIPSYNC])
                self.data[DataType.BUTTON] = self.__splitIntoPhases(self.data[DataType.BUTTON])
                self.data[DataType.POSITION] = self.__splitIntoPhases(self.data[DataType.POSITION])
                self.data[DataType.ENTITY] = self.__splitIntoPhases(self.data[DataType.ENTITY])

                # Remove room data, useless after the division in phases
                del self.data[DataType.ROOM]

                self.phasesMode = True
            else:
                print(f"Room data missing, is impossible to split into phases (subject {self.id})")
        else:
            print(f"Already into phases (subject {self.id})")
            
    def __splitIntoPhases(self, inputDf):
        # Get the room column, that contains actual room/phase
        splitterDf = self.data[DataType.ROOM]["actualRoom"]

        # Split the data based on phases into a dictionary using the phase number as key
        dfInPhases = {phase: data for phase, data in pd.concat([inputDf, splitterDf], axis=1).groupby("actualRoom")}

        # Remove the x.5 keys, that represents the moments user is travelling to another phase
        filterMovementToNextPhase = {phase: data for phase, data in dfInPhases.items() if float(phase).is_integer() }

        # Remove the column containing the room
        for phase, _ in filterMovementToNextPhase.items():
            filterMovementToNextPhase[phase] = filterMovementToNextPhase[phase].loc[:, filterMovementToNextPhase[phase].columns != 'actualRoom']

        return filterMovementToNextPhase

    def extractFeatures(self, nJobs = 1):
        #DA MODIFICARE
        if(self.phasesMode):
            for dataType, data in self.data.items():
                for phase, df in data.items():
                    print(f"Subject {self.id}: Feature extraction for datatype {dataType} in phase {phase}...")

                    # Make sure df has an id column for tsfresh
                    df['id'] = self.id

                    # Extract features with tsfresh
                    extracted_features = tsfresh.extract_features(df, column_id='id', default_fc_parameters= EfficientFCParameters(), n_jobs=nJobs)

                    # Store the extracted features back in the user's data
                    self.data[dataType][phase] = extracted_features
        else:
            print("method not created for not phases mode (subject {self.id})")

    def RemoveDatatype(self, dataType):
        if self.data.__contains__(dataType):
            del self.data[dataType]
        else:
            print(f"Datatype not present in dictionary (Subject {self.id})")

    def RemovePhase(self, phase):
        for dataType, _ in self.data.items():
            if self.data[dataType].__contains__(phase):
                del self.data[dataType][phase]
            else:
                print(f"Phase not present in dictionary (Subject {self.id})")