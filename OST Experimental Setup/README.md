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


- GameObject 'SceneManager' with 5 attached components: Network Manager, Network Manager HUD, Tiny Calibration, Spawn Targets and Spawn Targets Exp2.
Network Manager params:
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

Network Manager HUD params:
Show Runtime GUI: checked
GUI Horizontal Offset: 0
GUI Vertical Offset: 0

Tiny Calibration params:
Adjustment Shift: 0,008 (suggested)
Adjustment Angle: 10 (suggested)
Focal Step: 5 (suggested)

Toggles:
skip calibration
second experiment
meta sdk experiment
Checker Board: Checkerboard_local


Spawn Targets params:
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

Spawn Targets Exp 2 params:
-- Experimental Settings
- Numero Acquisizioni (number of times the entire array is displayed): we used 5. change at your own choice.
- Log Data Filename: an increasing index will be added at the end, e.g. writing "test_" here will generate "test_0", "test_1" and so on
-- Interaction area settings
- Spawn Position: define the x, y, z coordinates where the items will be spawned. Measure the x, y, z coordinates by placing a vive tracker in the desired positions and saving the Vive Tracker Coordinates.
- Spawn Sequence: can be used to define a custom sequence. If left undefined, the vector of positions will be shuffled and a target will spawn into a random position until every item of the grid has been displayed. The process is repeated for several times (defined by "Numero Acquisizioni" in the Experimental Settings).
-- Target Settings
- Size: represents the number of different objects models available. Every time a new object is spawned, a random one will be picked from the added library of objects.
- Element 0...N: represent the prefabs of the objects that will be instantiated.
- Head: Hmd_Tracker_local


- GameObject 'Hmd_Tracker_local' (tagged HMD) with one attached component: Hmd_tracker_pose. Child: Tracker_MetaCameraRig_Transformation (tagged TrackerMetaCameraTransform, obtained from several SPAAM calibration). approximate starting values: Position x=-0,02 y=0 z=-0,09; Rotation x=56,02 y=-3 z=-4,23. Invariant scale (1,1,1). Child: NeutralizeRotation with one attached component: Neutralize Rotation. Child: MetaCameraRig with following settings: Meta manager enabled, Playback Dir none, Context Bridge MetaCameraRig(MetaContextBridge); Slam Localizer enabled with Loading Map Wait Time 10, Show Calibration UI enabled, Rotation Only Tracking enabled; Meta Compositor Script enabled with Enable WebCam unchecked, Enable Depth Occlusion unchecked, Enable 2D Warp checked, Enable Asyncronous Rendering checked. Meta Context Bridge and Webcam Off Canvas Handler with Logo Canvas Prefab set to none.
child: StereoCameras, Alignment User Settings disabled. child: Left_camera (tagged MainCamera) and Right_camera (tagged secondCamera). child of MetaCameraRig: EnvironmentInitialization, with one component 'Environment Configuration', uncheck Slam Relocalization Active and Surface Reconstruction Active.
child of tracker_metacamrearig_transform: three empty objects leftEye, rightEye, TrackerEye.
- Target_Tracker_local with attached component Target_tracker_pose.
- Checkerboard_local with attached component Checkerboard_tracker_pose, and as child the checkerboard prefab resized and translated in the same position w.r.t. the vive tracker. Also requires two children "RotationInterface" with tag RotationInterface and "TranslationInterface" with tag "TranslationInterface", with the prefabs of the gizmos used during the alignment phase. Only translation is required.
the keybinds are the following ones:
- Empty gameobject Volume.







