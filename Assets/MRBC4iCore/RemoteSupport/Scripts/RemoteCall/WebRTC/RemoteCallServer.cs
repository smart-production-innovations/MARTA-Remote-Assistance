using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// manages the UI behavior for the device communication for the expert device
/// 
/// RemoteCallManager rewrites the plugin example class CallAppUi according to the specific remote support requirements.
/// </summary>
public class RemoteCallServer : RemoteCallManager
{
    #region properties
    public RectTransform serverUIPanel;
    public InputField uRoomNameInputField;
    #endregion

    #region unity loop
    protected override void Awake()
    {
        base.Awake();
        isReady = true;
    }

    protected override void Start()
    {
        base.Start();
        if (serverUIPanel) serverUIPanel.gameObject.SetActive(true);
    }
    #endregion

    #region settings
    /// <summary>
    /// Loads the UI state and create an unique communication key.
    /// </summary>
    protected override void LoadSettings()
    {
        base.LoadSettings();
#if UNITY_WEBGL
        URLParameters.Instance.RegisterOnDone((url) => {
            unique_id = url.SearchParameters["key"];
            ChatManager.Instance.AppendDebug("unique_id1: " + unique_id);
        });
#else
#endif

        videoOn = false;
    }

    /// <summary>
    /// Join button pressed. Tries to join a room.
    /// </summary>
    public override void JoinButtonPressed()
    {
        if (uRoomNameInputField.text.Trim().Length > 0)
        {
            base.JoinButtonPressed();
            unique_id = uRoomNameInputField.text.Trim();
            GetComponentInChildren<InitNewConnection>().ActivateWait();
        }
    }

    /// <summary>
    /// Set communication parameters in CallAppBackground class.
    /// </summary>
    protected override void SetupCallApp()
    {
        base.SetupCallApp();
        MApp.Join(uRoomNameInputField.text.Trim());
    }

    /// <summary>
    /// check maximal key length.
    /// </summary>
    protected override void EnsureLength()
    {
        base.EnsureLength();
        if (uRoomNameInputField.text.Length > CallAppBackend.MAX_CODE_LENGTH)
        {
            uRoomNameInputField.text = uRoomNameInputField.text.Substring(0, CallAppBackend.MAX_CODE_LENGTH);
        }
    }
    #endregion

    /// <summary>
    /// When the connection is ends, change the UI to connecting UI.
    /// </summary>
    public override void EndCall()
    {
        base.EndCall();
    }
}
