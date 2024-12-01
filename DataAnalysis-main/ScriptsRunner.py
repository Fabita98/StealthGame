from Preprocessing.DivideDataframe import DivideDataframe

if __name__ == "__main__":
    drivePath = "D:/University-Masters/Thesis"
    externalData = ['HeartBeatRate', 'MaxHeartBeatRate', 'MinHeartBeatRate', 'AverageHeartBeatRate', 'IsInStressfulArea', 'Deaths', 'LastCheckpoint']
    
    discreteData = ['HeartBeatRate', 'MaxHeartBeatRate', 'MinHeartBeatRate', 'AverageHeartBeatRate', 'IsInStressfulArea', 'Deaths', 'LastCheckpoint']

    
    # Divide Dataframe
    print("==========DivideDataframe==========")
    divide_data_processor = DivideDataframe(path=drivePath, externalData=externalData)
    divide_data_processor.processFileOnPath()