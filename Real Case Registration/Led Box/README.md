# Led Box
In this section, the Arduino project and 3D printable box used to convey a stimulus which is coherent with those displayed in AR are provided.
For the project, we used an Arduino Uno with an Arduino prototyping shield. The other components are an IR receiver, a (green) LED and a resistor. The image below display the general wiring. If the given code is used, the LED must be wired to I/O pin number 7 (yellow cable) and the IR receiver to the I/O pin number 11 (green cable).

The circuit can be powered with a 9V battery and is operated through a remote controller. Pressing numbers between 1-9 change the duration of the stimulus (from 1 to 9 seconds). To trigger the LED, simply press the "play" button on the remote controller.

The box can be printed without support flat on the base. Black filament is advised to reduce the amount of light bleed through the material. The light cover is a thin sheet of plastic which should be printed with the desired color of the stimuli (in our case, green), which should match the color of the LED. The plastic sheet diffuses the light from the LED filtering the higher frequencies of the spectrum, to produce an evenly colored circle. The back of the box can be used to house the 9V battery, while the frontal house-shaped hole is the housing for the IR receiver. Holes are house-shaped to not require additional support material during the printing process.

Two different lids are provided. The box is supposed to be fixed on a rod with 1.5cm of diameter. The first lid can be tied to the rod with zipties, the second one needs to be inserted directly on the rod from one of the endings. 

![alt text](https://github.com/3632741/AR-Registration-Framework-PhD-Thesis/blob/main/Real%20Case%20Registration/Led%20Box/Arduino.PNG)


