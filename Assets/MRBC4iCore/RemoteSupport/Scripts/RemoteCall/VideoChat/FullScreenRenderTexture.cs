using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// set render texture to full screen resolution
/// </summary>
[RequireComponent(typeof(Camera))]
public class FullScreenRenderTexture : MonoBehaviour
{
    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam.targetTexture)
        {
            cam.targetTexture.width = Screen.width;
            cam.targetTexture.height = Screen.height;
        }
    }
}
