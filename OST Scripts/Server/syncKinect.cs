using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class syncKinect : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        try {
            transform.position = new Vector3(gameObject.GetComponent<socketReceiver>().xKinectTracked, 
                                             gameObject.GetComponent<socketReceiver>().yKinectTracked,
                                             gameObject.GetComponent<socketReceiver>().zKinectTracked);
                    
            GameObject trackedPoint = GameObject.FindWithTag("kinect");
            trackedPoint.transform.localPosition = transform.position;
            //trackedPoint.transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);
        }
        catch {

            Debug.Log("Server offline. Host server to start transmitting data.");
        }

    }
}
