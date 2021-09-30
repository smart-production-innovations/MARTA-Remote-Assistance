using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FieldOfView : MonoBehaviour
{
    public Transform distanceReferenceMeasuringObject;
    private RenderTexture texture;

    public RenderTexture Texture
    {
        get
        {
            if (!texture)
                texture = FieldOfViewCamera.targetTexture;
            return texture;
        }
    }

    protected Transform DistanceReferenceMeasuringObject
    {
        get
        {
            if (!distanceReferenceMeasuringObject && transform.childCount > 0)
                distanceReferenceMeasuringObject = transform.GetChild(0);
            return distanceReferenceMeasuringObject;
        }
    }

    protected Camera FieldOfViewCamera
    {
        get
        {
            return GetComponent<Camera>();
        }
    }

    private void Start()
    {
        DistanceReferenceMeasuringObject.GetComponent<Renderer>().material.color = Color.red;
    }

    private bool coroutineRunning = false;
    private Texture2D imageFrame;
    private Color lastPixelColor = Color.black;
    private float distanceDelta = -1f;
    private float sensorDimensionReference = 0.501f;
    private int loopCount = 0;
    private int maxLoopCount = 5;
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!coroutineRunning)
        {
            StartCoroutine(CoroutineAdjustReferenceDistance(source));
        }
        Graphics.Blit(source, destination);
    }

    private bool lastWasOutside = false;
    public IEnumerator CoroutineAdjustReferenceDistance(RenderTexture source)
    {
        coroutineRunning = true;
        var isFirstTime = (imageFrame == null);
        if (!imageFrame)
            imageFrame  = new Texture2D(source.width, source.height, TextureFormat.RGB24, false);

        imageFrame.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        imageFrame.Apply();
        var currentColor = imageFrame.GetPixel(source.width / 2, 0);

        if (isFirstTime)
        {
            lastPixelColor = imageFrame.GetPixel(0, 0);
            if (currentColor != lastPixelColor)
            {
                distanceDelta = -distanceDelta;
                lastPixelColor = currentColor;
            }
        }

        if (lastPixelColor == currentColor)
        {
            var referencePosition = DistanceReferenceMeasuringObject.localPosition;
            referencePosition.z += distanceDelta;
            var isOutside = false;
            if (referencePosition.z < FieldOfViewCamera.nearClipPlane)
            {
                referencePosition.z = FieldOfViewCamera.nearClipPlane;
                isOutside = true;
            }
            if (referencePosition.z > FieldOfViewCamera.farClipPlane)
            {
                referencePosition.z = FieldOfViewCamera.farClipPlane;
                isOutside = true;
            }
            DistanceReferenceMeasuringObject.localPosition = referencePosition;

            if (lastWasOutside && isOutside)
            {
                calculateFieldOfView();
            }
            lastWasOutside = isOutside;
        }
        else
        {
            if (loopCount < maxLoopCount)
            {
                loopCount++;
                distanceDelta = (-distanceDelta / 10f);
            }
            else
            {
                calculateFieldOfView();
            }
        }

        lastPixelColor = currentColor;
        coroutineRunning = false;
        yield return null;
    }

    private void calculateFieldOfView()
    {
        var FoV = (2 * Mathf.Atan(sensorDimensionReference / DistanceReferenceMeasuringObject.localPosition.z)) * 180 / Mathf.PI;
        FoV = (float)System.Math.Round((double)FoV, 2);

        DistanceReferenceMeasuringObject.gameObject.SetActive(false);

        var cameras = SearchHelper.FindSceneObjectsOfTypeAll<Camera>(true);
        foreach (var cam in cameras)
        {
            cam.fieldOfView = FoV;
        }

        StatusProperties.Values.FieldOfView = FoV;
        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.ARFieldOfView, Commands.getFloatString(FoV)));
        enabled = false;
    }
}
