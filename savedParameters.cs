using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class savedParameters : MonoBehaviour
{

    public Vector3 leftEyePosition;
    public Quaternion leftEyeRotation;
    public Vector3 rightEyePosition;
    public Quaternion rightEyeRotation;

    //double focalLength = 100;
    public double SensorSizeX_left;
    public double SensorSizeY_left;
    public double LensShiftX_left;
    public double LensShiftY_left;

    public double SensorSizeX_right;
    public double SensorSizeY_right;
    public double LensShiftX_right;
    public double LensShiftY_right;

    public bool leftEyeCalibrated = false;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }


}
