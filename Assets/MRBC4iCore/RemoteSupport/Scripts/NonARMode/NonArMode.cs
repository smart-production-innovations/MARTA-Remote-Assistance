using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Check, if the application runs on a HMT1.
/// If yes, disable overlays which are not needed (e.g. Buttons which are not clickable on the HMT1 anyways)
/// </summary>
public class NonArMode : MonoBehaviour
{
    public GameObject[] unusedObjects;
    public Image[] unusedImages;
    public bool forceSmartGlassMode = false;

    private static bool initialized = false;
    private static bool _isSmartGlass;
    /// <summary>
    /// Smart glasses do not support the full range of functions and require the simplification of the GUI compared to tablets or smartphones.
    /// Is the connected mobile device a smart glasses?
    /// </summary>
    public static bool isSmartGlass
    {
        get
        {
            if (!initialized)
            {
                NonArMode nam = FindObjectOfType<NonArMode>();
                if (nam == null)
                {
                    _isSmartGlass = false;
                }
                else
                {
                    nam.CheckDevice();
                }
                initialized = true;
            }
            return _isSmartGlass;
        }
    }


    private void Awake()
    {
        if (isSmartGlass)
        {
            DisableOverlay();
        }
    }

    /// <summary>
    /// Check, if the device is a HMT1 by its device name
    /// </summary>
    private void CheckDevice()
    {
        string model = SystemInfo.deviceModel;
        if (model.Contains("RealWear") || model.Contains("T1100G") || forceSmartGlassMode)
        {
            _isSmartGlass = true;
        }
        else
        {
            _isSmartGlass = false;
        }
    }

    /// <summary>
    /// Disable unused buttons/overlays
    /// </summary>
    private void DisableOverlay()
    {
        foreach (GameObject go in unusedObjects)
        {
            go.SetActive(false);
        }
        foreach (Image img in unusedImages)
        {
            img.enabled = false;
        }
    }
}
