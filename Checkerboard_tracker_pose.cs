using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkerboard_tracker_pose : MonoBehaviour
{
    
    void Update()
    {
        try
        {
            GameObject checkerboard = GameObject.Find("Checkerboard_Data");
            gameObject.transform.position = checkerboard.transform.position;
            gameObject.transform.rotation = checkerboard.transform.rotation;
        }
        catch
        {

        }
    }

}