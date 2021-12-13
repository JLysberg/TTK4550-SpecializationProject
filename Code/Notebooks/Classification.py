from pandas._libs.tslibs.timedeltas import Timedelta
import ETDataInterface as et
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd

from sklearn.model_selection import train_test_split
from sklearn.metrics import roc_curve, auc, roc_auc_score
import sklearn.metrics as sklm
from sklearn.ensemble import RandomForestClassifier
from sklearn.ensemble import GradientBoostingClassifier

TEST_SIZE = 0.20

class MLModel():
    def __init__(self, data, clfType):
        self._trained = False
        self._data = data
        if (clfType == "RFC"):
            self._clf = RandomForestClassifier(
                n_jobs=-1,
                class_weight="balanced",
                criterion="entropy"
                )
        else: self._clf = None

        self._encodeDataset()
        self._splitDataset()

    def _encodeDataset(self):
        self._data["Label"] = self._data["Label"].replace({"FIXATION": 0, "SACCADE": 1, "SMOOTH": 2})
        # self._data = self._data.drop(["Timestamp", "delta", "sampleIndex", "x", "y"], axis=1)
        self._data = self._data.drop(["Timestamp", "delta", "x", "y"], axis=1)

        self._binary = (self._data["Label"].unique().size < 3)

    def _splitDataset(self):
        self._train, self._test, self._y_train, self._y_test = train_test_split(self._data.drop(["Label"], axis="columns"),
            self._data["Label"], test_size=TEST_SIZE, shuffle=True)
    
    def _postProcess(self, pred_prob, decode):
        y_class =  pd.DataFrame(pred_prob).idxmax(axis=1)
        if decode: y_class = y_class.replace({0:"FIXATION", 1:"SACCADE", 2:"SMOOTH"})

        # for i in range(len(y_class)-1):
        #     if i < len(y_class)-1:
        #         if y_class[i] == "SACCADE" and y_class[i+1] == "SMOOTH":
        #             y_class[i+1] = "SACCADE"
        #     if i > 0 and i < len(y_class)-1:
        #         if y_class[i-1] != y_class[i] and y_class[i] != y_class[i+1]:
        #             y_class[i] = y_class[i-1]
        return y_class
    
    def Train(self):
        self._clf.fit(self._train, self._y_train)

        self._trained = True
    
    def GetClassifications(self, data):
        assert self._trained, "Train model first"

        data["Label"] = data["Label"].replace({"FIXATION": 0, "SACCADE": 1, "SMOOTH": 2})
        data = data.drop(["Label", "Timestamp", "delta", "x", "y"], axis=1)

        pred_proba = self._clf.predict_proba(data)
        classifications = self._postProcess(self._clf.predict_proba(data), True)
        # y_class =  pd.DataFrame(classifications).idxmax(axis=1).replace({0:"FIXATION", 1:"SACCADE", 2:"SMOOTH"})

        # for i in range(len(y_class)-1):
        #     if i < len(y_class)-1:
        #         if y_class[i] == "SACCADE" and y_class[i+1] == "SMOOTH":
        #             y_class[i+1] = "SACCADE"
        #     if i > 0 and i < len(y_class)-1:
        #         if y_class[i-1] != y_class[i] and y_class[i] != y_class[i+1]:
        #             y_class[i] = y_class[i-1]

        return pd.DataFrame({
            "o_index": self._test["sampleIndex"].reset_index(drop=True),
            "y": self._y_test.reset_index(drop=True).replace({0:"FIXATION", 1:"SACCADE", 2:"SMOOTH"}),
            "y_class": classifications,
            "certainty": pd.DataFrame(self._clf.predict_proba(data)).max(axis=1)
        })
        
    def PlotBinaryModelAUC(self):
        assert self._trained, "Train model first"
        assert self._binary, "ROC only available for binary classification"

        # Compute ROC curve and ROC area
        fpr, tpr, _ = roc_curve(self._y_test, self._clf.predict_proba(self._test)[:, 1])
        roc_auc = auc(fpr, tpr)

        # Plot
        plt.figure()
        lw = 2
        plt.plot(fpr, tpr, color='darkorange',
                lw=lw, label='ROC curve (area = %0.2f)' % roc_auc)
        plt.plot([0, 1], [0, 1], color='navy', lw=lw, linestyle='--')
        plt.xlim([0.0, 1.0])
        plt.ylim([0.0, 1.05])
        plt.xlabel('False Positive Rate')
        plt.ylabel('True Positive Rate')
        plt.title("Classifier performance measure")
        plt.legend(loc="lower right")
        plt.show()    

    def ShowMultiModelPerformance(self):
        assert self._trained, "Train model first"

        pred_proba = self._clf.predict_proba(self._test)
        pred_consensus = self._postProcess(pred_proba, False)
        rocAuc = sklm.roc_auc_score(self._y_test, pred_proba, multi_class='ovo', average="weighted")
        balancedAcc = sklm.balanced_accuracy_score(self._y_test, pred_consensus)
        cohenKappa = sklm.cohen_kappa_score(self._y_test, pred_consensus)
        precision = sklm.precision_score(self._y_test, pred_consensus, average=None)
        recall = sklm.recall_score(self._y_test, pred_consensus, average=None)
        report = sklm.classification_report(self._y_test, pred_consensus, target_names=["FIXATION", "SACCADE", "SMOOTH"])

        print(f"Model Performance metrics:\n \
            \tROC AUC: \t\t{round(rocAuc, 2)}\t\t\t\t[0.0,1.0]\n \
            \tBalanced accuracy: \t{round(balancedAcc, 2)}\t\t\t\t[0.0,1.0]\n \
            \tCohen Kappa: \t\t{round(cohenKappa, 2)}\t\t\t\t[-1.0,1.0]\n \
            \tPrecision score: \tFi: {round(precision[0], 2)}, Sa: {round(precision[1], 2)}, Sm: {round(precision[2], 2)}\t[-1.0,1.0]\n \
            \tRecall score: \t\tFi: {round(recall[0], 2)}, Sa: {round(recall[1], 2)}, Sm: {round(recall[2], 2)}\t[-1.0,1.0]\n \
            ")

        print(report)

        confusionMatrix = sklm.confusion_matrix(self._y_test, pred_consensus)
        confusionMatrixNorm1 = sklm.confusion_matrix(self._y_test, pred_consensus, normalize="true")
        confusionMatrixNorm2 = sklm.confusion_matrix(self._y_test, pred_consensus, normalize="pred")

        labels = ["FIX", "SACC", "SMOOTH"]
        fig, (ax1, ax2, ax3) = plt.subplots(1, 3, sharey='row')
        # fig.suptitle('Confusion matrices', fontsize=16)
        ax1.imshow(confusionMatrix)
        ax1.set_title("Unnormalized")
        ax1.set_ylabel("True labels")
        ax1.set_xlabel("Classified labels")
        ax1.set_xticks(np.arange(len(labels)))
        ax1.set_yticks(np.arange(len(labels)))
        ax1.set_yticklabels(labels)
        ax1.set_xticklabels(labels)
        plt.setp(ax1.get_xticklabels(), rotation=45, ha="right",
            rotation_mode="anchor")
        for (i, j), z in np.ndenumerate(confusionMatrix):
            ax1.text(j, i, z, ha='center', va='center')

        ax2.imshow(confusionMatrixNorm1)
        ax2.set_title("True-normalized")
        ax2.set_xlabel("Classified labels")
        ax2.set_xticks(np.arange(len(labels)))
        ax2.set_xticklabels(labels)
        plt.setp(ax2.get_xticklabels(), rotation=45, ha="right",
            rotation_mode="anchor")
        for (i, j), z in np.ndenumerate(confusionMatrixNorm1):
            ax2.text(j, i, '{:0.2f}'.format(z), ha='center', va='center')
            
        ax3.imshow(confusionMatrixNorm2)
        ax3.set_title("Clf.-normalized")
        ax3.set_xlabel("Classified labels")
        ax3.set_xticks(np.arange(len(labels)))
        ax3.set_xticklabels(labels)
        plt.setp(ax3.get_xticklabels(), rotation=45, ha="right",
            rotation_mode="anchor")
        for (i, j), z in np.ndenumerate(confusionMatrixNorm2):
            ax3.text(j, i, '{:0.2f}'.format(z), ha='center', va='center')

    def PlotFeatureImportances(self):
        assert self._trained, "Train model first"

        clfFeatures = self._clf.feature_names_in_
        importances = self._clf.feature_importances_
        indices = np.argsort(importances)
        # plt.title('Feature Importances')
        plt.barh(range(len(indices)), importances[indices], color='b', align='center')
        plt.yticks(range(len(indices)), [clfFeatures[i] for i in indices])
        plt.xlabel('Relative Importance')
        plt.show()

