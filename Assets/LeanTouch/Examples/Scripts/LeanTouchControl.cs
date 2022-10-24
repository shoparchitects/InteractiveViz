using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton class to handle switching behavior between different objects that are using lean touch
/// </summary>
public class LeanTouchControl : MonoBehaviour
{
    /// <summary>
    /// Static field for Singleton Pattern
    /// </summary>
    public static LeanTouchControl control;


    public delegate void ModusEvent();

    public event ModusEvent OnDisableCameraLeanTouch;
    public event ModusEvent OnEnableCameraLeanTouch;

    //Set up singleton pattern
    void Awake()
    {
        if (control == null)
        {
            DontDestroyOnLoad(gameObject);
            control = this;
        }
        else if (control != this)
        {
            Destroy(gameObject);
        }
    }

    public void DisableCameraLeanTouch()
    {
        OnDisableCameraLeanTouch?.Invoke();
    }

    public void EnableCameraLeanTouch()
    {
        OnEnableCameraLeanTouch?.Invoke();
    }

    
}
