from sklearn.decomposition import PCA
from sklearn.feature_selection import SelectKBest, VarianceThreshold, f_regression
import os
import pandas as pd
import tsfresh as tsf
import numpy as np
from sklearn.preprocessing import MinMaxScaler, StandardScaler
from tsfresh import extract_features
from tsfresh.feature_extraction import EfficientFCParameters, MinimalFCParameters
import time

dim_seconds = 0.5
shift_seconds = 0.25
norm = False
stand = False
#~ subjects_to_exclude = ["S25", "S100", "S101"]
#subjects_to_exclude = subjects_to_exclude + ([f"S{i}" for i in range(0, 2)] + [f"S{i}" for i in range(3, 29)])
FraDrivePath = '/mnt/g/.shortcut-targets-by-id/1wNGSxajmNG6X6ORVLNxGRZkrJJVw02FA/Test'
SusiDrivePath = 'G:/.shortcut-targets-by-id/1wNGSxajmNG6X6ORVLNxGRZkrJJVw02FA/Test'
drivePath = 'D:/University-Masters/Thesis'
AlienPath = 'F:/Data_Analysis'


def featureSelection(df, corrth, varth):
    # selezione tramite correlazione tra feature,varianza

    dfselection = correlation(df, corrth)

    dfselection = variance(dfselection, varth)
    # nel caso, chiama pca
    # restituisci df
    return dfselection


def featureSelectionExternal(df, corrth, varth, y):
    # selezione tramite correlazione tra feature,varianza

    dfselection = correlation(df, corrth)
    dfselection = variance(dfselection, varth)
    dfselection = stressSelection(dfselection, y)

    # nel caso, chiama pca
    # restituisci df
    return dfselection


def correlation(data, corrth):
    corr_matrix = data.corr().abs()
    upper = corr_matrix.where(np.triu(np.ones(corr_matrix.shape), k=1).astype(bool))
    to_drop = [column for column in upper.columns if any(upper[column].gt(corrth))]
    data.drop(columns=to_drop)
    return data


def variance(data, varth):
    # Calculate variance for each feature
    variances = data.var()
    
    # Check if all variances are below the threshold
    if all(variances < varth):
        print(f"Warning: All features have variance below {varth}.")
        print("Consider lowering the threshold or using a different feature selection method.")
        print(f"Minimum variance: {variances.min()}, Maximum variance: {variances.max()}")
        # Optionally return the data as-is or drop constant columns
        return data.loc[:, (variances > 0)]
    
    # Apply VarianceThreshold if some features meet the threshold
    variance_selector = VarianceThreshold(threshold=varth)
    variance_selected = variance_selector.set_output(transform="pandas").fit_transform(data)
    return variance_selected



def stressSelection(data, y):
    # SelectKBest
    feature_selector = SelectKBest(f_regression, k="all")
    fit = feature_selector.fit(data, y)

    p_values = pd.DataFrame(fit.pvalues_)
    scores = pd.DataFrame(fit.scores_)
    input_variable_names = pd.DataFrame(data.columns)
    summary_stats = pd.concat([input_variable_names, p_values, scores], axis=1)
    summary_stats.columns = ["input_variable", "p_value", "f_score"]
    summary_stats.sort_values(by="p_value", inplace=True)

    # per ora sono questi
    p_value_threshold = 0.1
    score_threshold = 5

    selected_variables = summary_stats.loc[(summary_stats["f_score"] >= score_threshold) &
                                           (summary_stats["p_value"] <= p_value_threshold)]
    selected_variables = selected_variables["input_variable"].tolist()
    data = data[selected_variables]

    return data


def pca(data, pcaComponents):
    # principal component analysis

    nSamples, nFeatures = data.shape

    if pcaComponents > nSamples or pcaComponents > nFeatures:
        nComponents = min(pcaComponents, nSamples, nFeatures)
        if nSamples < nFeatures:
            print(f"Pca number reduced because of samples #{nSamples}")
        else:
            print(f"Pca number reduced because of features #{nFeatures}")
    else:
        nComponents = pcaComponents

    pca = PCA(n_components=nComponents)
    pca_result = pca.fit_transform(data)
    return pca_result


def save_dataframes_in_file(dataframe, subject_folder, name):
    if not os.path.exists(subject_folder):
        os.makedirs(subject_folder)

    dataframe.to_csv(os.path.join(subject_folder, name), index=False, sep=';')


