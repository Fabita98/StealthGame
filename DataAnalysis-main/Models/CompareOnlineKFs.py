import inspect
import os
import time
import warnings
from pandas import ExcelWriter, ExcelFile
import pandas as pd
from pathlib import Path
from sklearn.preprocessing import MinMaxScaler
from pykalman import KalmanFilter
from scipy.signal import savgol_filter
from scipy.stats import pearsonr
import numpy as np
from matplotlib import pyplot as plt
import matplotlib.gridspec as gridspec

drivePath = 'D:/University-Masters/Thesis'
susiDrivePath = 'G:/.shortcut-targets-by-id/1wNGSxajmNG6X6ORVLNxGRZkrJJVw02FA/Test'
AlienPath = 'F:/Data_Analysis'
# windowsPath = "/WindowedCsv_0.5_0.25_standFalse_normFalse/"
windowsPath = "/WindowedCsv_0.5_0.25_standTrue_normFalse/"
subjects_to_exclude = [f"S{i}" for i in range(0, 21)]  + ['S25', 'S100', 'S101']  + [f"S{i}" for i in range(22, 29)]
# Path dei csv
# dataPath = 'data'

subjects = {}
subjects_om = {}
subjects_tm = {}

# Numero di righe del df che vengono fornite in input al KF ad ogni iterazione
block_size = 5


