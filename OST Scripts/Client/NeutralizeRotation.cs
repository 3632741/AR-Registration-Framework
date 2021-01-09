using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Neutralizes any translation/rotation/scaling of a child GameObject by applying the inverse transformation.
// In order to use this class to neutralize tracking of the Meta headset, create a new empty GameObject and
// place this script on it. Then make the MetaCameraRig GameObject the only child of this new GameObject.
// You can then parent this GameObject by a further empty GameObject on which you can apply any custom
// transformation (such as third-party tracking).

public class NeutralizeRotation : MonoBehaviour
{ 
 // If SLAM tracking is disabled and the headset tracks only orientation through the IMU, you can enable this.
 //   public bool NeutralizeRotationOnly = true;

    private Transform child;

    private void Start()
    {
       Debug.Assert(transform.childCount == 1, "Object with TransformNeutralizer must have exactly one child.");
       child = transform.GetChild(0);
    }

    public void LateUpdate()
    {

        Quaternion childLocalOrientation = child.localRotation;
        Quaternion inverseOrientation = Quaternion.Inverse(childLocalOrientation);
        transform.localRotation = inverseOrientation;

    }
    
}