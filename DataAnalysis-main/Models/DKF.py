import inspect
import os
from pathlib import Path
import time
import warnings
from matplotlib import pyplot as plt
import pandas as pd
import numpy as np
from sklearn.preprocessing import MinMaxScaler
import tensorflow as tf
from sklearn.model_selection import train_test_split as skl_tt_split
from scipy.signal import savgol_filter
from scipy.stats import pearsonr

import numpy as np
import sklearn as skl
from numpy.linalg import *

# Path da dove prende i csv
dataPath = 'data'

# Path dove salva i csv
savePath = 'models'
SusiDrivePath = 'G:/.shortcut-targets-by-id/1wNGSxajmNG6X6ORVLNxGRZkrJJVw02FA/Test'
FradrivePath = '/mnt/g/.shortcut-targets-by-id/1wNGSxajmNG6X6ORVLNxGRZkrJJVw02FA/Test'
AlienPath = 'F:/Data_Analysis'
windowsPath = "/WindowedCsv_0.5_0.25_standFalse_normFalse/"
subjects_to_exclude = ['S25', 'S100', 'S101']
DataType = ['Button', 'Face', 'Eye', 'Movement','Dante', 'External']

class DiscriminativeKalmanFilter(skl.base.BaseEstimator):
    """
    Implements the Discriminative Kalman Filter as described in Burkhart, M.C.,
    Brandman, D.M., Franco, B., Hochberg, L.R., & Harrison, M.T.'s "The
    discriminative Kalman filter for Bayesian filtering with nonlinear and
    nongaussian observation models." Neural Comput. 32(5), 969–1017 (2020).
    """

    def __init__(
        self,
        Α=None,
        Γ=None,
        S=None,
        f=None,
        Q=None,
        μₜ=None,
        Σₜ=None,
    ):
        self.Α = Α  # from eq. (2.1b)
        self.Γ = Γ  # from eq. (2.1b)
        self.S = S  # from eq. (2.1a)
        self.f = lambda x: f(x)  # from eq. (2.2)
        self.Q = lambda x: Q(x)  # from eq. (2.2)
        self.μₜ = μₜ  # from eq. (2.6)
        self.Σₜ = Σₜ  # from eq. (2.6)
        self.d = Α.shape[0]  # as in eq. (2)

    def stateUpdate(self):
        """
        calculates the first 2 lines of eq. (2.7) in-place
        """
        self.μₜ = self.Α @ self.μₜ
        self.Σₜ = self.Α @ self.Σₜ @ self.Α.T + self.Γ

    def measurementUpdate(self, newMeasurement):
        """
        calculates the last 2 lines of eq. (2.7)
        :param newMeasurement: new observation
        """
        Qxₜ = self.Q(newMeasurement)
        fxₜ = self.f(newMeasurement)

        if not np.all(eigvals(inv(Qxₜ) - self.S)> 1e-6):
            Qxₜ = inv(inv(Qxₜ) + self.S)
        newPosteriorCovInv = inv(self.Σₜ) + inv(Qxₜ) -self.S
        self.μₜ = np.mat(
            solve(
                newPosteriorCovInv,
                solve(self.Σₜ, self.μₜ) + solve(Qxₜ, fxₜ),
            )
        )
        self.Σₜ = inv(newPosteriorCovInv)

    def predict(self, newMeasurement):
        """
        performs stateUpdate() and measurementUpdate(newMeasurement)
        :param newMeasurement: new observation
        :return: new posterior mean as in eq. (2.7)
        """
        self.stateUpdate()
        self.measurementUpdate(newMeasurement)
        return self.μₜ

