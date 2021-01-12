# Kinect Color Tracking

This module can be used to track the user's finger using a colored fingercap, using a simple color filtering. It can be used to track objects of any size as long as they are of a particular color, but the system will always output a single 3D position corresponding to the centroid of the area segmented with the color filter.

This project has been made by editing one of the code samples (CoordinateMappingBasics-D2D) of the Kinect for Windows SDK 2.0. Refer to the official Microsoft SDK instructions to download the required drivers (https://www.microsoft.com/en-us/download/details.aspx?id=44561).

OpenCV needs to be installed for the system to work, otherwise the following includes won't find any correspondance: <opencv2/opencv.hpp>, "opencv2/highgui/highgui.hpp", "opencv2/imgproc/imgproc.hpp".

The program works as follows.
- When the executable is launched, the RGB feed, the segmented image and a configuration panel will be displayed. To close the program, press exit or close the settings window. If the video stream window is closed, another one will simply be reopened.
- The RGB feed is the raw video stream captured by the Kinect. The coordinates of the centroid of tracked area are displayed both in Image coordinates (X, Y expressed in pixel, Z in mm) and in Kinect Coordinates (measures in meters), using the Kinect default axis orientation.
- The configuration panel has the sliders to set the HSV values. For each channel, two values have to be set (low and high), depending on the type of filtering required (low-pass, high-pass, or band-pass). The chosen values will be saved in the savedParameters.txt files when the program is closed. On the next program launch, the starting parameters will be the last ones used. If multiple settings needs to be saved, rename the correct settings file 'savedParameters.txt' before launching the program. It is advised to make a copy first as if the parameters are changed run-time, the file will be overwritten.
- The segmented image panel is a binarized image where the tracked portion of the image is white, while the background is black. The program performs a color filtering based on the HSV values entered in the configuration panel. The tracked object should thus be of an easily identifiable color which is not present in the rest of the scene (e.g. green or blue). A morphological opening filter removes small objects from the foreground, while a morphological closing filter fill small holes. The centroid (center of mass) of the white area is computed (tracked point). If the computed RGB point does not find a correspondence in the depth map (the depth sensor has a lower resolution than the RGB camera), the closest depth point will be used instead.
- The system will then stream the x, y, z position of the tracked point (in Kinect coordinates) with an UDP socket. Global variables SERVER, BUFLEN, PORT can be changed as needed.
- To receive the data inside Unity, refer to the Kinect Registration folder.

![alt text](https://github.com/3632741/AR-Registration-Framework-PhD-Thesis/blob/main/Kinect%20Color%20Tracking/colorSegmentation.jpg)
![alt text](https://github.com/3632741/AR-Registration-Framework-PhD-Thesis/blob/main/Kinect%20Color%20Tracking/colorSegmentation2.jpg)
