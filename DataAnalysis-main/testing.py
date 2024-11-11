import pandas as pd
from sklearn.cluster import KMeans
from maincode import FolderManager, LoadType, DataType
from copy import deepcopy


fManager = FolderManager(21)
fManager.LoadData(LoadType.RAWPHASES, False, True)
a = deepcopy(fManager)
print("AAA")
fManager.RemoveDatatype(DataType.TIME)
print("BBB")
fManager.RemoveDatatype(DataType.TIME)
print("CCC")
a.RemoveDatatype(DataType.TIME)
print("DDD")
a.RemoveDatatype(DataType.TIME)
print("EEE")