class IDFModel():
    def __init__(self, data, dispThreshold, durThreshold):
        self._data = data
        self._dispThreshold = dispThreshold
        self._durThreshold = durThreshold

        self._classify()
        self._splitDataset()

    def _splitDataset(self):
        self._train, self._test, self._y_train, self._y_test = train_test_split(self._data.drop(["Label"], axis="columns"),
            self._data["Label"], test_size=TEST_SIZE, shuffle=False)

    def _classify(self):
        fixationWindows = []

        cumDelta = Timedelta(0)
        windowStart = 0
        i = 0
        self._data = self._data.reset_index(drop=True)
        while i < len(self._data):
            if cumDelta < self._durThreshold: 
                if i in self._data.index:
                    if self._data["delta"][i] is not pd.NaT:
                        cumDelta += self._data["delta"][i]
                i += 1
                continue

            if self._disp(windowStart, i) < self._dispThreshold:
                while self._disp(windowStart, i) < self._dispThreshold and i < len(self._data):
                    i += 1
                fixationWindows.append((windowStart, i))
                windowStart = i
                cumDelta = Timedelta(0)
            else:
                i += 1
                windowStart += 1
                pass
        
        self._data["y_class"] = ["UNDEFINED" for i in range(len(self._data))]
        for wnd in fixationWindows:
            self._data[wnd[0]:wnd[1]]["y_class"] = "FIXATION"
    
    def _disp(self, startIndex, endIndex):
        df = self._data[startIndex:endIndex].copy()
        return ((max(df["x"]) - min(df["x"])) + (max(df["y"]) - min(df["y"])))

    def GetClassifications(self):
        return pd.DataFrame({
            "o_index": self._data["sampleIndex"].reset_index(drop=True),
            "y": self._data["Label"].reset_index(drop=True).replace({0:"FIXATION", 1:"SACCADE", 2:"SMOOTH"}),
            "y_class": self._data["y_class"].reset_index(drop=True)
        })

