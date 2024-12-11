from Preprocessing.DivideDataframe import DivideDataframe
from Preprocessing.ProcessDataframes import ProcessDataFrames
from Preprocessing.DanteAnnotation import DanteAnnotation
from Preprocessing.FeatureExtraction import FeatureExtraction
from Preprocessing.FeatureSelection import FeatureSelection
from Preprocessing.VisualizeData import VisualizeData
from Models.CompareOnlineKFs import CompareOnlineKFs

if __name__ == "__main__":
    drivePath = "D:/University-Masters/Thesis_heartbeat"
    externalData = ['IsInStressfulArea', 'Deaths', 'LastCheckpoint']
    discreteData = ['IsInStressfulArea', 'Deaths', 'LastCheckpoint']
    # discreteData = ['HeartBeatRate', 'IsInStressfulArea', 'Deaths', 'LastCheckpoint']
    datatypeToExcludeInKalman = ['Dante', 'External', "Eye", "Data_ALL"]
    subjectsToExclude = ["S0", "S1", "S2", "S3", "S4", "S5", "S6"]
    process_movement=True, 
    process_eye=True, 
    process_face=True, 
    process_external=True, 
    process_button=False, 
    process_heartbeat=True, 
    dante_samples_per_second = 50
    data_samples_per_second = 90
    dim_seconds = 0.5
    shift_seconds = 0.25
    selection_type = "Single"
    doNormalization=False
    doStandardization=False

    doDivideDataframe = True
    doVisualizeData = True
    doProcessDataframes = True
    doDanteAnnotation = True
    doFeatureExtraction = True
    doFeatureSelection = True
    doKalmanFilter = True

    
    # Divide Dataframe (Divide to multiple classes)
    if(doDivideDataframe):
        print("==========DivideDataframe==========")
        divideDataframe = DivideDataframe(path=drivePath, external_data=externalData, heartbeat_excluded=False, subjects_to_exclude=subjectsToExclude)
        divideDataframe.main()
        
    # Creat Plots of the processed data
    if(doVisualizeData):
        print("==========VisualizeData==========")
        visualizeData = VisualizeData(path=drivePath,
                                      process_movement=process_movement, 
                                      process_eye=process_eye, 
                                      process_face=process_face, 
                                      process_external=process_external, 
                                      process_button=process_button, 
                                      process_heartbeat=process_heartbeat,
                                      subjects_to_exclude=subjectsToExclude)
        visualizeData.main()

    # Process Dataframes (Remove NANs, Convert timestampUnityTime to datetime, Resample the data to the specified target sampling rate)
    if(doProcessDataframes):
        print("==========ProcessDataFrames==========")
        processDataframes = ProcessDataFrames(path=drivePath, 
                                              discrete_data=discreteData, 
                                              target_samples_per_second=data_samples_per_second,
                                              process_movement=process_movement, 
                                              process_eye=process_eye, 
                                              process_face=process_face, 
                                              process_external=process_external, 
                                              process_button=process_button, 
                                              process_heartbeat=process_heartbeat,
                                              subjects_to_exclude=subjectsToExclude)
        processDataframes.main()

    # Process Dante and set the correct sampling rate)
    if(doDanteAnnotation):
        print("==========DanteAnnotation==========")
        danteAnnotation = DanteAnnotation(path=drivePath, 
                                          samples_per_second=dante_samples_per_second,
                                          subjects_to_exclude=subjectsToExclude)
        danteAnnotation.main()
        
    # Extract features on dataframes like mean, median, ...
    if(doFeatureExtraction):
        print("==========FeatureExtraction==========")
        featureExtraction = FeatureExtraction(path=drivePath, 
                                              dim_seconds=dim_seconds,
                                              shift_seconds=shift_seconds, 
                                              norm=doNormalization, 
                                              stand=doStandardization, 
                                              sampling_rate=data_samples_per_second, 
                                              discrete_data=discreteData,
                                              subjects_to_exclude=subjectsToExclude)
        featureExtraction.main()

    # Select most important features based on correlation and variance
    if(doFeatureSelection):
        print("==========FeatureSelection==========")
        featureSelection = FeatureSelection(path=drivePath, 
                                            selection_type=selection_type, 
                                            dim_seconds=dim_seconds, 
                                            shift_seconds=shift_seconds, 
                                            norm=doNormalization, 
                                            stand=doStandardization)
        featureSelection.main()

    # Apply Kalman Filtering with baseline and external inputs
    if(doKalmanFilter):
        print("==========CompareOnlineKFs==========")
        compareOnlineKFs = CompareOnlineKFs(path=drivePath, 
                                            dim_seconds=dim_seconds, 
                                            shift_seconds=shift_seconds, 
                                            norm=doNormalization, 
                                            stand=doStandardization, 
                                            datatype_to_exclude=datatypeToExcludeInKalman, 
                                            subjects_to_exclude=subjectsToExclude)
        compareOnlineKFs.main()

