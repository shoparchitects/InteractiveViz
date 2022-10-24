using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeanTouchCameraCustomManagement : MonoBehaviour
{
    public List<MonoBehaviour> LeanTouchComponents;
    private Dictionary<MonoBehaviour, bool> ComponentInitialValue = new Dictionary<MonoBehaviour, bool>();

    private void OnEnable()
    {
        LeanTouchControl.control.OnDisableCameraLeanTouch += Control_OnDisableCameraLeanTouch;
        LeanTouchControl.control.OnEnableCameraLeanTouch += Control_OnEnableCameraLeanTouch;
    }

    private void OnDisable()
    {
        LeanTouchControl.control.OnDisableCameraLeanTouch -= Control_OnDisableCameraLeanTouch;
        LeanTouchControl.control.OnEnableCameraLeanTouch -= Control_OnEnableCameraLeanTouch;
    }

    private void Start()
    {
        foreach(var c in LeanTouchComponents)
        {
            ComponentInitialValue.Add(c, c.enabled);
        }
    }

    private void Control_OnEnableCameraLeanTouch()
    {
        foreach (var c in LeanTouchComponents)
        {
            c.enabled = ComponentInitialValue[c];
        }
    }

    private void Control_OnDisableCameraLeanTouch()
    {
        foreach(var c in LeanTouchComponents)
        {
            c.enabled = false;
        }
    }
}
