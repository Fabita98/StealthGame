from sklearn.decomposition import PCA
from sklearn.feature_selection import SelectKBest, VarianceThreshold, f_regression
import os
import pandas as pd
import numpy as np
import time

class FeatureSelection:
    def __init__(self, path='D:/University-Masters/Thesis', selection_type="Single", dim_seconds=0.5, shift_seconds=0.25, norm=False, stand=True, subjects_to_exclude=[]):
        self.dim_seconds = dim_seconds
        self.shift_seconds = shift_seconds
        self.norm = norm
        self.stand = stand
        self.path = path
        self.selection_type = selection_type
        self.subjects_to_exclude = subjects_to_exclude

    def feature_selection(self, df, corrth, varth):
        dfselection = self.correlation(df, corrth)
        dfselection = self.variance(dfselection, varth)
        return dfselection

    def feature_selection_external(self, df, corrth, varth, y):
        dfselection = self.correlation(df, corrth)
        dfselection = self.variance(dfselection, varth)
        dfselection = self.stress_selection(dfselection, y)
        return dfselection

    @staticmethod
    def correlation(data, corrth):
        corr_matrix = data.corr().abs()
        upper = corr_matrix.where(np.triu(np.ones(corr_matrix.shape), k=1).astype(bool))
        to_drop = [column for column in upper.columns if any(upper[column].gt(corrth))]
        data.drop(columns=to_drop)
        return data

    @staticmethod
    def variance(data, varth):
        variances = data.var()
        if all(variances < varth):
            print(f"Warning: All features have variance below {varth}.")
            print("Consider lowering the threshold or using a different feature selection method.")
            print(f"Minimum variance: {variances.min()}, Maximum variance: {variances.max()}")
            return data.loc[:, (variances > 0)]

        variance_selector = VarianceThreshold(threshold=varth)
        variance_selected = variance_selector.set_output(transform="pandas").fit_transform(data)
        return variance_selected

    @staticmethod
    def stress_selection(data, y):
        feature_selector = SelectKBest(f_regression, k="all")
        fit = feature_selector.fit(data, y)

        p_values = pd.DataFrame(fit.pvalues_)
        scores = pd.DataFrame(fit.scores_)
        input_variable_names = pd.DataFrame(data.columns)
        summary_stats = pd.concat([input_variable_names, p_values, scores], axis=1)
        summary_stats.columns = ["input_variable", "p_value", "f_score"]
        summary_stats.sort_values(by="p_value", inplace=True)

        p_value_threshold = 0.1
        score_threshold = 5

        selected_variables = summary_stats.loc[
            (summary_stats["f_score"] >= score_threshold) &
            (summary_stats["p_value"] <= p_value_threshold)
        ]
        selected_variables = selected_variables["input_variable"].tolist()
        data = data[selected_variables]
        return data

    @staticmethod
    def pca(data, pca_components):
        n_samples, n_features = data.shape
        if pca_components > n_samples or pca_components > n_features:
            n_components = min(pca_components, n_samples, n_features)
            if n_samples < n_features:
                print(f"PCA number reduced because of samples #{n_samples}")
            else:
                print(f"PCA number reduced because of features #{n_features}")
        else:
            n_components = pca_components

        pca = PCA(n_components=n_components)
        pca_result = pca.fit_transform(data)
        return pca_result

    @staticmethod
    def save_dataframes_in_file(dataframe, subject_folder, name):
        if not os.path.exists(subject_folder):
            os.makedirs(subject_folder)
        dataframe.to_csv(os.path.join(subject_folder, name), index=False, sep=';')

    def main(self):
        path = self.path
        dirs = sorted(list(filter(lambda x: x[0] == 'S', os.listdir(path))), key=lambda x: int(x[1:]))
        dirs = [dr for dr in dirs if dr not in self.subjects_to_exclude]

        import warnings
        warnings.filterwarnings("ignore")

        match self.selection_type:
            case "Single":
                for dr in dirs:
                    features_subject_folder = f"{path}/{dr}/WindowedCsv_{self.dim_seconds}_{self.shift_seconds}_stand{self.stand}_norm{self.norm}"
                    if os.path.exists(features_subject_folder):
                        feature_files = os.listdir(features_subject_folder)
                        for file in feature_files:
                            if file.startswith("S") and file.endswith(".csv") and "Dante" not in file and "External" not in file:
                                start_time = time.time()
                                print(f"Processing {file}")
                                df = pd.read_csv(f"{features_subject_folder}/{file}", sep=';')
                                selected_df = self.feature_selection(df, 0.8, 0.2)
                                self.save_dataframes_in_file(selected_df, f"{features_subject_folder}/FeatureSelected", file)
                                print(f"{file} done in {time.time() - start_time}")
            case "Multiple":

                for dr in dirs:
                    features_subject_folder = f"{path}/{dr}/WindowedCsv_{self.dim_seconds}_{self.shift_seconds}_stand{self.stand}_norm{self.norm}"
                    print(f"Processing {dr}")
                    total_data_df = pd.DataFrame()

                    total_columns = 0
                    if os.path.exists(features_subject_folder):
                        feature_files = os.listdir(features_subject_folder)
                        for file in feature_files:
                            if file.startswith("S") and file.endswith(".csv") and "Dante" not in file and "External" not in file:
                                df = pd.read_csv(f"{features_subject_folder}/{file}", sep=';')
                                print(f"Processing {file}")
                                total_data_df = pd.concat([total_data_df.reset_index(drop=True), df.reset_index(drop=True)], axis=1)
                                print(f"Total columns: {df.shape[1]}")
                                total_columns += df.shape[1]
                        selected_total_data_df = self.feature_selection(total_data_df, 0.8, 0.2)
                        self.save_dataframes_in_file(selected_total_data_df, f"{features_subject_folder}/FeatureSelected", "Data_ALL.csv")
                        print(f"Total columns: {total_columns} -> {selected_total_data_df.shape[1]}")
            case "ForDataTypes":
                datatypes = ["Dante", "External", "Movement", "Button", "Face", "Eye"]
                for datatype in datatypes:
                    print(f"Processing {datatype}")
                    total_df = pd.DataFrame()
                    for dr in dirs:
                        print(f"Processing {dr}")
                        features_subject_folder = f"{path}/{dr}/WindowedCsv_{self.dim_seconds}_{self.shift_seconds}_stand{self.stand}_norm{self.norm}"
                        if os.path.exists(features_subject_folder):
                            feature_files = os.listdir(features_subject_folder)
                            for file in feature_files:
                                if file.startswith("S") and file.endswith(".csv") and datatype in file:
                                    df = pd.read_csv(f"{features_subject_folder}/{file}", sep=';')
                                    df["Subject"] = dr.split("S")[1]
                                    total_df = pd.concat([total_df, df], axis=0, ignore_index=True)

                    subject_column = total_df["Subject"]
                    if datatype != "Dante":
                        total_df.drop(columns=["Subject"], inplace=True)
                        selected_total_data_df = self.feature_selection(total_df, 0.8, 0.2)
                        if datatype != "External":
                            selected_total_data_df.dropna(axis=1, inplace=True)
                        selected_total_data_df["Subject"] = subject_column
                        self.save_dataframes_in_file(selected_total_data_df, f"{path}/WindowedCsv_{self.dim_seconds}_{self.shift_seconds}_stand{self.stand}_norm{self.norm}/FeatureSelected_ALL", f"{datatype}_ALL.csv")
                    else:
                        self.save_dataframes_in_file(total_df, f"{path}/WindowedCsv_{self.dim_seconds}_{self.shift_seconds}_stand{self.stand}_norm{self.norm}/FeatureSelected_ALL", f"{datatype}_ALL.csv")
            case _:
                print("Invalid selection type")


if __name__ == "__main__":
    featureSelection = FeatureSelection(process_movement=True, process_eye=True, process_face=True, process_button=False)
    featureSelection.main()