class DKF:    
    def smoothing(self, df):
        df_filtered = savgol_filter(df, 16, 2, mode = 'nearest')
        return df_filtered

    def split_data_and_labels(self, all_features_df):
        X = []
        y = []
        
        y = all_features_df['stress_label'].values
        subjects = all_features_df['subject']
        
        X_df = all_features_df.drop(['stress_label', 'game_section', 'subject'], axis=1)
        X_df.reset_index(drop=True, inplace=True)
                
        # Iterate over each row
        for index, rows in X_df.iterrows():
            my_list = []
            for col in X_df.columns:
                # Create list for the current row (all the columns values)
                my_list.append(rows[col])
                
            # append the list to the final list
            X.append(my_list)

        return X, y, subjects
            
    def train_and_evaluate(self, X, y, subjects, savePath):
        sub_X = []
        sub_y = []
        #print((pd.concat([pd.DataFrame(X), subjects], axis = 1)))
        group_subject = (pd.concat([pd.DataFrame(X), subjects], axis = 1)).groupby('Subject')
        group_labels =  pd.DataFrame(y).groupby('Subject')

        scaler = MinMaxScaler(feature_range=(0, 1))
        
        for name, subject in group_subject:
            subject_df = subject.drop(['Subject'], axis = 1)
            subject_df = (subject_df.apply(self.smoothing))

            subject_df = scaler.fit_transform(subject_df)

            sub_X.append(subject_df)
        
        for name, labels in group_labels:

            labels_df = labels.drop(['Subject'], axis = 1)
            labels_df = labels_df.apply(self.smoothing)
            labels_df = scaler.fit_transform(labels_df)
            sub_y.append(labels_df)

        all_pred = []
        all_real = []
        avg_rmse = []
        avg_crl = []

        for sub_i in range(len(sub_X)):
            
            x = np.concatenate(sub_X, axis = 0)
            z = np.concatenate(sub_y, axis = 0)
     
            # dimensions of latent states and observations, respectively
            dz, dx = z.shape[1], x.shape[1]

            x_test = sub_X[sub_i]
            z_test = sub_y[sub_i]
            print(f"data test shape: {x_test.shape}")
            print(f"labels test shape: {z_test.shape}")
            temp = sub_X.copy()
            del temp[sub_i]
            x_train = np.concatenate(temp, axis = 0)

            temp = sub_y.copy()
            del temp[sub_i]
            z_train = np.concatenate(temp, axis = 0)

            n_test = len(x_test)
            print(f"training data shape: {x_train.shape}")
            print(f"training labels shape: {z_train.shape}")
            # learn state model parameters A & Gamma from Eq. (2.1b)
            A0 = np.linalg.lstsq(
                z_train[1:, :],
                z_train[:-1, :],
            )[0]
            Gamma0 = np.mat(
                np.cov(
                    z_train[1:, :] - z_train[:-1, :] @ A0,
                    rowvar=False,
                )
            )

            # split training set in order to train f() and Q() from Eq. (2.2) separately
            x_train_mean, x_train_covariance, z_train_mean, z_train_covariance = skl_tt_split(
                x_train, z_train, train_size=0.9
            )

            # learn f() as a neural network
            mean_model = tf.keras.models.Sequential(
                [
                    tf.keras.layers.Dense(32, activation="relu"),
                    tf.keras.layers.Dropout(0.1),
                    tf.keras.layers.Dense(1),
                ]
            )
            mean_model.compile(optimizer="adam", loss="mean_squared_error")
            mean_model.fit(x_train_mean, z_train_mean, epochs=50)
            fx = lambda x: mean_model.predict(x.reshape(-1, dx))[0].reshape(dz, 1)

            # learn Q() as a constant on held-out training data

            z_train_preds = np.array(mean_model.predict_on_batch(x_train_covariance))
            cov_est = np.zeros((dz, dz))
            for i in range(z_train_preds.shape[0]):
                resid_i = (np.mat((z_train_preds[i, :] - z_train_covariance[i, :])).reshape(dz, 1))
                cov_est += np.matmul(resid_i, resid_i.T) / z_train_preds.shape[0]
            Qx = lambda x: cov_est
            print(np.cov(z_train.T))
            # initialize DKF using learned parameters
            f0, Q0 = fx(x_test[0, :]), Qx(x_test[0, :])
            DKF = DiscriminativeKalmanFilter(
                Α=A0,
                Γ=Gamma0,
                S=np.cov(z_train.T),
                f=fx,
                Q=Qx,
                μₜ=f0,
                Σₜ=Q0,
            )

            # perform filtering
            z_preds = np.zeros_like(z_test)
            z_preds[0, :] = f0.reshape(dz)
            print(f"z_preds shape: {z_preds.shape}")
            print(z_preds)
            for i in range(1, len(x_test)):
                z_preds[i, :] = DKF.predict(x_test[i, :]).flatten()
            #z_preds[i, :] = DKF.predict(x_test[i, :]).flatten()
            
            rmse = np.sqrt(np.mean(np.square(z_test - z_preds))) / np.sqrt(np.mean(np.square(z_test)))
            avg_rmse.append(rmse)
            
            # handle output
            print(
                "normalized rmse",
                rmse,
            )

            all_pred.append(z_preds)
            all_real.append(z_test)
        
            crl = pearsonr(z_preds.flatten(), z_test.flatten())[0]
            avg_crl.append(crl)

            self.plot_results(z_test, z_preds, crl, f"DKF, s{str(sub_i)}")
            self.saveFigures(str(sub_i),  f"DKF", savePath)

            # è così che si salva l'excel con i risultati di stress?
            output_folder = f"{savePath}{windowsPath}FeatureSelected/"
            if not os.path.exists(output_folder):
                os.makedirs(output_folder)
            excel_file_results = os.path.join(output_folder, 'DKFkalman_results_stress.xlsx')
            
            df_dkf = pd.DataFrame(z_preds)
            dkf_stress_pivot = df_dkf.pivot(index=str(sub_i))

            # Check if the Excel file exists
            if not os.path.exists(excel_file_results):
                # Save DataFrames to a new Excel file
                with pd.ExcelWriter(excel_file_results) as writer:
                    pd.DataFrame.to_excel(writer, sheet_name='DKF_stress')
            else:
                # Load existing data
                with pd.ExcelFile(excel_file_results) as xls:
                    # Check and update Baseline sheet
                    if 'DKF_stress' in xls.sheet_names:
                        existing_dkf = pd.read_excel(xls, sheet_name='DKF_stress', index_col=0)
                        updated_dkf = existing_dkf.combine_first(dkf_stress_pivot)
                    else:
                        updated_dkf = dkf_stress_pivot

                    # Check and update Input sheet

                # Save updated data back to Excel
                with pd.ExcelWriter(excel_file_results, mode='a', engine='openpyxl', if_sheet_exists='replace') as writer:
                    updated_dkf.to_excel(writer, sheet_name='DKF_stress')
        
        all_pred = np.concatenate(all_pred, axis = 0)
        all_real = np.concatenate(all_real, axis = 0)

        # handle output
        print(" normalized rmse",
                avg_rmse)
        print(
                "average normalized rmse",
                np.sum(avg_rmse)/len(avg_rmse),
        )
        
        crl_tot = np.sum(avg_crl)/len(avg_crl)
        self.plot_results(all_real, all_pred, crl_tot, f"DKF, TOTAL")
        self.saveFigures(
            
            
            "TOTAL",  f"DKF", savePath)
        
    def plot_results(self, z_test, z_preds, crl, title):

        fig, axs = plt.subplots()
        fig.set_size_inches(20, 10)
        fig.subplots_adjust(top=0.8, bottom=0.2) 

        plt.figtext(0.1, 0.05, "correlation between actual and DKF predicted is " + str(round(crl, 2)), fontsize = 15)
        plt.plot(z_test)
        plt.plot(z_preds, color='red', linestyle='dashed')
        axs.legend(('actual', 'predicted'), fontsize=20)

        axs.set_xlabel("Time", fontsize=20)
        axs.set_ylabel("Amplitude", fontsize=20)
        axs.set_title(title, fontsize=20)  
        axs.tick_params(axis='both', labelsize=18)


    def saveFigures(self, subject_id, filter_type, savePath):
        #Qui dipende dove le vuoi salvare

        results_folder = f"Dkalman_results_{datatype}"
        results_folder_path = savePath + '/' + subject_id + windowsPath + "FeatureSelected" + '/' + results_folder
        if not os.path.exists(results_folder_path):
            os.makedirs(results_folder_path)

        # Salva la figura con il nome del soggetto
        filename = f"{subject_id}_{filter_type}.png"
        filepath = os.path.join(results_folder_path, filename)
        plt.get_current_fig_manager().full_screen_toggle()
        plt.savefig(filepath, dpi = 300)
        plt.close()

            
