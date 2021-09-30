using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

/// <summary>
/// static helper class witch provides the specific scene cameras and camera properties
/// </summary>
public static class CameraHelper
{
    /// <summary>
    /// Get the augmented reality camera in the scene.
    /// Use the web camera as fallback camera when non-AR mode is active.
    /// </summary>
    public static Camera ARCamera
    {
        get
        {
            var cam = CameraHelper.ARCameraOnly;
            if (!cam) cam = WebCamOnly;
            if (!cam) cam = MainCamera;
            return cam;
        }
    }

    /// <summary>
    /// get the augmented reality camera in the scene without fallback camera for non-AR mode
    /// </summary>
    private static Camera ARCameraOnly
    {
        get
        {
            var cams = SearchHelper.FindSceneObjectsOfTypeAll<Camera>();
            Camera cam = cams.FirstOrDefault(x => x.gameObject.tag == "ARCamera");
            return cam;
        }
    }

    /// <summary>
    /// Get the web camera for non AR mode in the scene.
    /// Use the AR camera as fallback camera when AR mode is active.
    /// </summary>
    public static Camera WebCam
    {
        get
        {
            var cam = CameraHelper.WebCamOnly;
            if (!cam) cam = ARCameraOnly;
            if (!cam) cam = MainCamera;
            return cam;
        }
    }

    /// <summary>
    /// Listing of all cameras in the scene to which the WebCam tag is assigned
    /// </summary>
    /// <param name="includeInactive">Should cameras that are currently inactive also be found?</param>
    /// <returns>list of all found cameras</returns>
    private static Camera[] getWebCamOnly(bool includeInactive = false)
    {
        var cams = SearchHelper.FindSceneObjectsOfTypeAll<Camera>(includeInactive);
        var cam = cams.Where(x => x.gameObject.tag == "WebCam").ToArray();
        return cam;
    }

    /// <summary>
    /// get the web camera in the scene without fallback camera for AR mode
    /// </summary>
    private static Camera WebCamOnly
    {
        get
        {
            return getWebCamOnly().FirstOrDefault();
        }
    }

    /// <summary>
    /// currently active camera, depending on whether AR or non-AR mode is active
    /// </summary>
    public static Camera ActiveARModeCamera
    {
        get
        {
            if (StatusProperties.Values.ARActive)
                return ARCamera;
            else
                return WebCam;
        }
    }

    /// <summary>
    /// get a list of all video streaming cameras in the scene
    /// </summary>
    public static Camera[] VideoStreamingCameras
    {
        get
        {
            var cams = getWebCamOnly(false).ToList();
            if (ARCameraOnly != null)
                cams.Add(ARCameraOnly);
            if (CaptureCameraOnly != null)
                cams.Add(CaptureCameraOnly);
            return cams.ToArray();
        }
    }

    /// <summary>
    /// get a fix anchor point position in a certain distance from the camera. Used in low bandwidth mode.
    /// </summary>
    public static Vector3 ARCameraStaticAnchorPosition
    {
        get
        {
            var cam = ARCamera;
            var pos = cam.transform.position;

            if (cam.transform.childCount > 0)
                pos = cam.transform.GetChild(0).position;
            else
                pos += cam.transform.forward.normalized * 0.15f;

            return pos;
        }
    }

    /// <summary>
    /// Get the camera with captures the display screen to send it to other devices or save the video stream.
    /// Use the main camera as fallback camera when no capture cam is found.
    /// </summary>
    public static Camera CaptureCamera
    {
        get
        {
            Camera cam = CaptureCameraOnly;
            if (!cam) cam = MainCamera;
            return cam;
        }
    }

    /// <summary>
    /// Get the camera with captures the display screen without fallback camera.
    /// </summary>
    private static Camera CaptureCameraOnly
    {
        get
        {
            var cams = SearchHelper.FindSceneObjectsOfTypeAll<Camera>();
            Camera cam = cams.FirstOrDefault(x => x.GetComponent<CameraCapture>() != null);
            return cam;
        }
    }

    /// <summary>
    /// get the main camera with displays what the user will finally see
    /// </summary>
    public static Camera MainCamera
    {
        get
        {
            Camera cam = Camera.main;
            if (!cam)
            {
                var cams = SearchHelper.FindSceneObjectsOfTypeAll<Camera>();
                cam = cams.FirstOrDefault(x => x.targetTexture == null);
            }
            return cam;
        }
    }

    /// <summary>
    /// calculate the scale factor which is changed by the distance between the camera and anchor point position
    /// </summary>
    /// <param name="distance">distance between AR camera and anchor point position</param>
    /// <param name="DefaultScaleValue">initial scale factor of the game object</param>
    /// <returns></returns>
    public static Vector3 getDistanceScaleFactor(float distance, Vector3 DefaultScaleValue)
    {
        var frustumHeight = 2.0f * distance * Mathf.Tan(CameraHelper.ActiveARModeCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        var scaleDistanceFactor = frustumHeight * DefaultScaleValue.x;

        return Vector3.one * scaleDistanceFactor;
    }

    /// <summary>
    /// calculate the scale factor which is changed by the distance between the camera and anchor point position
    /// </summary>
    /// <param name="camTransform">camera position</param>
    /// <param name="objPos">anchor point position</param>
    /// <param name="DefaultScaleValue">initial scale factor of the game object</param>
    /// <returns></returns>
    public static Vector3 getDistanceScaleFactor(Transform camTransform, Vector3 objPos, Vector3 DefaultScaleValue)
    {
        var distance = getDistance(camTransform, objPos);
        return getDistanceScaleFactor(distance, DefaultScaleValue);
    }

    /// <summary>
    /// calculate the distance between the camera and anchor point position
    /// </summary>
    /// <param name="camTransform">camera position</param>
    /// <param name="objPos">anchor point position</param>
    /// <returns></returns>
    public static float getDistance(Transform camTransform, Vector3 objPos)
    {
        Plane plane = new Plane();
        plane.SetNormalAndPosition(camTransform.forward, camTransform.position);
        var distance = plane.GetDistanceToPoint(objPos);
        return distance;
    }

    /// <summary>
    /// exchange the render texture at runtime for all cameras and RawImage currently using this render texture
    /// </summary>
    /// <param name="oldTexture">old render texture to be replaced</param>
    /// <param name="newTexture">new render texture</param>
    public static void UpdateTargetTexture(RenderTexture oldTexture, RenderTexture newTexture)
    {
        var cams = SearchHelper.FindSceneObjectsOfTypeAll<Camera>().Where(x => x.targetTexture == oldTexture);

        foreach (var cam in cams)
        {
            cam.targetTexture = newTexture;
        }

        var list = SearchHelper.FindSceneObjectsOfTypeAll<RawImage>();
        foreach (var item in list)
        {
            if (item.texture == oldTexture)
                item.texture = newTexture;
        }
    }
}
