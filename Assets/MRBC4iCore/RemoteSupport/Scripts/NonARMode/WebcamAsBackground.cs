using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Render the webcam video to the background of the camera attached to this object
/// Also rotates the webcam images to fit the device orientation
/// </summary>
[RequireComponent(typeof(Camera))]
public class WebcamAsBackground : MonoBehaviour
{
    public int requestedWidth = 1280;
    public int requestedHeight = 720;

    public Material backgroundMaterial;

    private WebCamTexture wcTexture;
    private int rotationAngle = 0;


    private void Awake()
    {
        // Render the real webcam stream as render output of the virtual unity webcam camera. 
        // Adjusts the real webcam resolution to the resolution of the device display to match the resolution of the AR framework video.
        // Adjust the rotation of the real webcam to the device orientation.
        wcTexture = new WebCamTexture(requestedWidth, requestedHeight);
        wcTexture.Play();

        Camera cam = GetComponent<Camera>();
        // The webcam stream will be passed to the "_MainTex" property of the BackgroundMaterial.
        // The BackgroundMaterial allows resolution and device orientation adaption.
        CommandBuffer commandBuffer = new CommandBuffer();
        commandBuffer.Blit(wcTexture, BuiltinRenderTextureType.CurrentActive, backgroundMaterial);
        // Render the real webcam stream as render output of the virtual unity webcam camera. 
        cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
    }

    private void Start()
    {
        // Adjust the real webcam resolution to the resolution of the device display to match the resolution of the AR framework video.
        int width = ResolutionManager.Instance.lWidth;
        int height = ResolutionManager.Instance.lHeight;
        if (width > 0 && height > 0)
        {
            UpdateAspectRatio(width, height);
        }
        ResolutionManager.OnLResolutionChanged += UpdateAspectRatio;
    }

    private void Update()
    {
        //Adjust the rotation of the real webcam to the device orientation.
        if (rotationAngle != wcTexture.videoRotationAngle)
        {
            rotationAngle = wcTexture.videoRotationAngle;
            backgroundMaterial.SetInt("_rotationAngle", rotationAngle);

            if (rotationAngle == 90 || rotationAngle == 270)
            {
                UpdateAspectRatio(ResolutionManager.Instance.lHeight, ResolutionManager.Instance.lWidth);
            }
            else
            {
                UpdateAspectRatio(ResolutionManager.Instance.lWidth, ResolutionManager.Instance.lHeight);
            }
        }
    }

    private void OnDestroy()
    {
        ResolutionManager.OnLResolutionChanged -= UpdateAspectRatio;
    }

    /// <summary>
    /// Adjust the real webcam resolution to the resolution of the device display to match the resolution of the AR framework video.
    /// Adjust the rotation of the real webcam to the device orientation.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void UpdateAspectRatio(int width, int height)
    {
        float screenAspect = width / (float)height;
        float videoAspect = wcTexture.width / (float)wcTexture.height;

        if (screenAspect < videoAspect)
        {
            float scaleX = videoAspect / screenAspect;
            backgroundMaterial.SetFloat("_scaleX", 1 / scaleX);
            backgroundMaterial.SetFloat("_scaleY", 1.0f);
        }
        else
        {
            float scaleY = screenAspect / videoAspect;
            backgroundMaterial.SetFloat("_scaleX", 1.0f);
            backgroundMaterial.SetFloat("_scaleY", 1 / scaleY);
        }
    }
}
