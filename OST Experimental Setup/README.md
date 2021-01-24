# OST Experimental Setup

Experimental scene setup:
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


- GameObject 'SceneManager'. Attached components: Network Manager, Network Manager HUD, Tiny Calibration, Spawn Targets and Spawn Targets Exp2.
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


- GameObject 'Hmd_Tracker_local' (tagged HMD). Attached component: Hmd_tracker_pose. 
- GameObject 'Tracker_MetaCameraRig_Transformation'. Tagged with 'TrackerMetaCameraTransform'. The transform values used for this GameObject are obtained from several SPAAM calibrations. approximate starting values: Position x=-0,02 y=0 z=-0,09; Rotation x=56,02 y=-3 z=-4,23. Invariant scale (1,1,1). 

- GameObject 'NeutralizeRotation'. Attached component: Neutralize Rotation. 

- GameObject 'MetaCameraRig' (default SDK prefab). 
    - params:  
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

- GameObject 'StereoCameras': Alignment User Settings disabled. 
- GameObject 'Left_camera': tagged with 'MainCamera'
- GameObject 'Right_camera': tagged with 'secondCamera'. 
- GameObject 'EnvironmentInitialization'. Attached component: 'Environment Configuration'.
    - params:
Slam Relocalization Active: uncheck
Surface Reconstruction Active: uncheck

- GameObject 'leftEye': empty GameObject. (leave empty)
- GameObject 'rightEye': empty GameObject. (leave empty)
- GameObject 'TrackerEye': empty GameObject. (leave empty)

- GameObject 'Target_Tracker_local'. Attached component: Target_tracker_pose.

- GameObject 'CheckerboardTracker'. Attached component: Checkerboard_tracker_pose. 
- GameObject 'CheckerboardModel'. This is the checkerboard prefab resized and translated in the same position w.r.t. the Vive Tracker. 
- GameObject 'RotationInterface'. Tagged with 'RotationInterface'. This is the prefab of the rotation gizmo used during the alignment phase.
- GameObject 'TranslationInterface'. Tagged with 'TranslationInterface'. This is the prefab of the translation gizmo used during the alignment phase.
the keybinds are the following ones:

- GameObject Volume: empty GameObject. (leave empty).







