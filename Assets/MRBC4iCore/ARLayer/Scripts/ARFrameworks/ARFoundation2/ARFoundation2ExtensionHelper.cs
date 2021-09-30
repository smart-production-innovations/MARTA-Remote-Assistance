#if ARFoundation2

using UnityEngine;
using System.Collections;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

/// <summary>
/// Prepares AR foundation 2 features for access by other scripts
/// </summary>
[RequireComponent(typeof(ARPlaneManager))]
public class ARFoundation2ExtensionHelper : ARExtensionHelper
{
    private ARPlaneManager arPlaneManager;
    public override void Awake()
    {
        arPlaneManager = GetComponent<ARPlaneManager>();
    }

    /// <summary>
    /// List of all AR planes detected by the AR algorithm
    /// </summary>
    /// <returns></returns>
    public override List<ARLayerPlane> GetAllPlanes()
    {
        var list = new List<ARLayerPlane>();
        foreach (var item in arPlaneManager.trackables)
        {
            var renderer = item.GetComponent<Renderer>();
            if (renderer && renderer.isVisible)
            {
                list.Add(new ARLayerPlane(item.center, item.transform, item.boundary.ToArray(), CameraHelper.ARCamera)); //CameraHelper.CaptureCamera
            }
        }
        return list;
    }
}

#endif