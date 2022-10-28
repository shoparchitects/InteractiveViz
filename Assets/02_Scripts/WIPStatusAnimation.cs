using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WIPStatusAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    private MeshRenderer _Renderer;

    void Start()
    {
        _Renderer = transform.GetComponentInChildren<MeshRenderer>();
    }

    void Update()
    {
        SetAlpha((Mathf.Sin(Time.time * 4) + 1) / 2);
    }

    public void SetAlpha(float a)
    {
        var color = _Renderer.sharedMaterial.color;
        color.a = a;
        _Renderer.sharedMaterial.SetColor("_Color",color);
    }
}
