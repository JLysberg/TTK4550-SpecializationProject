from datetime import timedelta
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

class DataSettings:
    def __init__(self, envType = "dynamic", postLabelled = True, hideBlink = True,
        dynLin = True, dynQuad = True, dynCubic = False):
        self.EnvType = envType
        self.PostLabelled = postLabelled
        self.HideBlink = hideBlink
        if envType == "dynamic":
            self.DynLin = dynLin
            self.DynQuad = dynQuad
            self.DynCubic = dynCubic

class ETDataInterface:
    # Hack to support execution from both vscode and notebook directory
    if __name__ == "__main__":
        _tsDir = Path("Code/ETDataHub/Outputs/ETTimeSeries/")
    else:
        _tsDir = Path("../ETDataHub/Outputs/ETTimeSeries/")

    _keptColumns = ["Timestamp",
                   "Label", 
                   "Left.GazePointOnDisplayNormalized.X", 
                   "Left.GazePointOnDisplayNormalized.Y"]
    _ts = []

    # Chosen feature window sizes where 33Hz give ~30ms per sample.
    _fws = 5     # 150ms
    _fwl = 11    # 330ms

    # Blink window. Samples to be removed surrounding blinks
    _bw = 10

    def __init__(self, settings = DataSettings()):
        self._settings = settings
        
        if settings.EnvType == "dynamic":
            dirID = "DynamicV1-WARP"
            if settings.DynLin:
                dirID += ",LIN"
            if settings.DynQuad:
                dirID += ",QUAD"
            if settings.DynCubic:
                dirID += ",CUBIC"
        elif settings.EnvType == "static":
            dirID = "StaticV1"
        else: 
            dirID = settings.EnvType
            settings.PostLabelled = False

        if settings.PostLabelled:
            dirID += "-Labelled"

        self._tsDir = self._tsDir / Path(f"{dirID}/")

        # Pre process data frames
        self.ImportDataFrames()
        self.CleanDataFrames()
        self.GenerateFeatures()
        self.MergeDataFrames()

        # self._dataset.index = pd.to_datetime(self._dataset.index, format="%Y-%m-%d %H:%M:%S.%f")

    def ImportDataFrames(self):
        self._ts.clear()

        # Get list of all time series paths
        tsPaths = listdir(self._tsDir)

        # Iterate through paths, load dataframes from csv and append to list
        for tsPath in tsPaths:
            df = pd.read_csv(self._tsDir / tsPath)
            self._ts.append(df)
    
    def CleanDataFrames(self):
        # TODO: Split into smaller dedicated routines
        cleanTs = []
        sampleCount = 0
        for i in range(len(self._ts)):
            # Filter out unnecessary data and rename coordinate columns
            self._ts[i] = self._ts[i].filter(self._keptColumns).rename(columns={
                "Left.GazePointOnDisplayNormalized.X": "x",
                "Left.GazePointOnDisplayNormalized.Y": "y"})
            
            # Convert timestamps to datetime format
            self._ts[i]["Timestamp"] = pd.to_datetime(
                self._ts[i]["Timestamp"], format="%Y-%m-%d %H:%M:%S.%f")
            
            # Set pre-labelled BLINK to UNDEFINED (If old timeseries are used) and remove
            self._ts[i].loc[self._ts[i].Label == "BLINK", 'Label'] = "UNDEFINED"
            self._ts[i] = self._ts[i][self._ts[i].Label != "UNDEFINED"]

            self._ts[i]["sampleIndex"] = [sampleCount + k for k in range(len(self._ts[i].index))]
            sampleCount += len(self._ts[i].index)

            # Add column with timeseries delta
            self._ts[i]["delta"] = self._ts[i]["Timestamp"].diff()

            # Infer blink labels to samples with missing data
            self._ts[i].loc[self._ts[i].delta > timedelta(microseconds=45000), 'Label'] = "BLINK"

            # Get indices of missing data samples
            mdIndices = self._ts[i]["sampleIndex"][self._ts[i]["Label"] == "BLINK"].tolist()

            # Create new extended array of missing data indices, including blink window
            extmdIndices = []
            for mdIndex in mdIndices:
                for k in range(mdIndex, min(mdIndex + self._bw, sampleCount + len(self._ts[i].index))):
                    if (k not in extmdIndices):
                        extmdIndices.append(k)
                mdIndex -= sampleCount - len(self._ts[i].index)
                self._ts[i].loc[mdIndex:min(mdIndex + self._bw - 1, len(self._ts[i].index))]["Label"] = "BLINK"

            # Skip rest of loop if blink labels are to be included
            if not self._settings.HideBlink:
                cleanTs.append(self._ts[i].copy())
                continue

            # Slice dataframe into purely blinkless fragments and append to new list of clean time series
            if len(extmdIndices) == 0:
                cleanTs.append(self._ts[i].copy())
            else:
                prevBIndex = -1
                for mdIndex in extmdIndices:
                    mdIndex -= sampleCount - len(self._ts[i].index)
                    if self._ts[i].iloc[prevBIndex + 1:mdIndex,:].empty:
                        pass
                    else:
                        cleanTs.append(self._ts[i].iloc[prevBIndex + 1:mdIndex,:].copy())
                    prevBIndex = mdIndex
                if prevBIndex != self._ts[i].last_valid_index():
                    cleanTs.append(self._ts[i].iloc[prevBIndex + 1:self._ts[i].last_valid_index(),:].copy())
                for ts in cleanTs:
                    self._ts.append(ts)

        self._ts = cleanTs.copy()

    def MergeDataFrames(self):
        self._dataset = pd.concat(self._ts, ignore_index=False, sort=False)
    
    def GenerateFeatures(self):
        for i in range(len(self._ts)):
            # Apply feature generators with single column operations
            for column in self._ts[i].columns.drop(["Timestamp", "Label", "delta", "sampleIndex"]):
                self._ts[i][column + "_rms"] = \
                    self._ts[i][column].rolling(self._fws, center=True).apply(rms, raw=True)
                self._ts[i][column + "_std"] = \
                    self._ts[i][column].rolling(self._fws, center=True).std()
                self._ts[i][column + "_med-diff"] = \
                    self._ts[i][column].rolling(self._fwl, center=True).apply(med_diff, raw=True)
                self._ts[i][column + "_mean-diff"] = \
                    self._ts[i][column].rolling(self._fwl, center=True).apply(mean_diff, raw=True)
                self._ts[i][column + "_rms-diff"] = \
                    self._ts[i][column].rolling(self._fwl, center=True).apply(rms_diff, raw=True)
                self._ts[i][column + "_std-diff"] = \
                    self._ts[i][column].rolling(self._fwl, center=True).apply(std_diff, raw=True)
                    
            # Apply feature generators with multiple column operations
            sLen = len(self._ts[i])
            self._ts[i]["disp"] = pd.Series(
                {
                    self._ts[i].index[k]: 
                    disp(self._ts[i].iloc[k - int(self._fws/2) : k + int(self._fws/2) + 1]) 
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

            # Remove NaN rows
            self._ts[i] = self._ts[i].dropna()

    def GetDataset(self):
        return self._dataset

if __name__ == "__main__":
    interface = ETDataInterface(DataSettings(hideBlink=True, envType="DataQualityTest"))
    data = interface.GetDataset().filter(["Timestamp", "Label", "x", "y", "delta"])


    
    