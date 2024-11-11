import os
import audeer
import audonnx
import audinterface
import numpy as np
import pandas as pd
from pydub import AudioSegment

model_root = 'voice/model'
cache_root = 'voice/cache'
test_root = 'G:/Il mio Drive/Test'
test_root = '/mnt/g/Il mio Drive/Test'
recordings_root = test_root + '/S{0}'
testers = sorted([int(x[1:]) for x in os.listdir(test_root) if x.startswith('S')])

audeer.mkdir(cache_root)


def cache_path(file):
    return os.path.join(cache_root, file)


url = 'https://zenodo.org/record/6221127/files/w2v2-L-robust-12.6bc4a7fd-1.1.0.zip'
dst_path = cache_path('model.zip')
if not os.path.exists(dst_path):
    audeer.download_url(url, dst_path, verbose=True)

if not os.path.exists(model_root):
    audeer.extract_archive(dst_path, model_root, verbose=True)

model = audonnx.load(model_root)

sampling_rate = 16000
interface = audinterface.Feature(
    model.labels('logits'),
    process_func=model,
    process_func_args={
        'outputs': 'logits'
    },
    sampling_rate=sampling_rate,
    resample=True,
    verbose=True
)


def get_recordings_root(tester_id, processed=False):
    folder = 'ProcessedRecordings' if processed else 'Recordings'
    return recordings_root.format(tester_id) + "/" + folder


def analize_audios(tester, trimmed, replace=False, ignore_silenced=True, processed=False):
    audio_root = get_recordings_root(tester)
    output_root = recordings_root.format(tester)
    # check if the file already exists
    if not replace:
        f = os.path.join(output_root, f'voice{"_trimmed" if trimmed else ""}{"_processed" if processed else ""}.csv')
        if os.path.exists(f):
            print(f'File {"(trimmed)" if trimmed else ""} already exists, skipping')
            return

    audio_files = [x for x in os.listdir(audio_root) if x.endswith('.wav') and not 'silenced' in x]
    audio_files.sort()

    if trimmed:
        audio_files = [y for y in audio_files if 'trimmed' in y]
    else:
        audio_files = [y for y in audio_files if 'trimmed' not in y]
    data = {
        'start': [],
        'end': [],
        'arousal': [],
        'valence': [],
        'dominance': []
    }
    audio_files_size = len(audio_files)
    print('Analysing {0} files'.format(audio_files_size))
    silenced = 0
    for i, record in enumerate(audio_files):
        path = os.path.join(audio_root, record)
        tmp = record.split("_")
        start = int(tmp[0])
        end = int(tmp[1].replace('.wav', ''))
        if ignore_silenced and AudioSegment.from_wav(path).dBFS < -40:
            print("Ignoring file {0} ({1}/{2})".format(record, i + 1, audio_files_size))
            silenced += 1
            continue
        else:
            print('Analysing {0} ({1}/{2})'.format(record, i + 1, audio_files_size))
        try:
            res = interface.process_file(path).round(2)
            ar = res.iloc[0]["arousal"]
            va = res.iloc[0]["valence"]
            do = res.iloc[0]["dominance"]
            data['start'].append(start)
            data['end'].append(end)
            data['arousal'].append(ar)
            data['valence'].append(va)
            data['dominance'].append(do)
        except:
            print("Error processing file {0}".format(path))
            data['start'].append(start)
            data['end'].append(end)
            data['arousal'].append(np.nan)
            data['valence'].append(np.nan)
            data['dominance'].append(np.nan)
            continue

    path = os.path.join(output_root, f'voice{"_trimmed" if trimmed else ""}{"_processed" if processed else ""}.csv')
    pd.DataFrame(data).to_csv(path, index=False)
    if ignore_silenced:
        print("Silenced files: {0}".format(silenced))
    print("done!")


def rename_silenced_files(tester):
    audio_root = get_recordings_root(tester)
    audio_files = [x for x in os.listdir(audio_root) if x.endswith('.wav')]
    audio_files.sort()
    for i, record in enumerate(audio_files):
        path = os.path.join(audio_root, record)
        if AudioSegment.from_wav(path).dBFS < -30:
            print("Renaming file {0}".format(record))
            os.rename(path, path.replace('.wav', '_silenced.wav'))


