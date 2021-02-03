# OST Registration (tested in unity 2017.4.17f1)
 # Server Scene Setup
```bash 
├───Checkerboard_Tracker
├───EventSystem
├───KinectSocketReceiver
├───SceneController
├───SPAAM_Target_Tracker
└───VR Head Tracking
    └───HMD_Tracker
```
- **GameObject 'VR head tracking'.**  
  Settings: 
  - VR play area: disabled.   
- **GameObject 'HMD_Tracker'.** Attached components: Steam VR_Tracked Object (origin none) and identify Tracker ID.  
   Settings:  
   - Tracker ID: write the ID of the vive tracker found under manage your vive trackers of steamVR.  
- **GameObject 'SPAAM_Target_Tracker'.** Attached components: Steam VR_Tracked Object (origin none) and identify Tracker ID.   
   Settings:   
   - Tracker ID: write the ID of the vive tracker found under manage your vive trackers of steamVR.  
- **GameObject 'Checkerboard_Tracker'.** Attached components: Steam VR_Tracked Object (origin none) and identify Tracker ID.   
   Settings:  
   - Tracker ID: write the ID of the vive tracker found under manage your vive trackers of steamVR.  
- **GameObject 'KinectSocketReceiver'.** Attached components: SyncKinect and Socket Receiver.  
   Settings:   
   - Port: Add the port used for the Kinect data transmission (default on port 8888).
   - X,Y,Z Kinect Tracked: leave empty, will be used to display the current tracked position.
- **GameObject 'SceneController'.** Attached components: network manager and network manager hud (same parameters as the client). NB synced data must be the same in both projects.
# Client Scene Setup
```bash 
├───CalibrationParameters
├───Checkerboard_Tracker
│   ├───CheckerboardModel
│   ├───RotationInterface
│   └───TranslationInterface
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
├───SceneController
└───Target_Tracker
```

- **GameObject 'SceneManager'.** Attached Components: 'Network Manager', 'Network Manager HUD', 'SPAAM' **OR** 'tiny Calibration', 'Enable Calibration'. **If using the 'SPAAM' calibration, the Mathnet library is required.**
  Parameters:  
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
  - SPAAM params:   
    Inlier Distance threshold: 0,0001 (suggested).  
    Ransac Points per Batch: 6.  
    Max Error: 0,001 suggested.  
    Toggle to apply intrinsic parameters: leave unchecked.  
    Toggle to use Custom input parameters (debugging): can be used to ignore the data acquired from the sensors, and run the SPAAM algorythm on a set of user defined coordinates instead.  
    Adjustment Shift: 0,003 (suggested).  
    Adjustment Angle: 10 (suggested).    
    Rendering Resolution: add the resolution and projected pixel size of the HMD. For the Meta2, Resolution Width = 1280, Resolution Height = 1440, Pixel Size X = 4,84375e-05, Pixel Size Y = 5,9375e-05.
    Crosshair positions during calibration: define the x and y screen positions where the crosshair will be displayed.    
    Alignment requested for SPAAM: number must be equal to the size of the specified crosshair position vector. suggested value = 15 to be used with RANSAC.    
    Crosshair size: size of the sprite used to display the crosshair. our sprite was 64x64px wide.   
    Object tracked by vive tracker: Add the GameObject 'target_tracker' from the scene hierarchy.    
    HMD camera: Add the GameObject 'HMD_tracker' from the scene hierarchy.    
    Original projection: leave empty, these field are display only.    
    Camera - Image plane reference frame alignment: check accordingly depending on your planes orientation.   
    Pixel Coordinates Transformations: metric conversion for the meta2.   
    Remove crosshair sprite size shift: leave unchecked unless the center of the crosshair is shifted with respect to the center of the sprite.   
    Use pixel dimensions during computations: leave unchecked unless needed.    
  - Enable Calibration params:  
    toggle after initialization.   
    Left and right Camera needs to be paired with the left camera of the metacamera rig.  
    Calibration Parameters must be paired with the CalibrationParameters gameobject. 
  - Tiny Calibration params:
Adjustment Shift: 0,008 (suggested)  
Adjustment Angle: 10 (suggested)  
Focal Step: 5 (suggested)  
Skip calibration: toggle to skip calibration.  
Second experiment: toggle to do the active alignment task, leave unchecked for the blind reaching.  
Meta sdk experiment: toggle to use the Meta origin as the center of the world reference frame (experimental).  
Checker Board: Add the 'Checkerboard_tracker' GameObject from the scene hierarchy.  
- **GameObject 'Hmd_Tracker_local'.** Tagged with 'HMD'. Attached component: Hmd_tracker_pose. 
- **GameObject 'Tracker_MetaCameraRig_Transformation'.** Tagged with 'TrackerMetaCameraTransform'. The transform values used for this GameObject are obtained from several SPAAM calibrations. 
    - Approximate starting values, if using the provided mount:  
    Position: x=-0,02; y=0; z=-0,09  
    Rotation: x=56,02; y=-3; z=-4,23  
    Scale: x=1; y=1; z=1  

- **GameObject 'NeutralizeRotation'.** Attached component: Neutralize Rotation. 

- **GameObject 'MetaCameraRig'** (default SDK prefab). 
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

- **GameObject 'StereoCameras'**: Alignment User Settings disabled. 
- **GameObject 'Left_camera':** tagged with 'MainCamera'. **IF USING THE SPAAM SCRIPT ON THE SCENECONTROLLER, Attach the component: 'smooth'.**  
  Params:  
  - X: -7.5  
  - Y: -8.5  
  - X scale: 1  
  - Y scale: 1  
  - xres= 16  
  - yres= 9  
  - Material: cross_shader  
  - x pixel : -675  
  - y pixel : -604  
- **GameObject 'Right_camera':** tagged with 'secondCamera'. **IF USING THE SPAAM SCRIPT ON THE SCENECONTROLLER, Attach the component: 'smooth'.**  
  Params:  
  - X: -7.5  
  - Y: -8.5  
  - X scale: 1  
  - Y scale: 1  
  - xres= 16  
  - yres= 9  
  - Material: cross_shader  
  - x pixel : -675  
  - y pixel : -604  
- **GameObject 'EnvironmentInitialization'.** Attached component: 'Environment Configuration'.
    - Params:  
Slam Relocalization Active: uncheck  
Surface Reconstruction Active: uncheck  

- **GameObject 'leftEye':** empty GameObject. (leave empty)
- **GameObject 'rightEye':** empty GameObject. (leave empty)
- **GameObject 'TrackerEye':** empty GameObject. (leave empty)
- **GameObject 'Target_Tracker_local'.**  Attached component: Target_tracker_pose.
- **GameObject 'CalibrationParameters'.** attached component SavedParameters, leave unitialized
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





    

