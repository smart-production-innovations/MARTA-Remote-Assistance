using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

/// <summary>
/// video send mode
/// constant video stream or low bandwidth single image frames
/// </summary>
public enum SupportModeType
{
    Videostream,
    LowBandwidthMode
}

/// <summary>
/// Status bar call toggle property setting
/// </summary>
public class CallSettings : AManager<CallSettings>
{
    #region properties
    public bool isServer;

    /// <summary>
    /// Toggle to switch the microphone on / off. 
    /// </summary>
    public Toggle uMuteToggle;

    /// <summary>
    /// Toggle to switch the loudspeakers on / off. Only for mobile visible.
    /// </summary>
    public Toggle uLoudspeakerToggle;

    /// <summary>
    /// Slider to just the remote users volume.
    /// </summary>
    public Slider uVolumeSlider;

    public Toggle uChatBoxToggle;

    public Toggle uGalleryButton;

    public SnackbarController uGallerySnackbar;

    /// <summary>
    /// Shutdown button. Disconnects all connections + shuts down the server if started.
    /// </summary>
    public Button uShutdownButton; //not used

    public Color activeColor;
    public Color inactiveColor;

    private float lastActiveVolume = 1;
    #endregion

    #region unity loop
    protected override void Awake()
    {
        base.Awake();

        // set the graphical feedback color for the UI elements
        var toggleUI = GetComponentsInChildren<ToggleUI>(true);
        foreach (var item in toggleUI)
        {
            if (item.activeUI) item.UIColor = activeColor;
            else item.UIColor = inactiveColor;
        }
    }

    protected virtual void Start()
    {
        // set initial values
        OnLoudspeakerToggle();

        if (uChatBoxToggle) OnChatBoxToggle(uChatBoxToggle.isOn);
        OnMuteToggle();
        if (uVolumeSlider && uVolumeSlider.isActiveAndEnabled)
            OnVolumeChanged(uVolumeSlider.value);
    }

    protected virtual void Update()
    {
        //work around for the loudspeaker button on mobile devices
        //loudspeaker state might be changed globally via
        //different APIs or multiple parallel callapps
        //we refresh the button every few frames to make sure it
        //shows the correct icon as this has confused users in the past
        if (uLoudspeakerToggle != null && uLoudspeakerToggle.IsActive())
        {
            if (Time.frameCount % 30 == 0)
                RefreshLoudspeakerToggle();
        }
    }
    #endregion

    #region GUI
    /// <summary>
    /// set audio call volume
    /// </summary>
    /// <param name="value">audio volume</param>
    public void OnVolumeChanged(float value)
    {
        CallAppBackend.Instance.SetRemoteVolume(value);
        if (value > 0) lastActiveVolume = value;
        RefreshVolumeChange(value);
    }

    /// <summary>
    /// synchronize speaker volume with back end property
    /// </summary>
    /// <param name="value"></param>
    private void RefreshVolumeChange(float value)
    {
        if (uVolumeSlider) uVolumeSlider.value = value;
        if (uLoudspeakerToggle) uLoudspeakerToggle.isOn = (value == 0); 
    }

    /// <summary>
    /// toggle speaker on or off
    /// </summary>
    public void OnLoudspeakerToggle()
    {
        if (uLoudspeakerToggle)
        {
            //watch out the on state of the toggle means
            //the icon is crossed out thus
            //isOn == true means the speaker is off
            bool state = !uLoudspeakerToggle.isOn;
            SetLoudspeaker(state);

            //read if the state actually changed
            RefreshLoudspeakerToggle();
        }
    }

    public void SetLoudspeaker(bool state)
    {
        float volume = (state ? lastActiveVolume : 0);

        if (Application.isMobilePlatform)
        {
            CallAppBackend.Instance.SetLoudspeakerStatus(state);
        }
        else
        {
            OnVolumeChanged(volume);
        }
    }

