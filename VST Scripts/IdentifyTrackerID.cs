using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class IdentifyTrackerID : MonoBehaviour
{
    public string trackerId;
    //private SteamVR_TrackedObject trackedObject;

    // Start is called before the first frame update
    void Start()
    {
       // trackedObject = GetComponent<SteamVR_TrackedObject>();



        uint index = 0;
        var error = ETrackedPropertyError.TrackedProp_Success;
        for (uint i = 0; i < 16; i++)
        {
            var result = new System.Text.StringBuilder((int)64);
            OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_SerialNumber_String, result, 64, ref error);
             if (result.ToString().Contains(trackerId))
              {
                 index = i;
              //  break;
               //  Debug.Log("serial number: " + result.ToString() + " device number: " + i);
            }
        }


        GetComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex)index;
    }

  
}
