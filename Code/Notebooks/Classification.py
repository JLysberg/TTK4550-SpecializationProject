import ETDataInterface as et
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd

from sklearn.model_selection import train_test_split
from sklearn.metrics import roc_curve, auc, roc_auc_score
import sklearn.metrics as sklm
from sklearn.ensemble import RandomForestClassifier
from sklearn.ensemble import GradientBoostingClassifier


class MLModel():
    
    def __init__(self, data):
        self._trained = False
        self._data = data

        self._encodeDataset()
        self._splitDataset()

    def _encodeDataset(self):
        self._data["Label"] = self._data["Label"].replace({"FIXATION": 0, "SACCADE": 1, "SMOOTH": 2})
        self._data = self._data.drop(["Timestamp", "delta", "sampleIndex", "x", "y"], axis=1)

        self._binary = (self._data["Label"].unique().size < 3)

    def _splitDataset(self):
        # Split dataset into train and validation splits
        self._train, self._test, self._y_train, self._y_test = train_test_split(self._data.drop(["Label"], axis="columns"),
            self._data["Label"], test_size=0.25, shuffle=False)
    
    def TrainRFC(self):
        self._clf = RandomForestClassifier(
            n_jobs=-1,
            class_weight="balanced",
            criterion="entropy"
            )
        self._clf.fit(self._train, self._y_train)

        self._trained = True
    
    def GetClassifications(self):
        assert self._trained, "Train model first"

        classifications = self._clf.predict_proba(self._test)
        classification_results = pd.DataFrame({
            "original_index": self._y_test.index,
            "y": self._y_test.reset_index(drop=True).replace({0:"FIXATION", 1:"SACCADE", 2:"SMOOTH"}),
            "y_class": pd.DataFrame(classifications).idxmax(axis=1).replace({0:"FIXATION", 1:"SACCADE", 2:"SMOOTH"}),
            "certainty": pd.DataFrame(classifications).max(axis=1)
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
        pred_consensus = pd.DataFrame(pred_proba).idxmax(axis=1)
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
        fig.suptitle('Confusion matrices', fontsize=16)
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
        ax3.set_title("Pred-normalized")
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
        plt.title('Feature Importances')
        plt.barh(range(len(indices)), importances[indices], color='b', align='center')
        plt.yticks(range(len(indices)), [clfFeatures[i] for i in indices])
        plt.xlabel('Relative Importance')
        plt.show()