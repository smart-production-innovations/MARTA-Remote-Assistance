using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MousePositionCalculation
{
    /// <summary>
    /// How long should the mouse pointer be displayed in the video after releasing the mouse?
    /// </summary>
    public static float MaxDisplayTime
    {
        get
        {
            // In AR mode the mouse pointer can be displayed longer, because the position is anchored in the 3d.
            if (StatusProperties.Values.ARActive)
                return 15f;
            else
                return 1.5f;
        }
    }

    public static void displayPointer(GameObject MousePosition3DMarker, Vector3 newPosition, Quaternion newRotation)
    {
        var MousePosition3DMarkerParticle = MousePosition3DMarker.GetComponent<ParticleAnnotationContainer>();
        if (!MousePosition3DMarkerParticle)
        {
            MousePosition3DMarker.transform.position = newPosition;
            MousePosition3DMarker.transform.rotation = newRotation;
            MousePosition3DMarker.SetActive(true);
            scalePointer(MousePosition3DMarker);
        }
        else
        {
            MousePosition3DMarkerParticle.showParticleAnnotationType(newPosition, newRotation);
        }
    }

    public static void hidePointer(GameObject MousePosition3DMarker, bool immediate = true)
    {
        var MousePosition3DMarkerParticle = MousePosition3DMarker.GetComponent<ParticleAnnotationContainer>();
        if (!MousePosition3DMarkerParticle)
        {
            if (immediate) MousePosition3DMarker.SetActive(false);
        }
        else
        {
            MousePosition3DMarkerParticle.FadeOutAnimation();
        }
    }

    public static void updatePointer(GameObject MousePosition3DMarker, float deltaTime)
    {
        var MousePosition3DMarkerParticle = MousePosition3DMarker.GetComponent<ParticleAnnotationContainer>();
        if (!MousePosition3DMarkerParticle)
        {
            // Check the validity of the mouse pointer visualization.
            if (MousePosition3DMarker && MousePosition3DMarker.activeSelf)
            {
                if (deltaTime > MousePositionCalculation.MaxDisplayTime)
                {
                    hidePointer(MousePosition3DMarker);
                }
            }
        }
    }

    public static void setColor(GameObject MousePosition3DMarker, Color color)
    {
        var MousePosition3DMarkerParticle = MousePosition3DMarker.GetComponent<ParticleAnnotationContainer>();
        if (MousePosition3DMarkerParticle)
            MousePosition3DMarkerParticle.SetColor(color);
    }

    public static void setType(GameObject MousePosition3DMarker, ParticleAnnotationType annotationType)
    {
        var MousePosition3DMarkerParticle = MousePosition3DMarker.GetComponent<ParticleAnnotationContainer>();
        if (MousePosition3DMarkerParticle)
            MousePosition3DMarkerParticle.setParticleAnnotationType(annotationType);
    }

    public static void clear(GameObject MousePosition3DMarker)
    {
        var particleAnnotations = SearchHelper.FindSceneObjectsOfTypeAll<ParticleAnnotationContainer>();
        foreach (var particleAnnotation in particleAnnotations)
        {
            particleAnnotation.clearAll();
        }
    }

    public static Vector3 getScaleFactor(Transform MousePosition3DMarker)
    {
        return CameraHelper.getDistanceScaleFactor(CameraHelper.ActiveARModeCamera.transform, MousePosition3DMarker.position, Vector3.one);
    }

    public static void scalePointer(GameObject MousePosition3DMarker)
    {
        var MousePosition3DMarkerParticle = MousePosition3DMarker.GetComponent<ParticleAnnotationContainer>();
        var size = getScaleFactor(MousePosition3DMarker.transform);
        if (MousePosition3DMarkerParticle)
        {
            MousePosition3DMarkerParticle.setSize(size.x);
        }
        else
        {
            MousePosition3DMarker.transform.localScale = size;
        }
    }

    public static float setNewPointerPosition(GameObject MousePosition3DMarker, Vector2 viewPointCoord, float fallBackDistance = 0)
    {
        if (MousePosition3DMarker)
        {
            var coord = viewPointCoord * new Vector2(Screen.width, Screen.height);
            Vector2Int coordinates = new Vector2Int((int)coord.x, (int)coord.y);

            if (viewPointCoord.x >= 0 && viewPointCoord.y >= 0 && viewPointCoord.x <= 1 && viewPointCoord.y <= 1)
            {
                Camera cam = CameraHelper.ActiveARModeCamera;
                if (fallBackDistance <= 0) fallBackDistance = (cam.nearClipPlane * 10);

                Pose pose;
                float distance = fallBackDistance;
                Transform plane;
                Vector3 position;
                Quaternion rotation;

                // calculate 3d position for the 2d screen coordinates
                if (StatusProperties.Values.ARActive && AnchorPointManager.Instance.TryGetPoseAndDistance(coordinates.x, coordinates.y, out pose, out plane, out distance))
                {

                    // 3d features in AR mode found
                    position = pose.position;
                    rotation = pose.rotation;
                    fallBackDistance = distance;
                    if (fallBackDistance >= cam.farClipPlane) fallBackDistance = cam.farClipPlane - 0.1f;
                    if (fallBackDistance <= cam.nearClipPlane) fallBackDistance = cam.nearClipPlane + 0.1f;
                }
                else
                {
                    // no 3d features found or non AR mode is active
                    var ray = cam.ViewportPointToRay(viewPointCoord);
                    position = ray.origin + ray.direction * fallBackDistance;
                    rotation = cam.transform.rotation;
                }

                // set the mouse pointer
                displayPointer(MousePosition3DMarker, position, rotation);
                return fallBackDistance;
            }
        }
        return -1;
    }
}

/// <summary>
/// Convert mouse position for expert mouse pointer visualization on the client device
/// </summary>
public class MousePositionConverter3DClient : MousePositionConverter3D
{
    public GameObject MousePosition3DMarker;
    private float showMousePosition3DMarkerTime = 0;
    private float fallBackDistance = 0;


    private ParticleAnnotationContainer mousePosition3DMarkerParticle;
    protected ParticleAnnotationContainer MousePosition3DMarkerParticle
    {
        get
        {
            if (!mousePosition3DMarkerParticle)
                mousePosition3DMarkerParticle = MousePosition3DMarker.GetComponent<ParticleAnnotationContainer>();
            return mousePosition3DMarkerParticle;
        }
    }

    protected override void hidePointer()
    {
        if (MousePosition3DMarkerParticle)
        {
            MousePosition3DMarkerParticle.FadeOutAnimation();
        }
    }

    protected override void updatePointer()
    {
        MousePositionCalculation.updatePointer(MousePosition3DMarker, Time.time - showMousePosition3DMarkerTime);
    }

    protected override void setNewPointerPosition(Vector2 viewPointCoord)
    {
        var distance = MousePositionCalculation.setNewPointerPosition(MousePosition3DMarker, viewPointCoord, fallBackDistance);
        if (distance > 0)
        {
            showMousePosition3DMarkerTime = Time.time;
            fallBackDistance = distance;
        }
    }
}
