# 
    DataAnalysis

## 
    Analisi dei dati e modelli di Simone rivisitata

### Folders structure before running the python codes

**MainFolder set as the "*drivePath"*  in the python codes.**

MainFolder
    ->S0
        —>S0.csv (game data)
        —>stress.csv (Dante data)
    ->S1
        —>S1.csv (game data)
        —>stress.csv (Dante data)
    ->S2
        —>S2.csv (game data)
        —>stress.csv (Dante data)
    and so on...

#### Preprocessing folder:

• **DivideDataframe.py:** division of different data types;
• **ProcessSingleDaframe.py:** preprocessing of different data types, on each single type;
• **ProcessDataframes.py:** preprocessing of data (calls ProcessSingleDaframe.py);
• **DanteAnnotation.py:** preprocessing of DANTE (stress annotations) and supersampling;
• **DataSynchronization.py:** synchronization of DANTE and OCULUS data, resampling;
• **FeatureExtraction.py:** feature extraction with *tsfresh*;
• **FeatureSelection.py:** feature selection (on single subjects and on all subjects);
• **VisualizeData.py:** raw data visualization;
• **dataframeManager.py:** not used;

#### Models folder:

• **CompareOnlineKFs.py:** kalman filter and kalman filter with input;
• **DKF.py:** discriminative kalman filter;
• **LSTM+KF.py:** long short term memory network with Kalman Filter (not used);

##### How to run Dante:

1. Videos need to be located at C:/wamp64/www/video/StealthGame;
2. Run Wampserver64;
3. Open URL: localhost/login
   3.1. username: admin
   3.2. password: strexspace
   3.3. Then, you choose the name, surname, email (also fake) and the folder you want to get the videos;
4. Go to a webpage where all the saved videos are there and you choose the one you want to use Dante with it;
5. Start the video and change the values;
6. At the end of the video, the Dante values will be saved in C:/wamp64/www/annotations;
7. We can use the python code for preprocessing the data we have and combination of our data with dante .csv;
8. We use CompareOnlineKFs.py.