def restore_silenced_files(tester):
    audio_root = get_recordings_root(tester)
    audio_files = [x for x in os.listdir(audio_root) if x.endswith('_silenced.wav')]
    audio_files.sort()
    for i, record in enumerate(audio_files):
        path = os.path.join(audio_root, record)
        print("Restoring file {0}".format(record))
        os.rename(path, path.replace('_silenced.wav', '.wav'))


def analize_all_testers(replace=True, processed=False):
    print("Testers: {0}".format(testers))
    for tester in testers:
        print("Analyzing Tester {0}".format(tester))
        analize_audios(tester, trimmed=False, replace=replace, ignore_silenced=True)
        analize_audios(tester, trimmed=True, replace=replace, ignore_silenced=True)


import numpy as np
import pandas as pd
import matplotlib.pyplot as plt

fps = 50


def plot_data(data_arr, title):
    plt.figure()
    plt.ylim(0, 1)
    plt.xlabel('Time (s)')
    plt.ylabel('Arousal')
    plt.title(title)
    # plt.xticks(np.linspace(0, (max(d.keys()) - min(d.keys())) / 1000, 10))
    plt.plot(list(data_arr.keys()), list(data_arr.values()))


def plot_valence(tester, trimmed=False, processed=False, separateCheckpoints=True):
    voice_path = test_root + f'/S{tester}/voice{"_trimmed" if trimmed else ""}{"_processed" if processed else ""}.csv'
    df = pd.read_csv(voice_path)
    min_time = df['start'].min()
    max_time = df['end'].max()

    print(f"Min time: {min_time}, Max time: {max_time}")
    print(f"Total time: {int((max_time - min_time) / 1000 / 60)}:{int((max_time - min_time) / 1000 % 60)} min")

    data = np.arange(0, max_time - min_time, 1000 / fps)
    d = {key: 0 for key in data}
    # remove last from df
    df = df[:-1]

    for row in df.index:
        start = df['start'][row] - min_time
        end = df['end'][row] - min_time
        ar = df['valence'][row]
        for k, v in d.items():
            if start < k < end:
                if d[k] != 0:
                    print("Overlapping data at {0}".format(k))
                d[k] = ar

    # print("0 values: {0}".format(np.sum(np.array(list(d.values())) == 0)))
    d = {k / 1000: v if v != 0 else np.nan for k, v in d.items()}

    plt.plot(list(d.keys()), list(d.values()), color='red')
    plt.title('Tester {0}'.format(tester))
    plt.xlabel('Time')
    plt.ylim((0, 1))
    plt.ylabel('Valence')
    plt.savefig(f"tester_valence{tester}{'_trimmed' if trimmed else ''}{'_processed' if processed else ''}.png")
    plt.show()


def get_checkpoint_dictionary(tester):
    data_tracker = pd.read_csv(test_root + '/S{0}/S{0}.csv'.format(tester), skiprows=1, sep=';')
    # print(data_tracker["LastCheckpoint"])
    init_time = data_tracker["timestampStartFixedMillisecond"].min()
    timestamps = data_tracker["timestampStartFixedMillisecond"] - init_time
    checkpointsDictionary = {key / 1000: val for key, val in zip(timestamps, data_tracker["LastCheckpoint"]) if
                             not pd.isna(val)}
    unique_checkpoints = dict()
    for timestamp, checkpoint in checkpointsDictionary.items():
        if checkpoint not in unique_checkpoints.values():
            unique_checkpoints[timestamp] = checkpoint

    print(unique_checkpoints)
    return unique_checkpoints