    /// <summary>
    /// synchronize speaker toggle state with back end property
    /// </summary>
    private void RefreshLoudspeakerToggle()
    {
        if (uLoudspeakerToggle && Application.isMobilePlatform)
        {
            bool state = CallAppBackend.Instance.GetLoudspeakerStatus();
            uLoudspeakerToggle.isOn = !state;
        }
    }

    /// <summary>
    /// toggle microphone on or off
    /// </summary>
    public void OnMuteToggle()
    {
        if (uMuteToggle)
        {
            bool state = uMuteToggle.isOn;
            SetMute(state);
            //read if the state actually changed
            RefreshMuteToggle();
        }
    }

    public void SetMute(bool state)
    {
        CallAppBackend.Instance.SetMute(state);

        if (!state)
        {
#if PLATFORM_ANDROID
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
            {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
            }
#endif
        }
    }

    /// <summary>
    /// synchronize microphone toggle state with back end property
    /// </summary>
    private void RefreshMuteToggle()
    {
        if (uMuteToggle)
        {
            bool state = CallAppBackend.Instance.IsMute();
            uMuteToggle.isOn = state;
        }
    }

    /// <summary>
    /// toggle chat box on or off
    /// </summary>
    public void OnChatBoxToggle(bool state)
    {
        ChatManager.Instance.ChatBoxActive = state;
    }

    /// <summary>
    /// toggle gallery on or off
    /// </summary>
    /// <param name="isOn">new toggle value</param>
    /// <param name="skipPreview">Should the overview display be skipped? true: open detail view; false: open overview view</param>
    public void OnAnchorGalleryToggle(bool isOn, bool skipPreview)
    {
        OnAnchorGalleryToggle(isOn);

        if (isOn && skipPreview)
        {
            AnchorGalleryOverviewManager.Instance.gameObject.SetActive(false);
            AnchorGalleryDetailManager.Instance.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// toggle gallery on or off
    /// </summary>
    /// <param name="isOn">new toggle value</param>
    public void OnAnchorGalleryToggle(bool isOn)
    {
        LiveviewItem.SetAllItemsActive(!isOn);
        AnchorGalleryOverviewManager.Instance.gameObject.SetActive(isOn);

        if (!isOn)
            AnchorGalleryDetailManager.Instance.gameObject.SetActive(isOn);

        if (isServer)
        {
            if (isOn)
            {
                EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.CallForAnchorData, ""));
            }
            else
            {
                DrawingRemoteManager.Instance.CancelDrawing();
            }
        }
        else
        {
            if (AnchorPointManager.Instance.GeAnchorCount() == 0)
            {
                LeaveGallery(true);
            }
        }
    }

    /// <summary>
    /// Is the gallery currently active?
    /// </summary>
    public bool IsGalleryActive
    {
        get
        {
            if (uGalleryButton)
                return uGalleryButton.isOn;
            return false;
        }
    }

    /// <summary>
    /// leave gallery
    /// </summary>
    /// <param name="noEntries"></param>
    public void LeaveGallery(bool noEntries = false)
    {
        if (uGalleryButton)
        {
            uGalleryButton.isOn = false;
        }

        if (noEntries && uGallerySnackbar)
            uGallerySnackbar.gameObject.SetActive(true);
    }
    #endregion

    /// <summary>
    /// Shutdown button pressed. Shuts the network down.
    /// </summary>
    public void ShutdownButtonPressed()
    {
        CallAppBackend.Instance.ResetCall();
        RemoteCallManager.Instance.EndCall();
    }

    /// <summary>
    /// Should the screen drawing annotation be cached and send constantly to the other devices?
    /// </summary>
    /// <param name="value">true: cache the screen drawing annotation while the drawing activity remains active</param>
    public void SetContinuousDataSend(bool value)
    {
        DrawingManager.InterfaceInstance.HideDrawingOverTime(value);
    }

    /// <summary>
    /// Save configuration changes permanently.
    /// </summary>
    public void SaveConfig()
    {
        StatusProperties.Values.saveData();
    }
}
