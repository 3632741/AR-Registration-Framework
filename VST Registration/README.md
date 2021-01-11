# VST Registration
The VST is already registered in its own reference system; this folder contains a few tools to register the ZED mini with respect to the HMD.
Specifically, this folder contains:
- The IdentifyTrackerID script: automatically binds the correct device number to a SteamVR_Tracked_Object (ViveTracker) by specifying the unique tracker ID obtainable from the     SteamVR settings. Should be used whenever a Vive Tracker is needed in the project.
- The tinyCalibration script: can be used to modify the left/right eye position by performing, one eye per time, an alignment task with a tracked checkerboard. Used to compensate small errors in the ZED camera mount positioning.
- 3D Printable Files folder: contains two different mounts to attach the ZED mini camera either on top of the Vive/Vive pro or to the front (manufacturer mount). If the former mount is used, the ZED mini position must be obtained by attaching a Vive Tracker to its housing in the mount. The folder also contains a clip which can be used to attach a vive tracker to the corner of an IKEA VARIERA metal table, used to track the checkerboard used for the alignment task.

