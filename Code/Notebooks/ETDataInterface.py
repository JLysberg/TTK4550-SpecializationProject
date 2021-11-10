import pandas as pd
import numpy as np
from enum import Enum
from pathlib import Path
from os import listdir, remove

def midPartition(series):
    return series[:int(len(series)/2)], series[int(len(series)/2)+1:]

# Single series feature generators
def rms(series):
    return np.sqrt(np.mean(series**2))

def med_diff(series):
    seriesA, seriesB = midPartition(series)
    return abs(np.median(seriesA) - np.median(seriesB))

def mean_diff(series):
    seriesA, seriesB = midPartition(series)
    return abs(np.mean(seriesA) - np.mean(seriesB))

def rms_diff(series):
    seriesA, seriesB = midPartition(series)
    return abs(rms(seriesA) - rms(seriesB))

def std_diff(series):
    seriesA, seriesB = midPartition(series)
    return abs(np.std(seriesA) - np.std(seriesB))

# Multiple colums feature generators
def disp(df):
    return ((max(df["x"]) - min(df["x"])) + (max(df["y"]) - min(df["y"])))

def bcea(df):
    return 0

def bcea_diff(df):
    return 0

def i2mc(df):
    # Await implementation until testing is done on low frequency dataset
    pass

def rayleightest(df):
    # Await implementation until testing is done on low frequency dataset
    pass

class LEnv(Enum):
    STATICPRE = 1    # Binary. Blink == NaN, bad data == UNDEFINED. Only pre-labelled
    STATICPOST = 2    # Binary. Blink == NaN, bad data == UNDEFINED. Post-labelled
    DYNAMIC = 3     # Multi-class. Blink == NaN, bad data == UNDEFINED

