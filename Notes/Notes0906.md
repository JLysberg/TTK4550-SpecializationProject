# 6. 9. - 9. 9.
## Thoughts
### What data is of interest?
- Maybe some data is more interesting for differing metrics. I reckon gaze is very important for precision analysis of performance. Head pose and distance to monitor may have a greater impact on fatigue
- Playstyle? Maybe some playstyles impose a greater fatigue than others.
- Pupil diameter difference during game.
- Difference between left eye and right eye gaze? -> Focus? 

### Classification of AOI fixation
- How would data sets be labelled? 
    - Manual labelling during game would be too prone to variance. Bad datasets make bad decision trees.
    - Automatic labeling using simple algorithms is of course useless as it would simply train the decision tree to perform as good or worse than the classification algorithms with poor performance.
- If AOI in ET data are defined by constant constraints (at least given monitor display ratio), very simple algorithms (if/else) are likely optimal.

### Classification of eye event (fixation/saccade/PSO/smooth pursuit/blink)
- Use as an addition to simple AOI determination?
    - E.g. Fixation within minimap, fixation within health bar, saccade between weapons, smooth pursuit in game area -> opponent spotted?
- How to label this data set?
    - Manual labelling of binary classification problem (e.g. fixation and saccades) during game not impossible but very difficult. Could however also be labelled in any non-game setting, which would be much easier.
    - Create labelling solution where data can be replayed. Imperative if multiple classifications are to be determined.
    - Maybe pre trained networks exist? -> Should still be trained on self made data set in order to adapt to hardware and setup.

## Reading
### Zemblys 2017 - Using machine learning to detect events in eye-tracking data
- Further research: Older algorithmic solution to event detection in eye tracking data.
- How are samples defined exactly? Not fully explained in paper.
    - Possible simply one (x,y) screen coordinate. Features are then extracted using previous and/or following samples to create a feature vector for entire recording (?).
- "Random forest resulted to the best eye-movement event clasification performance" why? Read Zemblys 2016.
- Decision tree bootstrap aggregation -> Process of pulling randomly selected samples from set of samples to several new sets, with potential duplicates. Final decision tree is an averaged version of the decision trees from each separate new set.
    - Random Forest algorithms do not overfit (Breiman, 2001). This is likely due to the act of bootstrap aggregation.
    - Underfitting often slightly increased, but the reduction in overfitting usually compensates for this problem due to averaging (Breiman, 2001).
- "Decision tree quality" -> Gini impurity, similar to information gain
- _Balanced subsample weighting_.