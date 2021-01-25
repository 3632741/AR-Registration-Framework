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
- GameObject 'VR head tracking'.  
  Settings: 
  - VR play area: disabled.   
- GameObject 'HMD_Tracker'. Attached components: Steam VR_Tracked Object (origin none) and identify Tracker ID.  
   Settings:  
   - Tracker ID: write the ID of the vive tracker found under manage your vive trackers of steamVR.  
- GameObject 'SPAAM_Target_Tracker'. Attached components: Steam VR_Tracked Object (origin none) and identify Tracker ID.   
   Settings:   
   - Tracker ID: write the ID of the vive tracker found under manage your vive trackers of steamVR.  
- GameObject 'Checkerboard_Tracker'. Attached components: Steam VR_Tracked Object (origin none) and identify Tracker ID.   
   Settings:  
   - Tracker ID: write the ID of the vive tracker found under manage your vive trackers of steamVR.  
- GameObject 'KinectSocketReceiver'. Attached components: SyncKinect and Socket Receiver.  
   Settings:   
   - Port: Add the port used for the Kinect data transmission (default on port 8888).
   - X,Y,Z Kinect Tracked: leave empty, will be used to display the current tracked position.
- GameObject 'SceneController'. Attached components: network manager and network manager hud (same parameters as the client). NB synced data must be the same in both projects.
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


Final alignment performed by adding the tiny Calibration script to a gameobject.

Tiny Calibration params:
Adjustment Shift: 0,008 (suggested)
Adjustment Angle: 10 (suggested)
Focal Step: 5 (suggested)

Toggles:
skip calibration
second experiment
meta sdk experiment
Checker Board: Checkerboard_local
    