class ETDataInterface:
    # Hack to support execution from both vscode and notebook directory
    if __name__ == "__main__":
        _timeSeriesDirectory = Path("Code/ETDataHub/Outputs/ETTimeSeries/")
    else:
        _timeSeriesDirectory = Path("../ETDataHub/Outputs/ETTimeSeries/")

    _keptColumns = ["Timestamp",
                   "Label", 
                   "Left.GazePointOnDisplayNormalized.X", 
                   "Left.GazePointOnDisplayNormalized.Y"]
    # _timeSeriesPaths = []
    _timeSeries = []
    # _dataset = pd.DataFrame()

    # Chosen feature window sizes where 33Hz give ~30ms per sample.
    _fws = 5     # 150ms
    _fwl = 11    # 330ms

    def __init__(self, labellingEnvironment = LEnv.STATICPOST, includeBlink = False):
        self._labellingEnvironment = labellingEnvironment
        self._includeBlink = includeBlink

        if labellingEnvironment == LEnv.STATICPRE:
            self._timeSeriesDirectory = self._timeSeriesDirectory / Path("LabellingV1/")
        elif labellingEnvironment == LEnv.STATICPOST:
            self._timeSeriesDirectory = self._timeSeriesDirectory / Path("LabellingV1.1/")

        # Pre process data frames
        self.ImportDataFrames()
        self.CleanDataFrames()
        self.GenerateFeatures()
        self.MergeDataFrames()

    def ImportDataFrames(self):
        self._timeSeries.clear()

        # Get list of all time series paths
        timeSeriesPaths = listdir(self._timeSeriesDirectory)

        # Iterate through paths, load dataframes from csv and append to list
        for timeSeriesPath in timeSeriesPaths:
            df = pd.read_csv(self._timeSeriesDirectory / timeSeriesPath)
            self._timeSeries.append(df)
    
    def CleanDataFrames(self):
        # TODO: Split into smaller dedicated routines
        cleanTimeseries = []
        for i in range(len(self._timeSeries)):
            # Filter out unnecessary data and rename coordinate columns
            self._timeSeries[i] = self._timeSeries[i].filter(self._keptColumns).rename(columns={
                "Left.GazePointOnDisplayNormalized.X": "x",
                "Left.GazePointOnDisplayNormalized.Y": "y"})
            # Convert timestamps to datetime format
            self._timeSeries[i]["Timestamp"] = pd.to_datetime(
                self._timeSeries[i]["Timestamp"], format="%Y-%m-%d %H:%M:%S.%f")
            
            # TODO: Set labelled labelled BLINK to UNDEFINED

            # TODO: Add row with timeseries delta

            # TODO: Infer blink labels from timeseries delta

            # Skip rest of loop if blink labels are to be included
            if self._includeBlink:
                cleanTimeseries.append(self._timeSeries[i].copy())
                continue

            # Slice dataframe into purely blinkless fragments and append to new list of clean time series
            blinkIndices = self._timeSeries[i].index[self._timeSeries[i]["Label"] == "BLINK"].tolist()
            if len(blinkIndices) == 0:
                cleanTimeseries.append(self._timeSeries[i].copy())
            else:
                prevBIndex = -1
                for bIndex in blinkIndices:
                    if self._timeSeries[i].iloc[prevBIndex + 1:bIndex,:].empty:
                        pass
                    else:
                        cleanTimeseries.append(self._timeSeries[i].iloc[prevBIndex + 1:bIndex,:].copy())
                    prevBIndex = bIndex
                for ts in cleanTimeseries:
                    self._timeSeries.append(ts)

        self._timeSeries = cleanTimeseries.copy()

    def MergeDataFrames(self):
        self._dataset = pd.concat(self._timeSeries, ignore_index=True, sort=False)
    
    def GenerateFeatures(self):
        for i in range(len(self._timeSeries)):
            # Apply feature generators with single column operations
            for column in self._timeSeries[i].columns.drop(["Timestamp", "Label"]):
                self._timeSeries[i][column + "_rms"] = \
                    self._timeSeries[i][column].rolling(self._fws, center=True).apply(rms, raw=True)
                self._timeSeries[i][column + "_std"] = \
                    self._timeSeries[i][column].rolling(self._fws, center=True).std()
                self._timeSeries[i][column + "_med-diff"] = \
                    self._timeSeries[i][column].rolling(self._fwl, center=True).apply(med_diff, raw=True)
                self._timeSeries[i][column + "_mean-diff"] = \
                    self._timeSeries[i][column].rolling(self._fwl, center=True).apply(mean_diff, raw=True)
                self._timeSeries[i][column + "_rms-diff"] = \
                    self._timeSeries[i][column].rolling(self._fwl, center=True).apply(rms_diff, raw=True)
                self._timeSeries[i][column + "_std-diff"] = \
                    self._timeSeries[i][column].rolling(self._fwl, center=True).apply(std_diff, raw=True)
                    
            # Apply feature generators with multiple column operations
            sLen = len(self._timeSeries[i])
            self._timeSeries[i]["disp"] = pd.Series(
                {
                    self._timeSeries[i].index[k]: 
                    disp(self._timeSeries[i].iloc[k - int(self._fws/2) : k + int(self._fws/2) + 1]) 
                    for k in range(int(self._fws/2), sLen - int(self._fws/2))
                }
            )
            # self._timeSeries[i]["bcea"] = pd.Series(
            #     {
            #         self._timeSeries[i].index[k]: 
            #         disp(self._timeSeries[i].iloc[k - int(self._fws/2) : k + int(self._fws/2) + 1]) 
            #         for k in range(int(self._fws/2), sLen - int(self._fws/2))
            #     }
            # )
            # self._timeSeries[i]["bcea-diff"] = pd.Series(
            #     {
            #         self._timeSeries[i].index[k]: 
            #         disp(self._timeSeries[i].iloc[k - int(self._fwl/2) : k + int(self._fwl/2) + 1]) 
            #         for k in range(int(self._fwl/2), sLen - int(self._fwl/2))
            #     }
            # )

            # Remove NaN or UNDEFINED rows
            self._timeSeries[i] = self._timeSeries[i].dropna()[self._timeSeries[i].Label != "UNDEFINED"]

    def GetDataset(self, labels=None):
        if labels==None: return self._dataset
        else:
            return self._dataset[self._dataset["Label"].isin(labels)]

if __name__ == "__main__":
    interface = ETDataInterface()
    # interface = ETDataInterface(False)
    data = interface.GetDataset()
    print(data[data["Label"]=="BLINK"])


    
    