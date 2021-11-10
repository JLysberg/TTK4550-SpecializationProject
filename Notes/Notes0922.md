# 22. - 23. Sep
## Thoughts
- I may need to look into how Tobii interprets raw data into gaze position so I can discuss this operation as the data pre processing it is.
- Write a backround section describing the oculomotor events of the human eye.

## Reading
### Yin et al. (2021), Classification of Eye Tracking Data in Visual Information Processing Tasks Using Convolutional Neural Networks and Feature Engineering
- Optimal feature engineering for CNNs with eye tracking data is currently unknown. Investigation is an objective of this paper.
- To feed input images to a CNN, this paper converted gaze data to images, which in turn were used for feature engineering. 

### Andersson et al. (2016), One algorithm to rule them all? An evaluation and discussion of ten eye movement event-detection algorithms
- My model needs to handle dynamic stimuli relatively well. This is something current detectors generally fail to do
- Classification of eye movement events is driven by the desire to isolate different intervals of the data stream strongly correlated with certian oculomotor or cognitive properties.
    - Visual intake of the eye is severely limited during a saccade.
    - Number of saccades/fixations may be a good set of features for future more complex classification.
- Algorithm evaluation. Difficult task in and of itself. Consider for my project to create a Unity scene which records gaze data as a response to known stimuli.
    - Limitations: Human behaviour. Reaction time?
- Binocular data allows for sample noise reduction by comparing eye-to-eye samples. BIT algorithm.

### Hein, Zangemeister (2017), Topology for gaze analyses - Raw data segmentation
- Introduces a new method of classifying raw eye-tracking data. Identification by topological characteristics (ITop).
- Contains an introduction which refers to many papers describing oculomotor events.
- Spatio-temporal representation of saccades and fixations. Feed into CNN???
    - Shows a very visual representation of scanpath pattens in eye tracking data.