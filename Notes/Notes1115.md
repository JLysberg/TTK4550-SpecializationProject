# 15.11.21
## Thoughts
- Is there a value proposition in using Bezier curves to interpolate and graph smooth pursuit scanpaths?
    - How good are simple line-graphs from raw data?
    - If bezier curves are used, a lot of additional information can be gathered from e.g. curve bounding box, curvature etc.
- Experiment with having time delta as an additional generated data feature such that blinks can be classified and not simply disregarded pre-classification
    1. Classify blinks in pre-process step, consider them contaminations and remove.
        - Pro: Possibly better classification accuracy for Fix, Sacc, SP.
        - Con: Some data points are lost due to overestimate in blink window (if an overestimate is required). 
    2. Accurately post-label blinks after pre-process classification, add time-delta- and time-since-data-loss-features, let ML-model classify the additional class. 
        - Pro: If there is inherent information in features other than time-delta, ML-agent will be better at classification than a manual algorithm.
        - Con: Requires additonal labelling work. Might impact Fix, sacc, SP classification accuracy.
    - Key question in deciding this dilemma: Can I design a manual algorithm which performs _as good or better_ than a manual labelling agent?
- Post-classification processing.
    - Remove micro-saccades and SPs.