using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>
/// Defines the video stream generation in non AR mode. In this case, the video signal is taken directly from the webcam without AR calculation.
/// </summary>
[RequireComponent(typeof(Camera))]
public class WebCam : MonoBehaviour
{
    private Material backgroundMaterial;
    /// <summary>
    /// Webcam render material. Material witch allows resolution and device orientation adaption.
    /// </summary>
    private Material BackgroundMaterial
    {
        get
        {
            if (!backgroundMaterial)
            {
                //Edit -> Project Settings -> Graphics: add shader "Hidden/WebcamBackground" to the "Always Included Shaders" list. 
                var shader = Shader.Find("Hidden/WebcamBackground");
                if (shader)
                    backgroundMaterial = new Material(shader);
            }
            return backgroundMaterial;
        }
    }

    private WebCamTexture webcamTexture;
    private Camera cam;

    /// <summary>
    /// virtual unity webcam camera
    /// </summary>
    private Camera Cam
    {
        get
        {
            if (!cam)
            {
                cam = GetComponent<Camera>();
            }
            return cam;
        }
    }

    /// <summary>
    /// Render the real webcam stream as render output of the virtual unity webcam camera. 
    /// Adjusts the real webcam resolution to the resolution of the device display to match the resolution of the AR framework video.
    /// Adjust the rotation of the real webcam to the device orientation.
    /// </summary>
    private void setCommandBuffer()
    {
        if (!webcamTexture)
        {
            webcamTexture = new WebCamTexture();
            webcamTexture.Play();
            CommandBuffer commandBuffer = new CommandBuffer();
            // The webcam stream will be passed to the "_MainTex" property of the BackgroundMaterial.
            // The BackgroundMaterial allows resolution and device orientation adaption.
            commandBuffer.Blit(webcamTexture, BuiltinRenderTextureType.CurrentActive, BackgroundMaterial);
            Cam.RemoveAllCommandBuffers();
            // Render the real webcam stream as render output of the virtual unity webcam camera. 
            Cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
        }
        else
        {
            webcamTexture.Play();
        }
    }

    private void OnDestroy()
    {
        DeviceChange.RemoveListener(OnResolutionChange, OnOrientationChange);
    }

    private void Awake()
    {
        DeviceChange.AddListener(OnResolutionChange, OnOrientationChange);
    }

    private void OnEnable()
    {
        setCommandBuffer();
        syncAspectRation();
    }

    private void OnDisable()
    {
        if (webcamTexture)
            webcamTexture.Stop();
    }

    int count = 0;
    /// <summary>
    /// calculate the rotation angle of the texture according to the device resolution
    /// </summary>
    private float RotationAngle
    {
        get
        {
            float rotationAngle = 0;
            switch (Input.deviceOrientation)
            {
                case DeviceOrientation.Unknown:
                    break;
                case DeviceOrientation.Portrait:
                    rotationAngle = 90;
                    break;
                case DeviceOrientation.PortraitUpsideDown:
                    rotationAngle = 270;
                    break;
                case DeviceOrientation.LandscapeLeft:
                    rotationAngle = 0;
                    break;
                case DeviceOrientation.LandscapeRight:
                    rotationAngle = 180;
                    break;
                case DeviceOrientation.FaceUp:
                    break;
                case DeviceOrientation.FaceDown:
                    break;
                default:
                    break;
            }
            return rotationAngle;
        }
    }

    /// <summary>
    /// Adjust the real webcam resolution to the resolution of the device display to match the resolution of the AR framework video.
    /// Adjust the rotation of the real webcam to the device orientation.
    /// </summary>
    private void syncAspectRation()
    {
        var rotationAngle = (int)RotationAngle;
        if (rotationAngle == 0 || rotationAngle == 180)
            BackgroundMaterial.SetInt("_rotationAngle", rotationAngle);

        if (webcamTexture) 
        {
            float screenAspect = Screen.width / (float)Screen.height;
            float videoAspect = webcamTexture.width / (float)webcamTexture.height;

            BackgroundMaterial.SetFloat("_scaleX", 1.0f);
            BackgroundMaterial.SetFloat("_scaleY", 1 / videoAspect);
        }
    }

    /// <summary>
    /// adapt the BackgroundMaterial setting if the resolution changed
    /// </summary>
    private void OnRectTransformDimensionsChange()
    {
        syncAspectRation();
    }

    /// <summary>
    /// adapt the BackgroundMaterial setting if the device orientation changed
    /// </summary>
    /// <param name="orientation">device orientation</param>
    void OnOrientationChange(DeviceOrientation orientation)
    {
        syncAspectRation();
    }

    /// <summary>
    /// adapt the BackgroundMaterial setting if the resolution changed
    /// </summary>
    /// <param name="resolution">target resolution</param>
    void OnResolutionChange(Vector2 resolution)
    {
        syncAspectRation();
    }
}
