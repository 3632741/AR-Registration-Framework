using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hmd_tracker_pose : MonoBehaviour
{
  
    void Update()
    {
        try
        {
            GameObject hmd = GameObject.Find("Hmd_Data");
            gameObject.transform.position = hmd.transform.position;
            gameObject.transform.rotation = hmd.transform.rotation;
        }
        catch
        {

        }
    }

}