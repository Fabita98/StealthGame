from __future__ import annotations
import os
import matplotlib.pyplot as plt
import numpy as np
from scipy import signal
import math
from sklearn.preprocessing import MinMaxScaler
import pandas as pd
from enum import Enum
import tsfresh
from tsfresh.feature_extraction import EfficientFCParameters
from tslearn.preprocessing import TimeSeriesScalerMeanVariance
from tslearn.piecewise import PiecewiseAggregateApproximation, SymbolicAggregateApproximation, \
    OneD_SymbolicAggregateApproximation

drivePath = 'G:/.shortcut-targets-by-id/1wNGSxajmNG6X6ORVLNxGRZkrJJVw02FA/Test'
AlienPath = 'F:/Data_Analysis'


def process_directoryDrive(path=drivePath):
    drive = sorted(list(filter(lambda x: x[0] == 'S', os.listdir(path))), key=lambda x: int(x[1:]))
    return drive


if __name__ == "__main__":
    files = process_directoryDrive(path=drivePath)
    print(files)
