using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class packData : MonoBehaviour
{
   // public bool triggerToSendData;
    //public bool triggerToUseSecondTracker;
    public enum Tracker { HMD, CalibrationTarget, Checkerboard }
    public Tracker selectedTracker;

    void Update()
    {
        //if (triggerToSendData)
       // {
           // if (!triggerToUseSecondTracker)
           if(selectedTracker==Tracker.CalibrationTarget)
            {
                GameObject target = GameObject.Find("SPAAM_target");
                gameObject.transform.position = target.transform.position;
                gameObject.transform.rotation = target.transform.rotation;
            }
            else if(selectedTracker == Tracker.HMD)
            {
                GameObject hmd = GameObject.Find("HMD_Tracker");
                gameObject.transform.position = hmd.transform.position;
                gameObject.transform.rotation = hmd.transform.rotation;
            }
           else if(selectedTracker == Tracker.Checkerboard)
        {
            GameObject checkerboard = GameObject.Find("Checkerboard");
            gameObject.transform.position = checkerboard.transform.position;
            gameObject.transform.rotation = checkerboard.transform.rotation;
        }
      //  }
    }

}
