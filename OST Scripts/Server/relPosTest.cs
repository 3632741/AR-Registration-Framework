using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class relPosTest : MonoBehaviour
{
    public GameObject b;
    public Vector3 relPos;
    // Start is called before the first frame update
    void Start()
    {
        relPos = transform.InverseTransformPoint(b.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        relPos = transform.InverseTransformPoint(b.transform.position);
    }
}