def plot_data_with_dante(tester, trimmed=False, processed=False, separateCheckpoints=True):
    voice_path = test_root + f'/S{tester}/voice{"_trimmed" if trimmed else ""}{"_processed" if processed else ""}.csv'
    df = pd.read_csv(voice_path)
    dante = pd.read_csv(test_root + '/S{0}/stress.csv'.format(tester), sep=';')

    min_time = df['start'].min()
    max_time = df['end'].max()

    print(f"Min time: {min_time}, Max time: {max_time}")
    print(f"Total time: {int((max_time - min_time) / 1000 / 60)}:{int((max_time - min_time) / 1000 % 60)} min")

    data = np.arange(0, max_time - min_time, 1000 / fps)
    d = {key: 0 for key in data}
    # remove last from df
    df = df[:-1]

    for row in df.index:
        start = df['start'][row] - min_time
        end = df['end'][row] - min_time
        ar = df['arousal'][row]
        for k, v in d.items():
            if start < k < end:
                if d[k] != 0:
                    print("Overlapping data at {0}".format(k))
                d[k] = ar

    # print("0 values: {0}".format(np.sum(np.array(list(d.values())) == 0)))
    d = {k / 1000: v if v != 0 else np.nan for k, v in d.items()}
    tot_time = (max_time - min_time) / 1000

    plt.plot(dante['TimeStamp'], (dante['Value'] + 1) / 2)
    plt.plot(list(d.keys()), list(d.values()), color='red')
    plt.title('Tester {0}'.format(tester))
    plt.xlabel('Time')
    plt.ylim((0, 1))
    plt.ylabel('Arousal')
    plt.savefig(f"tester{tester}{'_trimmed' if trimmed else ''}{'_processed' if processed else ''}.png")
    plt.show()

    if (separateCheckpoints):
        # split for each checkpoint
        checkpoints = get_checkpoint_dictionary(tester)
        last_t = 0
        for time, ckpt in checkpoints.items():
            # get data between 0 and time
            dante_part = dante[dante['TimeStamp'] < time]
            dante_part = dante_part[dante_part['TimeStamp'] > last_t]
            data_part = {k: v for k, v in d.items() if last_t < k < time}
            plt.plot(dante_part['TimeStamp'], (dante_part['Value'] + 1) / 2)
            plt.plot(list(data_part.keys()), list(data_part.values()), color='red')
            plt.title('Tester {0} - {1}'.format(tester, ckpt))
            plt.xlabel('Time')
            plt.ylim((0, 1))
            plt.ylabel('Value')
            plt.savefig(f"tester{tester}{'_trimmed' if trimmed else ''}{'_processed' if processed else ''}-{ckpt}.png")
            plt.show()
            last_t = time
            # dante_arousal = (dante_part['Value'] + 1)/2
            # print("Correlation between stress and voice data: {0}\n".format(dante_arousal.corr(pd.Series(list(data_part.values())))))

    dante_arousal = (dante['Value'] + 1) / 2
    print("Correlation between stress and voice data: {0}\n".format(dante_arousal.corr(pd.Series(list(d.values())))))


#for i in [i for i in range(1, 28) if i != 25]:
    #plot_valence(i, processed=False)
    #plot_valence(i, processed=True)
# plot_data_with_dante(101)
# plot_data_with_dante(1, False, False, False)
# plot_data_with_dante(1, True, False, False)
# plot_data_with_dante(1, False, True, False)
# plot_data_with_dante(101, False, False, False)

plot_valence(101)

# for i in [i for i in range(1,28) if i != 25]:
#    plot_data_with_dante(i, False, True, False)
#    plot_data_with_dante(i, False, True, True)
#    plot_data_with_dante(i, False, False, False)
#    plot_data_with_dante(i, False, False, True)


# Correlation between trimmed and untrimmed data
t = 1
df1 = pd.read_csv(test_root + '/S{0}/voice.csv'.format(t))
df2 = pd.read_csv(test_root + '/S{0}/voice_trimmed.csv'.format(t))

print(f"Arousal values correlation for trimmed and untrimmed data for tester {1}")
print("Arousal: {0}".format(df1['arousal'].corr(df2['arousal'])))
print("Valence: {0}".format(df1['valence'].corr(df2['valence'])))
print("Dominance: {0}".format(df1['dominance'].corr(df2['dominance'])))

plt.scatter(df1['arousal'], df2['arousal'])
plt.title(f"Arousal values correlation for trimmed and untrimmed data for tester {1}")
plt.xlabel("Untrimmed")
plt.ylabel("Trimmed")
plt.savefig(f"tester{t}_trimmed_untrimmed.png")
plt.show()
