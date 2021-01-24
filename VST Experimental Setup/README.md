# VST Experimental Setup

This module includes the scripts and Unity scene setup required to perform two different perception/interaction AR experiments with the HTC Vive Pro used as AR VST HMD thanks to the ZED mini stereo cameras mounted to the front of the HMD.

The first experiment is a blind reaching task, which aims to isolate the visual perceptual error in the reaching task by removing the visual alignment cue (blind paradigm). The experiment is performed in the darkness in order to be able to have a coherent control group, who do not use any HMD, to have a baseline with which to compare the performance of the users when wearing HMDs. 

The second experiment is an alignment task, which aims at obtaining a comparison of the OST HMD with respect to the VST HMD in a functional and interactive task. The aim is to assess whether and how the distortions observed in the blind reaching experiment affect user interaction in more naturalistic settings, where multiple depth cues are available.

The OST Experimental Setup is explained in detail in its own module, and the same applies for the Real Case control group (see Real Case Registration).

Scene structure:
```bash
├───CheckerboardTracker
│   ├───CheckerboardModel
│   ├───RotationInterface
│   └───TranslationInterface
├───Directional Light
├───KinectSocketReceiver
├───KinectTracker
│   └───Kinect
│       └───KinectTrackedObject
├───SceneController
├───Volume
└───ZED_Rig_Stereo
    └───Camera_eyes
        ├───Left_eye
        │   └───Frame
        └───Right_eye
            └───Frame
```       
- GameObject 'directional light'. (Default Unity Scene directional light).
- GameObject 'GameController'. Attached components: TinyCalibration, spawn targets, spawn targets exp 2. Scale factor and rotation correction for tiny calibration same as in the OST. toggle to skip calibration can be used if the session is interrupted but the hmd is not removed to avoid reperforming the alignment task. toggle second experiment before starting the project to execute the interaction task. add the checkerboard gameobject to the Checker Board public variable.
- Zed_Rig_Stereo: default ZED camera prefab (tested with SDK 2.8.0). Left_eye is tagged MainCamera, Right_eye is tagged secondCamera.
- GameObject 'CheckerboardTracker'. Attached component: Checkerboard_tracker_pose. 
- GameObject 'CheckerboardModel'. This is the checkerboard prefab resized and translated in the same position w.r.t. the Vive Tracker. 
- GameObject 'TranslationInterface'. Tagged with 'TranslationInterface'. This is the prefab of the translation gizmo used during the alignment phase. The experimenter, before the experiment, needs to adjust the hologram of the checkerboard until it overlaps with the real one, which is tracked by a Vive Tracker. When the scene starts, the scene will be in Translation-Editing mode. 
The keybinds of the Translation-Editing mode are the following ones:
    - Y: Switch to Rotation mode.  
    - W: Moves the virtual checkerboard forward in the Z axis (away from the user).   
    - A: Moves the virtual checkerboard backward in the X axis (to the left of the user).
    - S: Moves the virtual checkerboard backwards in the Z axis (towards the user). R
    - D: Moves the virtual checkerboard forward in the X axis (to the right of the user). 
    - R: Moves the virtual checkerboard forward in the Y axis (towards the ceiling).   
    - F: Moves the virtual checkerboard backwards in the Y axis (towards the floor).   
    - X: Increases the adjustment step (each keypress results in a bigger translation/rotation).  
    - Z: Decreases the adjustment step (each keypress results in a smaller translation/rotation).  
    - Space: Saves the current parameters and continues to the experiment.
- GameObject 'RotationInterface'. Tagged with 'RotationInterface'. This is the prefab of the rotation gizmo used during the alignment phase. The experimenter, before the experiment, needs to adjust the hologram of the checkerboard until it overlaps with the real one, which is tracked by a Vive Tracker. When the scene starts, the scene will be in Translation-Editing mode. 
The keybinds of the Rotation-Editing mode are the following ones:
    - U: Switch to Translation mode.  
    - W: Increases the virtual checkerboard rotation over the X axis.  
    - A: Decreases the virtual checkerboard rotation over the Y axis.  
    - S: Decreases the virtual checkerboard rotation over the X axis.  
    - D: Increases the virtual checkerboard rotation over the Y axis.  
    - Q: Increases the virtual checkerboard rotation over the Z axis (clockwise with respect to the user optical axis).  
    - E: Decreases the virtual checkerboard rotation over the Z axis (anticlockwise with respect to the user optical axis).  
    - X: Increases the adjustment step (each keypress results in a bigger translation/rotation).  
    - Z: Decreases the adjustment step (each keypress results in a smaller translation/rotation).  
    - Space: Saves the current parameters and continues to the experiment.  
- Volume
- kinectTracker (tag: kinectTracker). attached component identify tracker id and steam vr tracked object, origin none. child: kinect with the applied transormation found with camera calibration. child: kinectTrackedObject (tagged: kinect)
