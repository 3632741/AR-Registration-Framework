# Kinect Registration
This module is used to track a 3D point with a simple color tracking algorithm and send its position to unity in real-time. It is composed by two parts: a C++ program, which uses the kinect V2 SDK and openCV to perform the image segmentation and isolate the 3D point, which is then sent to unity, which reads the data through a C# script. 

The C++ program is provided in the folder "Kinect Color Tracking" on the root dir.

This folder contains the script to use in unity to receive the data (syncKinect.cs). Simply attach the script to an empty gameObject. The position of the gameObject will be updated with the 3D position of the tracked point, expressed with respect to the Kinect Reference Frame. To express the tracked point position with respect to the HTC Vive reference frame, the position of the Kinect in the HTC Vive reference frame must be known, and the tracked object must be set as child of a gameObject with the Kinect position (in the HTC Vive reference frame) as transform.  

To obtain such position it is sufficient to attach a Vive Tracker (or a controller) rigidly and in a known position with respect to the Kinect. To do this, we attached the Kinect to a perforated metal angular profile, and then attached a Vive Tracker on a 3D printer part which can house a ZED mini in a known position. We then obtained the transformation between the Zed Mini and the Kinect Camera through camera calibration, and since the transformation between the ZED mini and the Vive Tracker is known from the CAD of the 3D printed part, we can thus track the Kinect position (and therefore the 3D point tracked by the Kinect) in the HTC Vive Reference frame. 

For more details on the process refer to the related study.

The 3D printable files to attach the Vive Tracker to the Kinect are given.
Kinect Lock p1 and p2 are used to keep the kinect from rotating on the perforated corner profile.
The Kinect must be screwed rigidly on top of the profile to keep the registration valid; a 1/4'' screw is needed. The CAD of a 3D printable 1/4'' screw is provided (cut at the desired length).
An aproximate CAD of the used perforated corner profile is given, but any similar approach is valid as long as the Vive Tracker is rigidly attached to the Kinect in some way.
The 3D printable alignment pin can be used to avoid screwing the Vive Tracker during experimentation, while retaining a reasonably accurate tracking of the Kinect device.
All the parts, apart from the alignment pin, can be printed without supports.


