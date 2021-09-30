using Byn.Awrtc.Unity;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// manages the UI behavior for the device communication for the client device
/// 
/// RemoteCallManager rewrites the plugin example class CallAppUi according to the specific remote support requirements.
/// </summary>
public class RemoteCallClient : RemoteCallManager
{
    #region properties
    private int keySendTryCount = 20;
    private int waitCount = 0;
    #endregion

    #region unity loop
    protected override void Awake()
    {
        base.Awake();

#if PLATFORM_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
        }
#endif
        StartCoroutine(SendKeyToServer());
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(WaitForSecondsWrapper(1f));
    }

    /// <summary>
    /// send unique key to expert
    /// </summary>
    /// <returns></returns>
    private IEnumerator SendKeyToServer()
    {
        while (UnityCallFactory.Instance.VideoInput == null && keySendTryCount > 0)
        {
            yield return new WaitForSeconds(0.1f);
            keySendTryCount--;
        }

        if (UnityCallFactory.Instance.VideoInput != null)
        {
            string message = FileHelper.ReadTextFile("email.txt");
            if (message == null)
            {
                message = "key: @key \n\r";
            }
            message = message.Replace("@key", unique_id);


#if (UNITY_IOS || UNITY_ANDROID)
            if (StatusProperties.Values.IsSmartGlass)
            {
                ContactList.instance.SendMessageToContact(message);
            }
            else
            {
                new NativeShare().SetText(message).SetSubject("Remote Support Key").SetTitle("Remote Support Key").Share();
            } 
#else
            //Application.OpenURL("mailto:?subject=Remote Support Key&body=" + message);
#endif

            isReady = true;
        }
        yield return null;
    }
    #endregion

    #region settings
    /// <summary>
    /// Loads the UI state and create an unique communication key.
    /// </summary>
    protected override void LoadSettings()
    {
        base.LoadSettings();
        videoOn = true;
    }

    /// <summary>
    /// Tries to join a room.
    /// </summary>
    /// <param name="secs"></param>
    /// <returns></returns>
    IEnumerator WaitForSecondsWrapper(float secs)
    {
        while (!MApp.IsReady && waitCount < 10)
        {
            yield return new UnityEngine.WaitForSeconds(secs);
            waitCount++;
        }

        JoinButtonPressed();
    }

    /// <summary>
    /// Set communication parameters in CallAppBackground class.
    /// </summary>
    protected override void SetupCallApp()
    {
        base.SetupCallApp();
        MApp.Join(unique_id);
    }

    /// <summary>
    /// check maximal key length.
    /// </summary>
    protected override void EnsureLength()
    {
        base.EnsureLength();
        if (unique_id.Length > CallAppBackend.MAX_CODE_LENGTH)
        {
            unique_id = unique_id.Substring(0, CallAppBackend.MAX_CODE_LENGTH);
        }
    }
    #endregion

    #region communication between devices
    #region init communication
    /// <summary>
    /// When the connection is established, change the UI to active communication UI.
    /// </summary>
    public override void ConnectionEstablished()
    {
        CommunicationManager.Instance.SetActiveVideoDeviceSize();
        base.ConnectionEstablished();
    }

    #endregion

    #region end communication
    /// <summary>
    /// When the connection is ends, change the UI to connecting UI.
    /// </summary>
    public override void EndCall()
    {
        base.EndCall();
        Application.Quit();
    }
    #endregion
    #endregion
}
