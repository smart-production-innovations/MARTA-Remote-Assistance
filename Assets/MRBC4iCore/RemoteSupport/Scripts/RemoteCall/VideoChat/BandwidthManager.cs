using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// Manages the bandwidth properties. Adapt resolution, fps and webrtc sending mode (constant video stream or low bandwidth single image frames) depending from the connection bandwidth.
/// </summary>
public class BandwidthManager : AManager<BandwidthManager>
{
    #region properties
    public GameObject Properties;
    public GameObject qualitySettings, fpsSettings;

    public ToggleGroup qualityGroup, fpsGroup;
    public Dropdown supportModeDD;

    public Button takePictureButton;
    public SnackbarController snackbarInfoLowBandwidth;
    public SnackbarController snackbarInfoVideostream;

    private SupportModeType supportModeType = SupportModeType.Videostream;
    /// <summary>
    /// gets the video send mode (constant video stream or low bandwidth single image frames). 
    /// </summary>
    public SupportModeType SupportModeType
    {
        get
        {
            var cam = SearchHelper.FindSceneObjectOfType<CameraCapture>();
            if (cam)
            {
                if (cam.LowBandwidthMode)
                    return SupportModeType.LowBandwidthMode;
            }
            return supportModeType;
        }
    }

    /// <summary>
    /// Is the AR or non-AR mode active?
    /// </summary>
    public bool ARMode
    {
        get { return StatusProperties.Values.ARActive; }
        set
        {
            StatusProperties.Values.ARActive = value;
        }
    }

    #endregion

    #region video quality
    /// <summary>
    /// set the image resolution for the texture which is send via webrtc
    /// </summary>
    /// <param name="longSide">edge length of the longer side of the video resolution</param>
    public void SetImageQuality(int longSide)
    {
        setBandwithOptions(quality: longSide);
    }

    /// <summary>
    /// set the image resolution for the texture which is send via webrtc
    /// </summary>
    /// <param name="autoQuality">true: automatic video resolution adaption is on</param>
    public void SetImageQualityAuto(bool autoQuality)
    {
        if (autoQuality)
            setBandwithOptions(quality: 0);
    }

    /// <summary>
    /// set the fps for the video signal which is send via webrtc
    /// </summary>
    /// <param name="fps">frames per second</param>
    public void SetFPS(int fps)
    {
        setBandwithOptions(fps: fps);
    }

    /// <summary>
    /// set the fps for the video signal which is send via webrtc
    /// </summary>
    /// <param name="autoFPS">true: automatic video fps adaption is on</param>
    public void SetFPSAuto(bool autoFPS)
    {
        if (autoFPS)
            setBandwithOptions(fps: 0);
    }

    /// <summary>
    /// sets the video send mode (constant video stream or low bandwidth single image frames). 
    /// </summary>
    /// <param name="mode"></param>
    public void SupportMode(int mode)
    {
        setBandwithOptions(supportType: mode);
    }

    /// <summary>
    /// change bandwidth options to the given values
    /// </summary>
    /// <param name="quality">resolution of the texture to be send via webrtc</param>
    /// <param name="fps">fps of the video signal to be send via webrtc</param>
    /// <param name="supportType">video send mode: videostream (0) or lowbandwidhtmode (1)</param>
    /// <param name="sendCommand">inform the other devices of the status change</param>
    private void setBandwithOptions(int quality = -1, int fps = -1, int supportType = -1, bool sendCommand = true)
    {
        var cam = SearchHelper.FindSceneObjectOfType<CameraCapture>();
        // Adjusting the bandwidth settings on the mobile on-site device
        if (cam)
        {
            // change the resolution of the texture to be send via webrtc
            if (quality > 0)
            {
                cam.setSenderSize(quality);
                cam.AutoQuality = false;
            }
            else if (quality == 0)
            {
                cam.AutoQuality = true;
            }

            // change the fps of the video signal to be send via webrtc
            if (fps > 0)
            {
                cam.FPS = fps;
                cam.AutoFPS = false;
            }
            else if (fps == 0)
            {
                cam.AutoFPS = true;
            }

            // change between single image and live video stream transmission
            if (Enum.GetValues(typeof(SupportModeType)).Cast<int>().Any(x => x == supportType))
            {
                bool lowBandwidthMode = (supportType == (int)SupportModeType.LowBandwidthMode);
                cam.LowBandwidthMode = lowBandwidthMode;

                if (takePictureButton) takePictureButton.gameObject.SetActive(lowBandwidthMode);
                if (snackbarInfoLowBandwidth) snackbarInfoLowBandwidth.gameObject.SetActive(lowBandwidthMode);
                if (snackbarInfoVideostream) snackbarInfoVideostream.gameObject.SetActive(!lowBandwidthMode);
            }
        }

        // apply the status configuration to all associated UI elements
        if (Enum.IsDefined(typeof(SupportModeType), supportType))
        {
            var newType = (SupportModeType)supportType;
            if (supportModeType != newType)
            {
                supportModeType = newType;
                // apply the status configuration to all associated UI elements
                ToolProperties.SetAllItemsActive(true);
            }
        }

        // accept new resolution in the active drawing process
        if (Enum.GetValues(typeof(SupportModeType)).Cast<int>().Any(x => x == supportType))
        {
            bool lowBandwidthMode = (supportType == (int)SupportModeType.LowBandwidthMode);

            if (fpsSettings) fpsSettings.SetActive(!lowBandwidthMode);

            if (!lowBandwidthMode && DrawingManager.HasInstance)
                DrawingManager.InterfaceInstance.DrawingActive = DrawingManager.InterfaceInstance.DrawingActive;
        }

        // inform the other devices of the status change
        if (sendCommand)
            CommunicationManager.Instance.SendCommandMsg(new CommandMsg(CommandMsgType.BandwidthOptions, Commands.getBandwidthOptionsString(quality, fps, supportType)));
        else
        {
            if (qualityGroup) qualityGroup.SetActiveToggle(quality);
            if (fpsGroup) fpsGroup.SetActiveToggle(fps);
            if (supportModeDD) supportModeDD.value = supportType;
        }
    }

    /// <summary>
    /// change bandwidth options to the given values
    /// </summary>
    /// <param name="param">; separated bandwidth options. possible parameters Quality=;FPS=;Mode=</param>
    public void setBandwithOptions(string param)
    {
        int quality = -1, fps = -1, supportType = -1;
        Commands.parseBandwidthOptionsString(param, out quality, out fps, out supportType);
        setBandwithOptions(quality, fps, supportType, false);
    }

    /// <summary>
    /// In low bandwidth mode instance photo button was pressed. Send the current video frame for annotating to the other device.
    /// </summary>
    public void SendScreenshot()
    {
        var cam = GameObject.FindObjectOfType<CameraCapture>();
        if (cam)
        {
            cam.SendScreenshot();
        }
    }
    #endregion
}
