using Byn.Awrtc;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// manages the UI behavior for the device communication
/// 
/// RemoteCallManager rewrites the plugin example class CallAppUi according to the specific remote support requirements.
/// </summary>
[RequireComponent(typeof(CallAppBackend))]
public class RemoteCallManager : AManager<RemoteCallManager>
{
    #region properties
    public GameObject connectionUI;
    public GameObject communicationUI;

    protected CallAppBackend mApp;
    /// <summary>
    /// WebRTC back end instance
    /// </summary>
    protected CallAppBackend MApp
    {
        get
        {
            if (mApp == null)
                mApp = GetComponent<CallAppBackend>();

            return mApp;
        }
    }
    protected string unique_id;

    protected bool isReady = false;

    protected bool videoOn = true, audioOn = true, autoRejoin = false, showLocalVideo = true;
    protected int idealFPS = 30;

    public string UniqueID
    {
        get
        {
            return unique_id;
        }
    }

    /// <summary>
    /// reset values: If a new connection to another worker on site is initiated without closing the expert application, all connection parameters should be reset to the default value.
    /// </summary>
    private void setToDefault()
    {
        isReady = false;
        setUIState(false);

        unique_id = null;
        videoOn = true;
        audioOn = true;
        autoRejoin = false;
        showLocalVideo = true;
        idealFPS = 30;
    }
    #endregion

    #region unity loop
    protected override void Awake()
    {
        base.Awake();
        StatusProperties.Values.loadPreset(informDevices: false);
        setToDefault();
        LoadSettings();

        ActivateConnection();
    }

    void OnDestroy()
    {
        setToDefault();
    }

    protected virtual void Start()
    {
    }
    #endregion

    #region UI state
    /// <summary>
    /// set communication state to connection.
    /// </summary>
    public void ActivateConnection()
    {
        setUIState(false);
    }

    /// <summary>
    /// set communication state to active communication.
    /// </summary>
    public void ActivateCommunication()
    {
        setUIState(true);
    }

    /// <summary>
    /// set communication state to connection or active communication.
    /// </summary>
    /// <param name="connectionEstablished">if true active communication UI is visible</param>
    private void setUIState(bool connectionEstablished)
    {
        if (connectionUI) connectionUI.gameObject.SetActive(!connectionEstablished);
        if (communicationUI) communicationUI.SetActive(connectionEstablished);
    }

    /// <summary>
    /// Initialize the a new communication process
    /// </summary>
    /// <param name="key">unique communication key</param>
    public void InitCommunication(string key)
    {
    }
    #endregion

    #region settings
    /// <summary>
    /// Create an unique communication key.
    /// </summary>
    protected virtual void LoadSettings()
    {
        unique_id = "";
        if (StatusProperties.Values.GenerateUniqueKey)
            unique_id = "MRBC4i" + SystemInfo.deviceUniqueIdentifier + DateTime.Now.ToString("yyMMddHHmm");
        else
        {
            unique_id = FileHelper.ReadTextFile("staticKey.txt");
            if (unique_id == null)
                unique_id = "000";
        }
        ActionEventManager.SendEvent<string>(EventName.UniqueKeyCalculated, unique_id);
    }

    /// <summary>
    /// Set communication parameters in CallAppBackground class.
    /// </summary>
    protected virtual void SetupCallApp()
    {
        CommunicationManager.Instance.SetActiveVideoDevice();
        MApp.SetAudio(audioOn);
        MApp.SetVideo(videoOn);

        int width = Screen.width;
        int height = Screen.height;
        int fps = idealFPS;
        MApp.SetIdealResolution(width, height);
        MApp.SetIdealFps(fps);
        MApp.SetAutoRejoin(autoRejoin);
        MApp.SetShowLocalVideo(showLocalVideo);
        MApp.SetupCall();
        EnsureLength();
    }

    /// <summary>
    /// check maximal key length.
    /// </summary>
    protected virtual void EnsureLength()
    {
    }
    #endregion

    #region communication between devices
    #region init communication
    /// <summary>
    /// Shows the setup screen or the chat + video
    /// </summary>
    /// <param name="showSetup">true Shows the setup. False hides it.</param>
    public void SetGuiState(bool showSetup)
    {
        CommunicationManager.Instance.UpdateRemoteTexture(null, FramePixelFormat.Invalid);
    }

    /// <summary>
    /// Join button pressed. Tries to join a room.
    /// </summary>
    public virtual void JoinButtonPressed()
    {
        ChatManager.Instance.ClearMessageBox();
        SetupCallApp();
    }

    /// <summary>
    /// When the connection is established, change the UI to active communication UI.
    /// </summary>
    public virtual void ConnectionEstablished()
    {
        setUIState(true);
    }

    #endregion

    #region end communication
    /// <summary>
    /// Shutdown button pressed. Shuts the network down.
    /// </summary>
    public void ShutdownButtonPressed()
    {
        MApp.ResetCall();
        EndCall();

        if (GetComponentInChildren<InitNewConnection>())
            GetComponentInChildren<InitNewConnection>().OnEnable();
    }

    /// <summary>
    /// When the connection is ends, change the UI to connecting UI.
    /// </summary>
    public virtual void EndCall()
    {
        setUIState(false);
    }
    #endregion
    #endregion
}
