import pandas as pd


def synchronize(data,dante):
    # Read the datafarame of data
    # Read the dataframe of annotation

    # tagliare un centinaio di righe dai dataframe

    dfData = data.iloc[100:]
    dfDante = dante.iloc[100:]

    
    # synchronize the two dataframes

    dfData.reset_index(drop=True, inplace=True)
    dfDante.reset_index(drop=True, inplace=True)

    if len(dfData) > len(dfDante):
        dfData = dfData.iloc[:len(dfDante)]
    elif len(dfData) < len(dfDante):
        dfDante = dfDante.iloc[:len(dfData)]

    dfData = pd.concat([dfData, dfDante], axis=1)
    dfData.rename(columns={'Value': 'Stress_Value'}, inplace=True)

    return dfData



