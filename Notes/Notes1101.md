# 1.11.21
## Thoughts
- Format project focus to be around the use of commercial eye trackers for eye event classification in both a static and dynamic(/interactive) environment. The static environment will likely produce better results, as I only need to consider binary classification. The dynamic environment will require more comprehensive labelling systems, as I need to handle multi-label classification.
- Discuss the importance of high quality labelling in results by comparing the simplest systems with improved ones.
- Possible sampling/labelling schemes: Note: Manual operator labelling is always aided by some user input labelling.
    - Binary: Manual labelling by user input while sampling. Unlabelled blinks cause disruptions.
    - Binary: Manual labelling by user input while sampling. Blink labelled by user input.
    - Binary: Manual labelling by user input while sampling. Blink labelled by loss of data.
    - Binary: Manual operator labelling after sampling. Reliable.
    - Binary: Automatic labelling by sampling window around warping dot.
    - Multi-class: Automatic labelling by sampling window around warping/moving dot.
    - Multi-class: Manual operator labelling after sampling. Reliable.
    - Multi-class: Pre train? https://zenodo.org/record/1343920

## Veiledermøte
- Vær realistisk
- Ta deg tid til å lek, det blir bare interessant
- Stoffet som kommer ut av lekinga er veldig interessant for sensor
- Mulighet for data mining. Unlabelled.
- Ta en prat med Haakon og spill ball rundt prosjektet.
- Alt jeg har snakket om er relevant å skrive oppgave om. Data labelling i statisk miljø, data labelling i dynamisk miljø
- Det jeg har snakket om kan legges i flere deler av rapporten.
- Kan deles inn i flere implementasjoner i metode/resultat