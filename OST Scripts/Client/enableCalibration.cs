using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enableCalibration : MonoBehaviour
{
    public bool disableCalibration;
    public GameObject leftCamera;
    public GameObject rightCamera;
    public GameObject calibrationParameters;
    // Start is called before the first frame update
    void Start()
    {

        if (disableCalibration)
        {
           leftCamera.GetComponent<smooth>().enabled = false;
           rightCamera.GetComponent<smooth>().enabled = false;
            gameObject.GetComponent<SPAAM>().enabled = false;
            calibrationParameters.GetComponent<savedParameters>().enabled = false;
        }
    }

}
