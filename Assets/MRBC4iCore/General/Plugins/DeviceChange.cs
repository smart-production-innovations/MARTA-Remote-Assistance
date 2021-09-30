using System;
using System.Collections;
using UnityEngine;

//https://forum.unity.com/threads/device-screen-rotation-event.118638/
//DougMcFarlane
public class DeviceChange : MonoBehaviour
{
    public static event Action<Vector2> OnResolutionChange;
    public static event Action<DeviceOrientation> OnOrientationChange;
    public static float CheckDelay = 0.5f;        // How long to wait until we check again.

    static Vector2 resolution;                    // Current Resolution
    static DeviceOrientation orientation;        // Current Device Orientation
    static bool isAlive = true;                    // Keep this script running?

    public static void AddListener(Action<Vector2> resolutionAction, Action<DeviceOrientation> orientaionAction)
    {
        OnResolutionChange += resolutionAction;
        OnOrientationChange += orientaionAction;

        resolution = Vector2.zero;
        orientation = DeviceOrientation.Unknown;
    }

    public static void RemoveListener(Action<Vector2> resolutionAction, Action<DeviceOrientation> orientaionAction)
    {
        OnResolutionChange -= resolutionAction;
        OnOrientationChange -= orientaionAction;
    }

    void Start()
    {
        StartCoroutine(CheckForChange());
    }

    IEnumerator CheckForChange()
    {
        resolution = Vector2.zero;
        orientation = DeviceOrientation.Unknown;

        while (isAlive)
        {

            // Check for a Resolution Change
            if (resolution.x != Screen.width || resolution.y != Screen.height)
            {
                resolution = new Vector2(Screen.width, Screen.height);
                if (OnResolutionChange != null) OnResolutionChange(resolution);
            }

            // Check for an Orientation Change
            switch (Input.deviceOrientation)
            {
                case DeviceOrientation.Unknown:            // Ignore
                case DeviceOrientation.FaceUp:            // Ignore
                case DeviceOrientation.FaceDown:        // Ignore
                    break;
                default:
                    if (orientation != Input.deviceOrientation)
                    {
                        orientation = Input.deviceOrientation;
                        if (OnOrientationChange != null) OnOrientationChange(orientation);
                    }
                    break;
            }

            yield return new WaitForSeconds(CheckDelay);
        }
    }

    void OnDestroy()
    {
        isAlive = false;
    }

}
