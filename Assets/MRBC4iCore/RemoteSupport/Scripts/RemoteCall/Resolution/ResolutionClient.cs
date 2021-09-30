using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// the mobile device on site determines the resolution of the video to be transmitted
/// </summary>
public class ResolutionClient : ResolutionManager
{
    public int maxResolution = 640;
    public RenderTexture[] videoStreamingTextures;

    private int screenWidth;
    private int screenHeight;

    private void OnDestroy()
    {
        DeviceChange.RemoveListener(OnResolutionChange, OnOrientationChange);
    }

    protected override void Awake()
    {
        base.Awake();
        DeviceChange.AddListener(OnResolutionChange, OnOrientationChange);
    }

    /// <summary>
    /// List of video stream RenderTextures to be monitored.
    /// </summary>
    private RenderTexture[] VideoStreamingTextures
    {
        get
        {
            if (videoStreamingTextures == null || videoStreamingTextures.Length == 0)
            {
                // All virtual cameras that capture a video stream to be transmitted are monitored.
                var cams = CameraHelper.VideoStreamingCameras;
                var textures = new List<RenderTexture>();
                foreach (var cam in cams)
                {
                    if (!textures.Contains(cam.targetTexture))
                        textures.Add(cam.targetTexture);
                }
                videoStreamingTextures = textures.ToArray();
            }
            return videoStreamingTextures;
        }
    }

    private void Start()
    {
        calcResolution();
    }

    /// <summary>
    /// Check for an Orientation Change
    /// </summary>
    /// <param name="orientation">new device orientation</param>
    void OnOrientationChange(DeviceOrientation orientation)
    {
        calcResolution();
    }

    /// <summary>
    /// Check for a Resolution Change
    /// </summary>
    /// <param name="resolution">new resolution</param>
    void OnResolutionChange(Vector2 resolution)
    {
        calcResolution();
    }

    /// <summary>
    /// Adjust RenderTexture resolution on all cameras that render into the RenderTexture, as well as on all RawImages that display the RenderTexture.
    /// </summary>
    private void calcResolution()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        int res = Mathf.Max(screenWidth, screenHeight);

        // calculate new resolution
        if (res > maxResolution)
        {
            lWidth = Mathf.RoundToInt(screenWidth * (float)maxResolution / res);
            lHeight = Mathf.RoundToInt(screenHeight * (float)maxResolution / res);
        }
        else
        {
            lWidth = screenWidth;
            lHeight = screenHeight;
        }

        // Adjust RenderTexture resolution on all cameras that render into the RenderTexture, as well as on all RawImages that display the RenderTexture.
        for (int i = 0; i < VideoStreamingTextures.Length; i++)
        {
            var oldResTexture = VideoStreamingTextures[i];
            var newResTexture = RenderTexture.GetTemporary(lWidth, lHeight);
            newResTexture.name = oldResTexture.name;
            CameraHelper.UpdateTargetTexture(oldResTexture, newResTexture);
            VideoStreamingTextures[i] = newResTexture;
            oldResTexture.Release();
        }

        ChangeLResolution(lWidth, lHeight);
    }
}