if __name__ == "__main__":
    path = drivePath
    dirs = sorted(list(filter(lambda x: x[0] == 'S', os.listdir(path))), key=lambda x: int(x[1:]))

    import warnings

    selectionType = "Single"
    warnings.filterwarnings("ignore")
    match selectionType:
        case "Single":
            #~ dirs = [dr for dr in dirs if dr not in subjects_to_exclude]
            for dr in dirs:
                features_subject_folder = path + '/' + dr + f"/WindowedCsv_{dim_seconds}_{shift_seconds}_stand{stand}_norm{norm}"
                if os.path.exists(features_subject_folder):
                    feature_files = os.listdir(features_subject_folder)
                    for file in feature_files:
                        if file.startswith("S") and file.endswith(
                                ".csv") and "Dante" not in file and "External" not in file:
                            start_time = time.time()
                            print(f"Processing {file}")
                            df = pd.read_csv(features_subject_folder + "/" + file, sep=';')
                            selected_df = featureSelection(df, 0.8, 0.2)
                            save_dataframes_in_file(selected_df, features_subject_folder + "/FeatureSelected", file)
                            print(f"{file} done in {time.time() - start_time}")
        case "Multiple":
            total_dante_df = pd.DataFrame()
            total_external_df = pd.DataFrame()

            # subjects_to_exclude = subjects_to_exclude + ["S0"]
            # dirs = [dr for dr in dirs if dr not in subjects_to_exclude]
            for dr in dirs:
                features_subject_folder = path + '/' + dr + f"/WindowedCsv_{dim_seconds}_{shift_seconds}_stand{stand}_norm{norm}"
                print(f"Processing {dr}")
                total_data_df = pd.DataFrame()

                dfDante = pd.DataFrame()
                dfExternal = pd.DataFrame()
                total_columns = 0
                if os.path.exists(features_subject_folder):
                    feature_files = os.listdir(features_subject_folder)
                    for file in feature_files:
                        '''
                        if file.startswith("S") and file.endswith(".csv") and "Dante" in file:
                            dfDante = pd.read_csv(features_subject_folder + "/" + file, sep=';')
                        if file.startswith("S") and file.endswith(".csv") and "External" in file:
                            dfExternal = pd.read_csv(features_subject_folder + "/" + file, sep=';')
                            dfExternal.fillna(0, inplace=True)
                        '''
                        if file.startswith("S") and file.endswith(
                                ".csv") and "Dante" not in file and "External" not in file:
                            df = pd.read_csv(features_subject_folder + "/" + file, sep=';')
                            print(f"Processing {file}")
                            total_data_df = pd.concat([total_data_df.reset_index(drop=True), df.reset_index(drop=True)],
                                                      axis=1)
                            print(f"Total columns: {df.shape[1]}")
                            total_columns += df.shape[1]
                    selectedTotalDataDf = featureSelection(total_data_df, 0.8, 0.2)
                    save_dataframes_in_file(selectedTotalDataDf, features_subject_folder + "/FeatureSelected",
                                            "Data_ALL.csv")
                    print(f"Total columns: {total_columns} -> {selectedTotalDataDf.shape[1]}")
        case "ForDataTypes":
            datatypes = ["Dante", "External", "Movement", "Button", "Face", "Eye"]
            #~ subjects_to_exclude = subjects_to_exclude + ["S0"]
            # dirs = [dr for dr in dirs if dr not in subjects_to_exclude]
            for datatype in datatypes:
                print(f"Processing {datatype}")
                total_df = pd.DataFrame()
                for dr in dirs:
                    print(f"Processing {dr}")
                    features_subject_folder = path + '/' + dr + f"/WindowedCsv_{dim_seconds}_{shift_seconds}_stand{stand}_norm{norm}"
                    if os.path.exists(features_subject_folder):
                        feature_files = os.listdir(features_subject_folder)
                        for file in feature_files:
                            if file.startswith("S") and file.endswith(".csv") and datatype in file:
                                df = pd.read_csv(features_subject_folder + "/" + file, sep=';')
                                df["Subject"] = dr.split("S")[1]
                                total_df = pd.concat([total_df, df], axis=0, ignore_index=True)

                subject_column = total_df["Subject"]
                if datatype != "Dante":
                    total_df.drop(columns=["Subject"], inplace=True)
                    selectedTotalDataDf = featureSelection(total_df, 0.8, 0.2)
                    if (datatype != "External"):
                        selectedTotalDataDf.dropna(axis=1, inplace=True)
                    selectedTotalDataDf["Subject"] = subject_column
                    save_dataframes_in_file(selectedTotalDataDf, path + f"/WindowedCsv_{dim_seconds}_{shift_seconds}_stand{stand}_norm{norm}/FeatureSelected_ALL", f"{datatype}_ALL.csv")
                else:
                    save_dataframes_in_file(total_df, path + f"/WindowedCsv_{dim_seconds}_{shift_seconds}_stand{stand}_norm{norm}/FeatureSelected_ALL", f"{datatype}_ALL.csv")
        case _:
            print("Invalid selection type")
            '''
                if len(dfDante) > len(dfExternal):
                    dfDante = dfDante[:len(dfExternal)]
                else:
                    dfExternal = dfExternal[:len(dfDante)]

                total_dante_df = pd.concat([total_dante_df, dfDante], axis=0, ignore_index=True)
                total_external_df = pd.concat([total_external_df, dfExternal], axis=0, ignore_index=True)
                '''

    '''

        SelectedExternalDf = featureSelectionExternal(total_external_df, 0.8, 0.2, total_dante_df)
        save_dataframes_in_file(SelectedExternalDf,drivePath, "All_Selected_External.csv")

        selectedExternalcolumns = SelectedExternalDf.columns

        for dr in dirs:
            features_subject_folder = path + '/' + dr + f"/WindowedCsv_{dim_seconds}_{shift_seconds}_stand{stand}_norm{norm}"
            if os.path.exists(features_subject_folder):
                feature_files = os.listdir(features_subject_folder)
                for file in feature_files:
                    if file.startswith("S") and file.endswith(".csv") and "External" in file:
                        df = pd.read_csv(features_subject_folder + "/" + file, sep=';')
                        df = df[selectedExternalcolumns]
                        #if os.path.exists(features_subject_folder + "/FeatureSelected/" + file + "_ALL"):
                            #os.remove(features_subject_folder + "/FeatureSelected/" + file + "_ALL")
                        save_dataframes_in_file(df, features_subject_folder + "/FeatureSelected", file.replace("External", "External_ALL"))
        '''
