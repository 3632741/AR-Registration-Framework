# OST Experimental Setup

This module includes the scripts and Unity scene setup required to perform two different perception/interaction AR experiments with the Meta2 OST HMD by Metavision.

The first experiment is a blind reaching task, which aims to isolate the visual perceptual error in the reaching task by removing the visual alignment cue (blind paradigm). The experiment is performed in the darkness in order to be able to have a coherent control group, who do not use any HMD, to have a baseline with which to compare the performance of the users when wearing HMDs. 

The second experiment is an alignment task, which aims at obtaining a comparison of the OST HMD with respect to the VST HMD in a functional and interactive task. The aim is to assess whether and how the distortions observed in the blind reaching experiment affect user interaction in more naturalistic settings, where multiple depth cues are available.

The VST Experimental Setup is explained in detail in its own module, and the same applies for the Real Case control group (see Real Case Registration).

# Scene Structure
```bash 
├───CheckerboardTracker
│   ├───CheckerboardModel
│   ├───RotationInterface
│   └───TranslationInterface
├───Directional Light
├───Hand_Tracker
├───HMD_Tracker
│   ├───leftEye
│   ├───rightEye
│   ├───TrackerEye
│   └───Tracker_Metacamerarig_Transformation
│       └───NeutralizeRotation
│           └───MetaCameraRig
│               ├───EnvironmentInitialization
│               ├───InteractionEngineSettings
│               └───MetaCameras
│                   ├───AudioListener
│                   ├───StereoCameras
│                   │   ├───EventCamera
│                   │   ├───LeftCamera
│                   │   └───RightCamera
│                   └───VirtualWebcam
│                       └───ContentCamera
├───SceneManager
└───Volume
```


- **GameObject 'SceneManager'.** Attached components: Network Manager, Network Manager HUD, Tiny Calibration, Spawn Targets and Spawn Targets Exp2.
    - Network Manager params:  
don't destroy on load: checked  
run in background: checked  
log level: info  
offline scene: none  
online scene: none  
network address: localhost  
network port: 7777  
max delay: 0,01  
max buffered packets: 16  
packet fragmentation: checked  
MatchMaker Host URI: mm.unet.unity3d.com  
MatchMaker Port: 443  
Match Name: default  
Maximum Match Size: 4  
Spawn Info  
Player prefab: none  
Registered Spawnable Prefab: Synced_Data  

    - Network Manager HUD params:  
Show Runtime GUI: checked  
GUI Horizontal Offset: 0  
GUI Vertical Offset: 0  

    - Tiny Calibration params:  
Adjustment Shift: 0,008 (suggested)  
Adjustment Angle: 10 (suggested)  
Focal Step: 5 (suggested)  

    - Toggles:  
skip calibration  
second experiment  
meta sdk experiment  
Checker Board: Checkerboard_local  


    - Spawn Targets params:  
Experimental Settings:  
Numero Acquisizioni (number of displayed stimuli): 27 (in our study, but can be changed).   
Log Data Filename: an increasing index will be added at the end, e.g. writing "test_" here will generate "test_0", "test_1" and so on  
Interaction Area Settings:   
defines the number of nodes of the grid along the three axis: depth (z axis) width (x axis) and height (y axis).  
Spawn Distance: distance between nodes (cm).  
User Distance: distance from the user point of view to the center of the grid volume (cm).  
Volume: item Volume in the scene.  
Head: Hmd_Tracker_Local.  
Target: Sphere  
Target size: change to rescale your target prefab.  

    - Spawn Targets Exp 2 params:  
Experimental Settings  
Numero Acquisizioni (number of times the entire array is displayed): we used 5. change at your own choice.  
Log Data Filename: an increasing index will be added at the end, e.g. writing "test_" here will generate "test_0", "test_1" and so on  
Interaction area settings  
Spawn Position: define the x, y, z coordinates where the items will be spawned. Measure the x, y, z coordinates by placing a vive tracker in the desired positions and saving the Vive Tracker Coordinates.  
Spawn Sequence: can be used to define a custom sequence. If left undefined, the vector of positions will be shuffled and a target will spawn into a random position until every item of the grid has been displayed. The process is repeated for several times (defined by "Numero Acquisizioni" in the Experimental Settings).  
Target Settings  
Size: represents the number of different objects models available. Every time a new object is spawned, a random one will be picked from the added library of objects.  
Element 0...N: represent the prefabs of the objects that will be instantiated.  
Head: Hmd_Tracker_local  


- **GameObject 'Hmd_Tracker_local'**. Tagged with 'HMD'. Attached component: Hmd_tracker_pose. 
- **GameObject 'Tracker_MetaCameraRig_Transformation'.** Tagged with 'TrackerMetaCameraTransform'. The transform values used for this GameObject are obtained from several SPAAM calibrations. 
    - Approximate starting values, if using the provided mount:  
    Position: x=-0,02; y=0; z=-0,09  
    Rotation: x=56,02; y=-3; z=-4,23  
    Scale: x=1; y=1; z=1  

- **GameObject 'NeutralizeRotation'.** Attached component: Neutralize Rotation. 

- **GameObject 'MetaCameraRig'.** Default SDK prefab. 
    - Params:  
Meta manager: enabled  
Playback Dir: none  
Context Bridge: MetaCameraRig(MetaContextBridge)  
Slam Localizer: enabled, with Loading Map Wait Time 10   
Show Calibration UI: enabled  
Rotation Only Tracking: enabled  
Meta Compositor Script: enabled with Enable WebCam unchecked  
Enable Depth Occlusion: unchecked  
Enable 2D Warp: checked  
Enable Asyncronous Rendering: checked  
Meta Context Bridge and Webcam: Off   
Canvas Handler Logo Canvas Prefab: none  

- **GameObject 'StereoCameras':** Alignment User Settings disabled. 
- **GameObject 'Left_camera':** tagged with 'MainCamera'
- **GameObject 'Right_camera':** tagged with 'secondCamera'. 
- **GameObject 'EnvironmentInitialization'.** Attached component: 'Environment Configuration'.
    - Params:  
Slam Relocalization Active: uncheck  
Surface Reconstruction Active: uncheck  

- **GameObject 'leftEye':** empty GameObject. (leave empty)
- **GameObject 'rightEye':** empty GameObject. (leave empty)
- **GameObject 'TrackerEye':** empty GameObject. (leave empty)

- **GameObject 'Target_Tracker_local'.** Attached component: Target_tracker_pose.

- **GameObject 'CheckerboardTracker'.** Attached component: Checkerboard_tracker_pose. 
- **GameObject 'CheckerboardModel'.** This is the checkerboard prefab resized and translated in the same position w.r.t. the Vive Tracker. 
- **GameObject 'TranslationInterface'.** Tagged with 'TranslationInterface'. This is the prefab of the translation gizmo used during the final alignment phase. The experimenter, before the experiment, needs to adjust the hologram of the checkerboard until it overlaps with the real one, which is tracked by a Vive Tracker. When the scene starts, the scene will be in Translation-Editing mode. 
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
- **GameObject 'RotationInterface'.** Tagged with 'RotationInterface'. This is the prefab of the rotation gizmo used during the final alignment phase. The experimenter, before the experiment, needs to adjust the hologram of the checkerboard until it overlaps with the real one, which is tracked by a Vive Tracker. When the scene starts, the scene will be in Translation-Editing mode. 
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
  
- **GameObject Volume:** empty GameObject. (leave empty).







