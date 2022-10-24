using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//tODO - come up with better name
public class LeanTouchControlConnector : MonoBehaviour
{
    public void DisableCameraLeanTouch()
    {
        LeanTouchControl.control.DisableCameraLeanTouch();
    }

    public void EnableCameraLeanTouch()
    {
        LeanTouchControl.control.EnableCameraLeanTouch();
    }
}
