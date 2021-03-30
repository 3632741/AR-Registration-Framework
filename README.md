# AR Registration Framework
Authors: Giorgio Ballestin, Manuela Chessa, Fabio Solari.

Dept. of Informatics, Bioengineering, Robotics, and Systems Engineering, University of Genoa, Italy

Corresponding author: Giorgio Ballestin (e-mail: giorgio.ballestin@dibris.unige.it)

Scripts and tools to register different devices in the same reference frame. All the devices will be registered in the HTC Vive reference frame. The same techniques and methods can be applied to any OST/VST HMD with similar characteristics.

![alt text](https://github.com/3632741/AR-Registration-Framework/blob/main/FrameworkScheme.png)

The framework is composed of several modules:
- Kinect Color Tracking: this module uses a Kinect V2 to threshold the RGB-D data around specified HSV/RGB values, and is used to track the user's finger. The image is filtered with a morphological erosion filter to   remove noise and a morphological dilation filter to fill small gaps. The coordinates of the centroid of the thresholded blob are sent with a socket into Unity.

- Kinect Registration: The Kinect is registered inside the HTC Vive reference system by using stereo camera calibration, with a Vive Tracker and a ZED mini stereo camera mounted on a perforated corner metal profile. The 3D printable files for this setup are given. A similar approach can be used with any camera as long as it can be fixed in a known position with respect to the Vive Tracker, which must be rigidly attached to the Kinect.

- OST Registration: this module registers the OST HMD (Meta2) into the HTC Vive System. The manufacturer tracking is disabled and the HMD is tracked with a Vive Tracker. The tracker is registered to the HMD reference frame by means of a ZED mini stereo camera which is placed in a fixed position with respect to the tracker and the HMD (3D printable files for the mount are given). The user's eye positions are found by first using the Single Point Active Alignment Method (SPAAM) several times with a 3D printable mannequin (files are given) which has the ZED mini stereo camera as eyes, and obtaining a general profile. Each user then performs an alignment task with a checkerboard to eliminate the residual alignment error.

- VST Registration: This module is composed by the same checkerboard alignment task used in the last step of the OST registration. The VST device (HTC Vive) is already registered inside the HTC reference system, thus only the minor residual alignment error is compensated. The files to register the ZED mini in different positions (frontal or on top of the HMD) are also given. 

- Blind Reaching Control Group: This module contains the structure and 3D printable files of the setup used to perform a blind reaching task with the control group who does not wear any HMD. The structure is build using perforated corner metal profiles, 3D printed hooks and a metal rod with a sliding Led Box (see Led Box module). Scripts to save the positions of the displayed stimuli with a Vive Tracker are given.

- OST Experimental Setup: In this module, the scripts used to perform two different experiments using the OST HMD registered with the previous modules are given. The first experiment is a blind reaching task. The second experiment is an active alignment task.

- VST Experimental Setup: In this module, the scripts used to perform two different experiments using the VST HMD registered with the previous modules are given. The first experiment is a blind reaching task. The second experiment is an active alignment task.