if __name__ == '__main__':
    DKFRegression = DKF()
    #dirs = os.listdir(drivePath)

    warnings.filterwarnings("ignore")

    #dirs = [f for f in dirs if f[0] == 'S' and f not in subjects_to_exclude]
    #dirs = sorted(dirs, key=lambda x: int(x[1:]))
    path = FradrivePath

    dir = f"{path}{windowsPath}"

    dir_all = "FeatureSelected_ALL/"

    subjects_dfs = []
    data_dfs = {typology: [] for typology in ['Button', 'Face', 'Eye', 'Movement']}
    dante_dfs = []

    dante_df =pd.read_csv(f"{dir}{dir_all}Dante_ALL.csv" , sep=';')
    movement = pd.read_csv(f"{dir}{dir_all}Movement_ALL.csv", sep=';')

    subject_lengths = movement.groupby('Subject').size()




    #subject_lengths = dante_df.groupby('Subject').size()
    truncated_dfs = []

    # Tronca le righe in dante_df per ogni soggetto
    for subject, length in subject_lengths.items():
        subject_rows = dante_df[dante_df['Subject'] == subject]
        truncated_rows = subject_rows.head(length)
        truncated_dfs.append(truncated_rows)


    # Combina tutti i DataFrame troncati
    concatenated_dante = pd.concat(truncated_dfs, ignore_index = True)
    concatenated_subjects = concatenated_dante[["Subject"]]

    dir_files = os.listdir(f"{path}{windowsPath}{dir_all}")
    for file in dir_files:
        if file.endswith(".csv") and "External" not in file and "Dante" not in file:
            X = pd.read_csv(f"{path}{windowsPath}{dir_all}{file}", sep=';')
            # Tronca le righe in X per ogni soggetto
            truncated_data = []
            for subject, length in subject_lengths.items():
                subject_rows = X[X['Subject'] == subject]
                truncated_rows = subject_rows.head(length)
                truncated_data.append(truncated_rows)
            X = pd.concat(truncated_data, ignore_index = True)
            datatype = file.split('_')[0].replace('.csv', '')

            # Append the data to corresponding lists
            if datatype in data_dfs:
                data_dfs[datatype].append(X.drop(columns=["Subject"], axis = 1))


    start_time = time.time()
    # Call the filter function

    for datatype in data_dfs:
        DKFRegression.train_and_evaluate(data_dfs[datatype][0], concatenated_dante, concatenated_subjects, path)
    print("--- %s seconds ---" % (time.time() - start_time))
