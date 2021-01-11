# OST Registration (tested in unity 2017.4.17f1)
###### SERVER ############
scene setup:
- VR head tracking, vr play area disabled. child: HMD tracker, with components: Steam VR_Tracked Object (origin none) and identify Tracker ID (write the ID of the vive tracker found under manage your vive trackers of steamVR).
- SPAAM_target: same as HMD tracker
- Checkerboard: same as HMD tracker
- KinectSocketReceiver with added components: SyncKinect and Socket Receiver. Add the port used for the Kinect data transmission (default on port 8888).
- scene controller with the same network manager and network manager hud as below. NB synced data must be the same in both projects.
###### CLIENT ############
SPAAM scene setup:
1 - SceneManager with following attachments:
    - network manager, network manager HUD same as exp scene.
    - SPAAM: 
        #RANSAC settings: Inlier Distance threshold = 0,0001 (suggested). Ransac Points per Batch = 6. Max Error          0,001 suggested. Toggle to apply intrinsic parameters: leave unchecked. 
        #Toggle to use Custom input parameters (debugging) can be used to ignore the data acquired from the     sensors and instead run the SPAAM algorythm on a set of user defined coordinates. 
        # Scale Factor and Rotation adjustment Shift = 0,003 (suggested) Adjustment Angle = 10 (suggested)
        # Rendering Resolution: add the resolution and projected pixel size of the HMD. For the Meta2, Resolution Width = 1280, Resolution Height = 1440, Pixel Size X = 4,84375e-05, Pixel Size Y = 5,9375e-05
        # Crosshair positions during calibration: define the x and y screen positions where the crosshair will            be displayed.
        # Alignment requested for SPAAM: number must be equal to the size of the specified crosshair position vector. suggested value = 15 to be used with RANSAC.
        # Crosshair size: size of the sprite used to display the crosshair. our sprite was 64x64px wide.
        # object tracked by vive tracker: target_tracker_local
        # HMD camera: HMD_tracker_local
        # original projection: leave empty, these field are display only.
        # Camera - Image plane reference frame alignment: check accordingly depending on your planes orientation.
        # Pixel Coordinates Transformations: metric conversion for the meta2.
        # remove crosshair sprite size shift: leave unchecked unless the center of the crosshair is shifted with respect to the center of the sprite.
        # use pixel dimensions during computations: leave unchecked unless needed.
    - Enable Calibration: toggle after initialization. Left and right Camera needs to be paired with the left camera of the metacamera rig. Calibration Parameters must be paired with the CalibrationParameters gameobject.
2 - Hmd_tracker_local. same settings for all the children but add the smooth script to left and right camera, with the following parameters: x =-7.5, Y=-8.5, X scale=1, Y scale=1, xres=16, yres=9, Material=cross_shader, x pixel = -675, y pixel = -604
3 - Target_Tracker_local : same as exp 2
4 - CalibrationParameters : attached component SavedParameters, leave unitialized
5 - Checkerboard_local : same as exp 2
    



Experimental scene setup:
- GameObject 'SceneManager' with 5 attached components: Network Manager, Network Manager HUD, Tiny Calibration, Spawn Targets and Spawn Targets Exp2.
- GameObject 'Hmd_Tracker_local' with one attached component: Hmd_tracker_pose. Child: Tracker_MetaCameraRig_Transformation (obtained from several SPAAM calibration). approximate starting values: Position x=-0,02 y=0 z=-0,09; Rotation x=56,02 y=-3 z=-4,23. Invariant scale (1,1,1). Child: NeutralizeRotation with one attached component: Neutralize Rotation. Child: MetaCameraRig with following settings: Meta manager enabled, Playback Dir none, Context Bridge MetaCameraRig(MetaContextBridge); Slam Localizer enabled with Loading Map Wait Time 10, Show Calibration UI enabled, Rotation Only Tracking enabled; Meta Compositor Script enabled with Enable WebCam unchecked, Enable Depth Occlusion unchecked, Enable 2D Warp checked, Enable Asyncronous Rendering checked. Meta Context Bridge and Webcam Off Canvas Handler with Logo Canvas Prefab set to none.
child: StereoCameras, Alignment User Settings disabled. child of MetaCameraRig: EnvironmentInitialization, with one component 'Environment Configuration', uncheck Slam Relocalization Active and Surface Reconstruction Active.
child of tracker_metacamrearig_transform: three empty objects leftEye, rightEye, TrackerEye.
- Target_Tracker_local with attached component Target_tracker_pose.
- Checkerboard_local with attached component Checkerboard_tracker_pose, and as child the checkerboard prefab resized and translated in the same position w.r.t. the vive tracker. Also requires two children "RotationInterface" with tag RotationInterface and "TranslationInterface" with tag "TranslationInterface", with the prefabs of the gizmos used during the alignment phase. Only translation is required.
the keybinds are the following ones:
- Empty gameobject Volume.


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
# Experimental Settings
- Numero Acquisizioni (number of times the entire array is displayed): we used 5. change at your own choice.
- Log Data Filename: an increasing index will be added at the end, e.g. writing "test_" here will generate "test_0", "test_1" and so on
# Interaction area settings
- Spawn Position: define the x, y, z coordinates where the items will be spawned. Measure the x, y, z coordinates by placing a vive tracker in the desired positions and saving the Vive Tracker Coordinates.
- Spawn Sequence: can be used to define a custom sequence. If left undefined, the vector of positions will be shuffled and a target will spawn into a random position until every item of the grid has been displayed. The process is repeated for several times (defined by "Numero Acquisizioni" in the Experimental Settings).
# Target Settings
- Size: represents the number of different objects models available. Every time a new object is spawned, a random one will be picked from the added library of objects.
- Element 0...N: represent the prefabs of the objects that will be instantiated.
- Head: Hmd_Tracker_local