class IVTModel():
    def __init__(self, data, velThreshold):
        self._data = data
        self._velThreshold = velThreshold

        self._classify()
    #     self._splitDataset()

    # def _splitDataset(self):
    #     self._train, self._test, self._y_train, self._y_test = train_test_split(self._data.drop(["Label"], axis="columns"),
    #         self._data["Label"], test_size=TEST_SIZE, shuffle=False)

    def _classify(self):
        self._data["y_class"] = ["UNDEFINED" for i in range(len(self._data))]
        self._data.loc[self._data.vel > self._velThreshold, 'y_class'] = "SACCADE"

    def GetClassifications(self):
        return pd.DataFrame({
            "o_index": self._data["sampleIndex"].reset_index(drop=True),
            "y": self._data["Label"].reset_index(drop=True).replace({0:"FIXATION", 1:"SACCADE", 2:"SMOOTH"}),
            "y_class": self._data["y_class"].reset_index(drop=True)
        })

if __name__ == "__main__":
    import ETDataInterface as et
    # interface = et.ETDataInterface(et.DataSettings(
    #     envType="static",
    #     generateFeatures=False,
    #     hideBlink=False
    # ))
    # data = interface.GetDataset()
    # IVTmodel = IVTModel(data, 0.003, Timedelta(120, 'milliseconds'))
    # print(IVTmodel.GetClassifications())

    TrainDataInterface = et.ETDataInterface()
    TrainData = TrainDataInterface.GetDataset()
    DataInterface = et.ETDataInterface(et.DataSettings(
        # removeNaNFeatures=False,
        # envType="static",
        # hideBlink=False
        dynLin=False,
        dynQuad=False
    ))
    Data = DataInterface.GetDataset()

    # RFCModel = MLModel(TrainData, "RFC")
    # RFCModel.Train()
    # RFCClassifications = RFCModel.GetClassifications(Data)

    idf = IDFModel(Data.copy(), 0.003, Timedelta(120, 'milliseconds'))
    # ivt = IVTModel(Data.copy(), 0.1)
    print()