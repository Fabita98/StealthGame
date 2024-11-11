import os

import audeer
import audinterface
import audonnx
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd

audio_root = 'datasets/Corpus_Globalv1/'
model_root = 'voice/model'
cache_root = 'voice/cache'

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


def get_audio_path(root, audio_id):
    return os.path.join(root, f"Audio{audio_id}")


def analize():
    evaluations = {}
    with open(audio_root + "evaluations.csv", "w") as f:
        for i in range(1, 15):
            print(get_audio_path(audio_root, i))
            tmp = [x for x in os.listdir(get_audio_path(audio_root, i)) if ".wav" in x]
            for t in tmp:
                path = os.path.join(get_audio_path(audio_root, i), t)
                print(f"Processing {path}")
                evals = t.replace(".wav", "").split("-")[1:]
                ear = int(evals[1]) / 5
                eva = int(evals[0]) / 5
                edo = int(evals[2]) / 5
                res = interface.process_file(path).round(2)
                ar = res.iloc[0]["arousal"]
                va = res.iloc[0]["valence"]
                do = res.iloc[0]["dominance"]
                v = (evals[0], evals[1], evals[2], ar, va, do)
                evaluations[t] = v
                f.write(f"{t},{ear},{eva},{edo},{ar},{va},{do}\n")
                f.flush()
    print("finished")


def print_data():
    # Read the data from csv
    data = pd.read_csv(audio_root + "evaluations.csv", header=None)
    #print(data)
    #print(f"Correlation between Arousal and Dominance in evals {data[2].corr(data[3])}")
    #plt.scatter(data[1], data[3])
    #plt.show()
    print(f"Correlation between Arousal and Dominance in results {data[4].corr(data[6])}")
    plt.scatter(data[4], data[6])
    plt.savefig("arousal_dominance_correlation.png")
    plt.show()

    #print(f"Correlation between Arousal in evals and results {data[1].corr(data[4])}")
    #plt.scatter(data[1], data[4])
    #plt.show()
    #print(f"Correlation between Valence in evals and results {data[2].corr(data[5])}")
    #plt.scatter(data[2], data[5])
    #plt.show()

    a = np.where(abs(data[1] - data[4]) < 0.15)
    v = np.where(abs(data[2] - data[5]) < 0.15)
    print(f"Percentage of Arousal correct {len(a[0]) / len(data)}")
    print(f"Percentage of Valence correct {len(v[0]) / len(data)}")

print_data()

