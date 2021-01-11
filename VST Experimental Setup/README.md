# VST Experimental Setup

Scene structure:
- Default directional light
- GameController: TinyCalibration, spawn targets, spawn targets exp 2. Scale factor and rotation correction for tiny calibration same as in the OST. toggle to skip calibration can be used if the session is interrupted but the hmd is not removed to avoid reperforming the alignment task. toggle second experiment before starting the project to execute the interaction task. add the checkerboard gameobject to the Checker Board public variable.
- Zed_Rig_Stereo: default ZED camera prefab (tested with SDK 2.8.0). Left_eye is tagged MainCamera, Right_eye is tagged secondCamera.
- CheckerBoard: attached component steamvr tracked object and identify tracker id. child: checkerboard with the attached RotationInterface and TranslationInterface gizmo.
- Volume
- kinectTracker (tag: kinectTracker). attached component identify tracker id and steam vr tracked object, origin none. child: kinect with the applied transormation found with camera calibration. child: kinectTrackedObject (tagged: kinect)
