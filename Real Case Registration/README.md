# Real Case Registration

This module can be used to provide a 3D visual stimulus to a person not wearing any HMD. The stimulus is designed to be easily replicable in AR, in such a way that a control group can be performed during perception/interaction experimental setups in AR environments. 

Below, an example is displayed: the stimulus as observed without any HMD, displayed through a green led, using the structure (see experimentalSetup.stl) and the the same stimulus as observed through an AR OST HMD. In the darkness, the perceived stimuli are the same.

![alt text](https://github.com/3632741/AR-Registration-Framework-PhD-Thesis/blob/main/Real%20Case%20Registration/perceivedStimulus.PNG)

This setup uses a led light connected to an Arduino Uno programmed to turn on for a chosen amount of time (1-9 seconds). The light can be controlled with an IR remote controller, which can be used to set the desired amount of time (presesing 1-9) or to trigger the start of the light stimulus (start/pause button). 
The whole circuit can be encased in the provided 3D-printed box, designed to be able to slide in specific positions of a metal rod. The box has a circular hole (1 cm radius), which can be internally covered with a small square of plastic (0.8 mm thick, stl provided) to diffuse the led light in a uniform circle, to be as similar as possible to the stimuli provided in AR. The obj file of a circle is provided to be imported in Unity. 
The metal rod can be placed in fixed positions of a metal structure made of perforated corner profiles. The 3D printable files of the hook supports are provided.  

To obtain similar stimuli, the experiment must be conducted in the darkness, as the whole metal structure's sight would otherwise provide a strong cue on the stimulus's 3D position.

The positions of the grid points, expressed in the HTC Vive reference system, can been obtained by positioning the led box in all the chosen positions and measuring its 3D world coordinates with a Vive Tracker attached on top of the center of the led light. The whole structure was designed to quickly remove the metal rod after the visual stimulus vanished, leaving an empty volume in front of the user. In this way, we deprived the users of the haptic feedback deriving from the finger's physical contact with the led box. 

In this folder all the required 3D printable files and the arduino project are provided.