class Comparison:

    def split_data_and_labels(self, all_features_df):
        X = []
        y = []
        subjects = all_features_df['subject']

        # Qui ci vanno gli external data
        u = all_features_df['game_section'].values

        y = all_features_df['stress_label'].values
        # Non ricordo se i csv sono tutti distinti, nel caso questa funzione va riscritta
        X_df = all_features_df.drop(['stress_label', 'game_section', 'subject'], axis=1)

        for index, rows in X_df.iterrows():
            my_list = []
            for col in X_df.columns:
                my_list.append(rows[col])
            X.append(my_list)

        return X, y, u, subjects

    # Filtro di Kalman base, online, con test con le combinazioni di matrici
    def apply_online_baseline_kalman(self, dz, dx, x_test, transition_matrix, observation_matrix):
        transition_matrix = transition_matrix if transition_matrix is not None else np.full(dz, 1)
        observation_matrix = observation_matrix if observation_matrix is not None else np.ones((dx, dz))

        kf = KalmanFilter(n_dim_state=dz, n_dim_obs=dx)
        kf.transition_matrices = transition_matrix
        kf.observation_matrices = observation_matrix
        kf.transition_covariance = np.random.rand(dz)
        kf.observation_covariance = np.random.rand(dx)

        z_preds = np.zeros(shape=(len(x_test), dz))
        cov = np.zeros(shape=(len(x_test), dz))

        for i in range(len(x_test)):
            if i == 0:
                z_preds[i], cov[i] = kf.filter_update(filtered_state_mean=np.zeros(dz),
                                                      filtered_state_covariance=np.eye(dz), observation=x_test[i])
            else:
                z_preds[i], cov[i] = kf.filter_update(filtered_state_mean=z_preds[i - 1],
                                                      filtered_state_covariance=cov[i - 1], observation=x_test[i])

        return z_preds

    # Filtro di Kalman con input di controllo, online, con test con combinazioni di matrici
    def apply_online_input_kalman(self, dz, dx, x_test, u_test, transition_matrix, observation_matrix):
        transition_matrix = transition_matrix if transition_matrix is not None else np.full(dz, 1)
        observation_matrix = observation_matrix if observation_matrix is not None else np.ones((dx, dz))

        kf = KalmanFilter(n_dim_state=dz, n_dim_obs=dx)
        kf.transition_matrices = transition_matrix
        kf.observation_matrices = observation_matrix
        kf.transition_covariance = np.random.rand(dz)
        kf.observation_covariance = np.random.rand(dx)

        z_preds = np.zeros(shape=(len(x_test), dz))
        cov = np.zeros(shape=(len(x_test), dz))

        for i in range(len(x_test)):
            if i == 0:
                z_preds[i], cov[i] = kf.filter_update(filtered_state_mean=np.zeros(dz),
                                                      filtered_state_covariance=np.eye(dz), observation=x_test[i],
                                                      transition_offset=u_test[i])
            else:
                z_preds[i], cov[i] = kf.filter_update(filtered_state_mean=z_preds[i - 1],
                                                      filtered_state_covariance=cov[i - 1], observation=x_test[i],
                                                      transition_offset=u_test[i])
        return z_preds

    # Filtro di Kalman base, online, con in input più di una riga di df, con test con le combinazioni di matrici
    def apply_online_baseline_kalman_group(self, dz, dx, x_test, block_size, transition_matrix, observation_matrix):
        transition_matrix = transition_matrix if transition_matrix is not None else np.full(dz, 1)
        observation_matrix = observation_matrix if observation_matrix is not None else np.ones((dx, dz))

        kf = KalmanFilter(n_dim_state=dz, n_dim_obs=dx)
        kf.transition_matrices = transition_matrix
        kf.observation_matrices = observation_matrix
        kf.transition_covariance = np.random.rand(dz)
        kf.observation_covariance = np.random.rand(dx)

        num_blocks = len(x_test) // block_size
        if len(x_test) % block_size != 0:
            num_blocks += 1

        z_preds = np.zeros((num_blocks, dz))
        cov = np.zeros((num_blocks, dz))

        for i in range(0, len(x_test), block_size):
            observations = x_test[i:i + block_size]
            z_preds_block = np.zeros(shape=(block_size, dz))
            cov_block = np.zeros(shape=(block_size, dz))

            for j in range(block_size):
                if i + j < len(x_test):
                    if i == 0:
                        z_preds_block[j], cov_block[j] = kf.filter_update(filtered_state_mean=np.zeros(dz),
                                                                          filtered_state_covariance=np.eye(dz),
                                                                          observation=observations[j])
                    else:
                        z_preds_block[j], cov_block[j] = kf.filter_update(filtered_state_mean=z_preds_block[j - 1],
                                                                          filtered_state_covariance=cov_block[j - 1],
                                                                          observation=observations[j])

            z_preds[i // block_size] = np.mean(z_preds_block, axis=0)
            cov[i // block_size] = np.mean(cov_block, axis=0)

        return z_preds

    # Filtro di Kalman con input di controllo, online, con in input più di una riga di df, con test con le combinazioni di matrici
    def apply_online_input_kalman_group(self, dz, dx, x_test, u_test, block_size, transition_matrix,
                                        observation_matrix):
        transition_matrix = transition_matrix if transition_matrix is not None else np.full(dz, 1)
        observation_matrix = observation_matrix if observation_matrix is not None else np.ones((dx, dz))

        kf = KalmanFilter(n_dim_state=dz, n_dim_obs=dx)
        kf.transition_matrices = transition_matrix
        kf.observation_matrices = observation_matrix
        kf.transition_covariance = np.random.rand(dz)
        kf.observation_covariance = np.random.rand(dx)

        num_blocks = len(u_test) // block_size
        remainder = len(u_test) % block_size
        if remainder != 0:
            num_blocks += 1

        z_preds = np.zeros((num_blocks, dz))
        cov = np.zeros((num_blocks, dz))

        for i in range(0, len(x_test), block_size):
            observations = x_test[i:i + block_size]
            u_block = u_test[i:i + block_size]
            z_preds_block = np.zeros(shape=(block_size, dz))
            cov_block = np.zeros(shape=(block_size, dz))

            for j in range(block_size):
                if i + j < len(x_test):
                    '''
                    if i == 0:
                        z_preds_block[j], cov_block[j] = kf.filter_update(filtered_state_mean=np.zeros(dz), filtered_state_covariance=np.eye(dz), observation=observations[j], transition_offset=u_block[j])
                    else:
                        z_preds_block[j], cov_block[j] = kf.filter_update(filtered_state_mean=z_preds_block[j-1], filtered_state_covariance=cov_block[j-1], observation=observations[j], transition_offset=u_block[j])
                    '''
                    z_preds_array = []
                    cov_array = []
                    for col in range(len(u_block[j])):
                        if i == 0:
                            z_preds_block[j], cov_block[j] = kf.filter_update(filtered_state_mean=np.zeros(dz),
                                                                              filtered_state_covariance=np.eye(dz),
                                                                              observation=observations[j],
                                                                              transition_offset=u_block[j][col])
                            z_preds_array.append(z_preds_block[j])
                            cov_array.append(cov_block[j])
                        else:
                            z_preds_block[j], cov_block[j] = kf.filter_update(filtered_state_mean=z_preds_block[j - 1],
                                                                              filtered_state_covariance=cov_block[
                                                                                  j - 1],
                                                                              observation=observations[j],
                                                                              transition_offset=u_block[i][col])
                            z_preds_array.append(z_preds_block[i])
                            cov_array.append(cov_block[i])
                    z_preds[i] = np.mean(z_preds_array, axis=0)
                    cov[i] = np.mean(cov_array, axis=0)
            z_preds[i // block_size] = np.mean(z_preds_block, axis=0)
            cov[i // block_size] = np.mean(cov_block, axis=0)

        return z_preds

    def smoothing(self, df):
        df_filtered = savgol_filter(df, 64, 2, mode='nearest')
        return df_filtered

    def compareKalmans_online(self, x, z, u, subject_id, datatype, external_col):

        scaler = MinMaxScaler(feature_range=(0, 1))
        u.fillna(0, inplace=True)
        # x_subject = pd.DataFrame(x)
        # z_subject = pd.DataFrame(z)
        # u_subject = pd.DataFrame(u)
        # print(x)
        x_subject = scaler.fit_transform(x)
        u_subject = scaler.fit_transform(u)
        z_subject = scaler.fit_transform(z)
        dz, dx, du = z_subject.shape[1], x_subject.shape[1], u_subject.shape[1]
        # Genera tutte le combinazioni di matrici di transizione e di osservazione
        transition_matrices = [np.full(dz, 1)]
        observation_matrices = [np.full((dx, dz), 1)]

        results_dict = {}

        for transition_matrix in transition_matrices:
            for observation_matrix in observation_matrices:
                matrix_results = {}

                t_m = ""
                o_m = ""

                if (transition_matrix == np.full(dz, 1)).all():
                    t_m = "TM = ones"
                else:
                    t_m = "TM = random"

                if (observation_matrix == np.full((dx, dz), 1)).all():
                    o_m = "OM = ones"
                else:
                    o_m = "OM = random"

                # Applica il filtro di Kalman di base
                z_preds_baseline = self.apply_online_baseline_kalman(z_subject.shape[1], x_subject.shape[1], x_subject,
                                                                     transition_matrix=transition_matrix,
                                                                     observation_matrix=observation_matrix)
                z_preds_baseline = scaler.fit_transform(pd.DataFrame(z_preds_baseline))
                crl_baseline = pearsonr(np.concatenate(z_preds_baseline, axis=0), np.concatenate(z_subject, axis=0))[0]
                matrix_results["Baseline"] = crl_baseline

                # Applica il filtro di Kalman con input
                z_preds_input = self.apply_online_input_kalman(z_subject.shape[1], x_subject.shape[1], x_subject,
                                                               u_subject, transition_matrix=transition_matrix,
                                                               observation_matrix=observation_matrix)
                z_preds_input = scaler.fit_transform(pd.DataFrame(z_preds_input))
                crl_input = pearsonr(np.concatenate(z_preds_input, axis=0), np.concatenate(z_subject, axis=0))[0]
                matrix_results["Input"] = crl_input
                '''
                # Applica filtro di Kalman baseline con più righe di features
                z_preds_baseline_group= self.apply_online_baseline_kalman_group(z_subject.shape[1], x_subject.shape[1], x_subject, block_size = block_size, transition_matrix=transition_matrix, observation_matrix=observation_matrix)
                z_preds_baseline_group = scaler.fit_transform(pd.DataFrame(z_preds_baseline_group) )

                # Applica filtro di Kalman input con più righe di features
                z_preds_input_group = self.apply_online_input_kalman_group(z_subject.shape[1], x_subject.shape[1], x_subject, u_subject, block_size = block_size, transition_matrix=transition_matrix, observation_matrix=observation_matrix)
                z_preds_input_group = scaler.fit_transform(pd.DataFrame(z_preds_input_group))
                
                # Dividi z in blocchi di dimensione block_size
                blocks = [z_subject[i:i+block_size] for i in range(0, len(z_subject), block_size)]
                
                # Calcola la media per ogni blocco e crea z_mean
                z_mean = np.array([np.mean(block) for block in blocks])
                '''
                # Calcola RMSE
                rmse_baseline = np.sqrt(np.mean(np.square(z_subject - z_preds_baseline))) / np.sqrt(
                    np.mean(np.square(z_subject)))
                rmse_input = np.sqrt(np.mean(np.square(z - z_preds_input))) / np.sqrt(np.mean(np.square(z)))
                '''     
                crl_baseline_group = pearsonr(np.concatenate(z_preds_baseline_group, axis=0), z_mean)[0]
                matrix_results["Baseline Group"] = crl_baseline_group
                crl_input_group = pearsonr(np.concatenate(z_preds_input_group, axis=0), z_mean)[0]
                matrix_results["Input Group"] = crl_input_group
                '''

                # Visualizza i risultati
                print("Subject:", subject_id)
                print("RMSE (Baseline):", rmse_baseline)
                print("RMSE (Input):", rmse_input)
                print("CRL (Baseline):", crl_baseline)
                print("CRL (Input):", crl_input)

                smoothed_z_preds_baseline = np.apply_along_axis(self.smoothing, 1, z_preds_baseline)
                smoothed_z_preds_input = np.apply_along_axis(self.smoothing, 1, z_preds_input)

                # Visualizza i grafici
                # self.plot_results(z_mean, z_preds_baseline_group, z_preds_input_group, crl_baseline_group, crl_input_group, u, f"Kalman blocks_{t_m}_{o_m}, s{subject_id}")
                # self.saveFigures(subject_id,  f"Kalman blocks_{t_m}_{o_m}", datatype)
                self.plot_results(z_subject, smoothed_z_preds_baseline, smoothed_z_preds_input, crl_baseline, crl_input,
                                  u_subject,
                                  f"Kalman_{t_m}_{o_m}, s{subject_id}")
                self.saveFigures(subject_id, f"Kalman_{t_m}_{o_m}", datatype, u.columns[0])

                # In order to save crls in an excel file
                if subject_id not in subjects:
                    subjects[subject_id] = {}
                key = (t_m, o_m)
                if key not in subjects[subject_id]:
                    subjects[subject_id][key] = {}
                if datatype not in subjects[subject_id][key]:
                    subjects[subject_id][key][datatype] = {}
                if external_col not in subjects[subject_id][key][datatype]:
                    subjects[subject_id][key][datatype][external_col] = {}

                subjects[subject_id][key][datatype][external_col]['Baseline'] = crl_baseline
                subjects[subject_id][key][datatype][external_col]['Input'] = crl_input

                return crl_baseline, crl_input, smoothed_z_preds_baseline, smoothed_z_preds_input

    def plot_results(self, z_test, z_preds_baseline, z_preds_input, crl_baseline, crl_input, u, title):
        kf_type = ""
        kfInput_type = ""

        if ("blocks" in title):
            kf_type = "KF blocks"
            kfInput_type = "KF input blocks"
        else:
            kf_type = "KF"
            kfInput_type = "KF input"

        fig, axs = plt.subplots()
        fig.set_size_inches(20, 10)
        fig.subplots_adjust(top=0.8, bottom=0.2)

        plt.figtext(0.1, 0.05, "correlation between actual and " + kf_type + " predicted is " + str(round(crl_baseline,
                                                                                                          2)) + '\n' + "correlation between actual and " + kfInput_type + " with control input prediced is " + str(
            round(crl_input, 2)) + '\n', fontsize=15)
        plt.plot(z_test)
        plt.plot(z_preds_baseline, color='red', linestyle='dashed')
        plt.plot(z_preds_input, color='green', linestyle='dotted')
        axs.legend(('actual', kf_type, kfInput_type), fontsize=20)

        axs.set_xlabel("Time", fontsize=20)
        axs.set_ylabel("Amplitude", fontsize=20)
        axs.set_title(title, fontsize=20)
        axs.tick_params(axis='both', labelsize=18)

    def plot_kalman_results(self, subject, dante_df, datatype_results, external_col):
        # Create the results folder if it does not exist
        results_folder_path = f"{drivePath}/{subject}{windowsPath}FeatureSelected/kalman_results_all_datatypes/{external_col}"
        if not os.path.exists(results_folder_path):
            os.makedirs(results_folder_path)

        # Create figures and axes
        # Plot settings for Baseline
        fig1 = plt.figure(figsize=(20, 12))
        gs1 = gridspec.GridSpec(2, 1, height_ratios=[4, 1])
        ax1 = fig1.add_subplot(gs1[0])
        ax1_text = fig1.add_subplot(gs1[1])

        fig2 = plt.figure(figsize=(20, 12))
        gs2 = gridspec.GridSpec(2, 1, height_ratios=[4, 1])
        ax2 = fig2.add_subplot(gs2[0])
        ax2_text = fig2.add_subplot(gs2[1])

        # Plot Dante data
        ax1.plot(dante_df, label='Dante', color='black', linestyle='--')
        ax2.plot(dante_df, label='Dante', color='black', linestyle='--')

        # Clear text boxes
        ax1_text.axis('off')
        ax2_text.axis('off')

        # Initialize dictionaries to store correlation positions
        baseline_positions = {}
        input_positions = {}

        # Define colors for different datatypes
        colors = plt.cm.viridis(np.linspace(0, 1, len(datatype_results)))

        # Plot datatype results and calculate correlations
        for i, (datatype, results) in enumerate(datatype_results.items()):
            baseline_results = results[external_col]['baseline']
            input_results = results[external_col]['input']
            color = colors[i]

            ax1.plot(baseline_results, label=f'Baseline - Datatype {datatype}', linestyle='--', alpha=0.7, color=color)
            ax2.plot(input_results, label=f'Input - Datatype {datatype}', linestyle='--', alpha=0.7, color=color)

            baseline_positions[datatype] = {}
            input_positions[datatype] = {}

            # Keep track of encountered pairs
            encountered_pairs = set()

            for j, (other_datatype, other_results) in enumerate(datatype_results.items()):
                if datatype == other_datatype:
                    continue

                pair = tuple(sorted((datatype, other_datatype)))  # Sort to ensure unique representation

                if pair in encountered_pairs:
                    continue

                encountered_pairs.add(pair)

                baseline_results2 = other_results[external_col]['baseline']
                input_results2 = other_results[external_col]['input']

                # Ensure data lengths match before correlation
                min_len_baseline = min(len(baseline_results), len(baseline_results2))
                min_len_input = min(len(input_results), len(input_results2))

                baseline_results_trimmed = np.ravel(baseline_results[:min_len_baseline])
                baseline_results2_trimmed = np.ravel(baseline_results2[:min_len_baseline])
                input_results_trimmed = np.ravel(input_results[:min_len_input])
                input_results2_trimmed = np.ravel(input_results2[:min_len_input])

                # Calculate correlations
                baseline_corr, _ = pearsonr(baseline_results_trimmed, baseline_results2_trimmed)
                input_corr, _ = pearsonr(input_results_trimmed, input_results2_trimmed)

                # Store correlation positions
                baseline_positions[datatype][pair] = baseline_corr
                input_positions[datatype][pair] = input_corr

        # Collect correlation strings
        baseline_corr_text = "Baseline Correlations:\n"
        input_corr_text = "Input Correlations:\n"

        # Keep track of encountered pairs
        encountered_pairs = set()

        for datatype in baseline_positions:
            for pair, baseline_corr in baseline_positions[datatype].items():
                datatype1, datatype2 = pair

                # Ensure each pair is processed only once
                if pair in encountered_pairs:
                    continue

                encountered_pairs.add(pair)

                input_corr = input_positions[datatype][pair]
                baseline_corr_text += f"Corr({datatype1}-{datatype2}): {baseline_corr:.2f}\n"
                input_corr_text += f"Corr({datatype1}-{datatype2}): {input_corr:.2f}\n"

        # Annotate Baseline Correlations
        ax1_text.text(0.01, 0.5, baseline_corr_text, va='center', ha='left', fontsize=12, wrap=True)

        # Annotate Input Correlations
        ax2_text.text(0.01, 0.5, input_corr_text, va='center', ha='left', fontsize=12, wrap=True)

        # Finalize and save the Baseline plot
        ax1.set_title(f'Subject {subject} - External {external_col} - Comparison of datatypes Baseline', fontsize=20)
        ax1.set_xlabel("Time", fontsize=20)
        ax1.set_ylabel("Amplitude", fontsize=20)
        ax1.tick_params(axis='both', labelsize=18)
        ax1.legend()
        filename1 = f"{subject}_{external_col}_baseline.png"
        filepath1 = os.path.join(results_folder_path, filename1)
        fig1.savefig(filepath1, dpi=300)
        plt.close(fig1)

        # Finalize and save the Input plot
        ax2.set_title(f'Subject {subject} - External {external_col} - Comparison of datatypes Input', fontsize=20)
        ax2.set_xlabel("Time", fontsize=20)
        ax2.set_ylabel("Amplitude", fontsize=20)
        ax2.tick_params(axis='both', labelsize=18)
        ax2.legend()
        filename2 = f"{subject}_{external_col}_input.png"
        filepath2 = os.path.join(results_folder_path, filename2)
        fig2.savefig(filepath2, dpi=300)
        plt.close(fig2)

    def saveFigures(self, subject_id, filter_type, datatype, external_col):

        results_folder = f"kalman_results_{datatype}/{external_col}"
        results_folder_path = drivePath + '/' + subject_id + windowsPath + "FeatureSelected" + '/' + results_folder
        if not os.path.exists(results_folder_path):
            os.makedirs(results_folder_path)

        # Crea una sottocartella con il nome della funzione che la chiama

        # Salva la figura con il nome del soggetto
        filename = f"{subject_id}_{filter_type}.png"
        filepath = os.path.join(results_folder_path, filename)
        plt.get_current_fig_manager().full_screen_toggle()
        plt.savefig(filepath, dpi=300)
        plt.close()

    def generate_latex_table(self, subjects):
        configurations = [(tm, om) for tm in ['ones', 'random'] for om in ['ones', 'random']]

        latex_table = "\\begin{table}[h]\n"
        latex_table += "\\centering\n"
        latex_table += "\\begin{tabular}{|c|c|c|c|c|c|c|c|c|c|c|c|c|c|c|c|c|}\n"
        latex_table += "\\hline\n"
        latex_table += "& \\multicolumn{4}{c|}{Baseline} & \\multicolumn{4}{c|}{Input} & \\multicolumn{4}{c|}{Baseline Group} & \\multicolumn{4}{c|}{Input Group} \\\\ \n"
        latex_table += "\\hline\n"
        latex_table += "& \\makecell{OM = o \\\\ TM = o} & \\makecell{OM = r \\\\ TM = o} & \\makecell{OM = o \\\\ TM = r} & \\makecell{OM = r \\\\ TM = r} & \\makecell{OM = o \\\\ TM = o} & \\makecell{OM = r \\\\ TM = o} & \\makecell{OM = o \\\\ TM = r} & \\makecell{OM = r \\\\ TM = r} & \\makecell{OM = o \\\\ TM = o} & \\makecell{OM = r \\\\ TM = o} & \\makecell{OM = o \\\\ TM = r} & \\makecell{OM = r \\\\ TM = r} & \\makecell{OM = o \\\\ TM = o} & \\makecell{OM = r \\\\ TM = o} & \\makecell{OM = o \\\\ TM = r} & \\makecell{OM = r \\\\ TM = r} \\\\ \n"
        latex_table += "\\hline\n"

        # Ordina i soggetti per ID crescente
        sorted_subjects = sorted(subjects.items(), key=lambda x: x[0])

        for subject_id, results_dict in sorted_subjects:
            latex_table += f"{subject_id}"
            for filter_type in ['Baseline', 'Input', 'Baseline Group', 'Input Group']:
                for tm, om in configurations:
                    key = (f'TM = {tm}', f'OM = {om}')
                    if key in results_dict:
                        latex_table += f" & {results_dict[key][filter_type]:.2f}"
            latex_table += " \\\\ \n"
            latex_table += "\\hline\n"

        latex_table += "\\end{tabular}\n"
        latex_table += "\\caption{Correlazioni per ogni soggetto e tipologia di filtro}\n"
        latex_table += "\\end{table}\n"

        return latex_table


def save_results_to_excel(subjects, output_folder):
    # Collect all unique datatypes, filter types, and external columns from the results
    '''
    datatypes = set()
    #print(subjects)
    filter_types = ['Baseline', 'Input']
    external_columns = set()
    for results_dict in subjects.values():
        for datatype, filters in results_dict.items():
            datatypes.add(datatype)
            for filter_type in filter_types:
                for external_col in filters.get(filter_type, {}):
                    external_columns.add(external_col)
    datatypes = sorted(datatypes)
    external_columns = sorted(external_columns)

    # Create a DataFrame to store the results
    columns = pd.MultiIndex.from_product([datatypes, filter_types, external_columns], names=['Datatype', 'Filter Type', 'External Column'])
    df_results = pd.DataFrame(index=subjects.keys(), columns=columns)

    # Fill the DataFrame with correlation values
    for subject_id, results_dict in subjects.items():
        for datatype in datatypes:
            for filter_type in filter_types:
                for external_col in external_columns:
                    correlation = results_dict.get(datatype, {}).get(filter_type, {}).get(external_col, None)
                    df_results.at[subject_id, (datatype, filter_type, external_col)] = correlation

    # Save the DataFrame to an Excel file
    output_file = f"{output_folder}/kalman_results_all_subjects.xlsx"
    df_results.to_excel(output_file)
    print(f'Results saved to {output_file}')
    '''
    excel_file = os.path.join(output_folder, 'kalman_results_all_corr.xlsx')

    # Create DataFrames for Baseline and Input
    df_baseline_data = []
    df_input_data = []
    baseline_columns = ['Subject', 'Datatype', 'Correlation']
    input_columns = ['Subject', 'Datatype', 'External Column', 'Correlation']

    # Populate DataFrames
    for subject_id, data in subjects.items():
        for _, datatypes in data.items():
            for datatype, metrics in datatypes.items():
                # Extract Baseline correlation (taking the first one found, assuming they're consistent)
                baseline_value = next(iter(metrics.values()))['Baseline']
                df_baseline_data.append([subject_id, datatype, baseline_value])

                # For Input, record correlations for each external column
                for external_col, values in metrics.items():
                    input_value = values['Input']
                    df_input_data.append([subject_id, datatype, external_col, input_value])

    # Convert to DataFrames
    df_baseline = pd.DataFrame(df_baseline_data, columns=baseline_columns)
    df_input = pd.DataFrame(df_input_data, columns=input_columns)

    # Pivot the DataFrames for better formatting
    df_baseline_pivot = df_baseline.pivot(index='Subject', columns='Datatype', values='Correlation')
    df_input_pivot = df_input.pivot(index='Subject', columns=['Datatype', 'External Column'], values='Correlation')

    # Check if the Excel file exists
    if not os.path.exists(excel_file):
        # Save DataFrames to a new Excel file
        with ExcelWriter(excel_file) as writer:
            df_baseline_pivot.to_excel(writer, sheet_name='Baseline')
            df_input_pivot.to_excel(writer, sheet_name='Input')
    else:
        # Load existing data
        with ExcelFile(excel_file) as xls:
            # Check and update Baseline sheet
            if 'Baseline' in xls.sheet_names:
                existing_baseline = pd.read_excel(xls, sheet_name='Baseline', index_col=0)
                updated_baseline = existing_baseline.combine_first(df_baseline_pivot)
            else:
                updated_baseline = df_baseline_pivot

            # Check and update Input sheet
            if 'Input' in xls.sheet_names:
                existing_input = pd.read_excel(xls, sheet_name='Input', index_col=0, header=[0, 1])
                updated_input = existing_input.combine_first(df_input_pivot)
            else:
                updated_input = df_input_pivot

        # Save updated data back to Excel
        with ExcelWriter(excel_file, mode='a', engine='openpyxl', if_sheet_exists='replace') as writer:
            updated_baseline.to_excel(writer, sheet_name='Baseline')
            updated_input.to_excel(writer, sheet_name='Input')


def transpose_array(array):
    # Reshape the array if it's one-dimensional
    if len(array.shape) == 1:
        array = array.reshape((array.shape[0], 1))

    # Transpose the array
    array_transposed = array.T

    return array_transposed


if __name__ == '__main__':
    comparison = Comparison()
    dirs = os.listdir(drivePath)
    print(dirs)
    warnings.filterwarnings("ignore")

    dirs = [f for f in dirs if f[0] == 'S' and f not in ['S0', 'S1']]
    # dirs = sorted(dirs, key=lambda x: int(x[1:]))

    scaler = MinMaxScaler(feature_range=(0, 1))

    for dir in dirs:
        dante_df = pd.read_csv(drivePath + '/' + dir + windowsPath + f"{dir}_Dante.csv", sep=';')
        selectFeaturesFiles = os.listdir(drivePath + '/' + dir + windowsPath + "FeatureSelected")

        subject = str(dir)
        print(subject)

        datatype_results = {}  # Dizionario per salvare i risultati per ogni datatype
        datatype_to_exclude = ['Dante', 'External', "Eye", "Button", "Data_ALL"]
        subjects = {}

        for file in selectFeaturesFiles:
            if file.startswith("S") and file.endswith(".csv") and not any(
                    exclude in file for exclude in datatype_to_exclude) or "Data_ALL.csv" in file:

                X = pd.read_csv(drivePath + '/' + dir + windowsPath + "FeatureSelected/" + file, sep=';')
                if (not file == f"Data_ALL.csv"):
                    datatype = file.split('_')[1].replace('.csv', '')
                    external = pd.read_csv(
                        #~ drivePath + '/' + dir + windowsPath + "FeatureSelected/" + f"{dir}_External.csv",
                        drivePath + '/' + dir + windowsPath + f"{dir}_External.csv",
                        sep=';')
                else:
                    datatype = "Data_ALL"
                    external = pd.read_csv(
                        drivePath + '/' + dir + windowsPath + "FeatureSelected/" + f"{dir}_External_ALL.csv",
                        sep=';')
                if len(external) <= len(X):
                    X = X.iloc[:len(external)]
                else:
                    external = external.iloc[:len(X)]
                dante_df = dante_df.iloc[:len(X)]
                baseline_results = []
                input_results = []

                # Confronta con tutti i datatype per questa colonna di external
                for col in external.columns:
                    start_time = time.time()
                    crl_baseline, crl_input, pred_baseline, pred_input = comparison.compareKalmans_online(X, dante_df,
                                                                                                          external[
                                                                                                              col].to_frame(),
                                                                                                          subject,
                                                                                                          datatype, col)
                    current_time = time.time()
                    local_time = time.localtime(current_time)

                    hour = local_time.tm_hour
                    minute = local_time.tm_min
                    second = local_time.tm_sec

                    print(f"Time: {hour}:{minute}:{second} --- {subject} - {datatype} - {col} --- {current_time - start_time} seconds")

                    if datatype not in datatype_results:
                        datatype_results[datatype] = {}
                    if col not in datatype_results[datatype]:
                        datatype_results[datatype][col] = {'baseline': [], 'input': []}
                    datatype_results[datatype][col]['baseline'].extend(pred_baseline)
                    datatype_results[datatype][col]['input'].extend(pred_input)

        # Chiama plot_kalman_results per ogni valore della colonna di external

        for col in list(datatype_results.values())[0].keys():
            comparison.plot_kalman_results(subject, dante_df, datatype_results, col)

        # comparison.compareMatrixConfigurationsSelection(X, y, external, subject, withInput=True)
        output_folder = f"{drivePath}/{dir}{windowsPath}FeatureSelected/"
        if not os.path.exists(output_folder):
            os.makedirs(output_folder)

        save_results_to_excel(subjects, output_folder)
    # Genera la tabella latex con i risultati come correlazione tra stress predetto e dante
    # latex_table_online = comparison.generate_latex_table(subjects)
    # print(latex_table_online)

    # Save the results to Excel files for each external column
    # x fra: vedi tu dove salvare
