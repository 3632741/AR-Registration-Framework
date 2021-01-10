using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchProjection : MonoBehaviour
{
    [Header("Toggle to use custom projection by default")]
    public bool enableCorrectedProjection;

    [Header("Custom transform position")]
    public float t1 = 0.015f, t2 = 0.05f, t3 = -0.2f;

    [Header("Custom transform rotation")]
    public float r1= 62.5f, r2=0, r3=0;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            if (enableCorrectedProjection)
            {
                GameObject.FindWithTag("MainCamera").transform.rotation = Quaternion.Euler(r1, r2, r3);
                GameObject.FindWithTag("MainCamera").transform.position = new Vector3(t1, t2, t3);
                enableCorrectedProjection = false;
            }
            else
            {
                enableCorrectedProjection = true;
            }
        }
    }
}
