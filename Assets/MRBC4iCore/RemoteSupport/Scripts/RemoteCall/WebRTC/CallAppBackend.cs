﻿using Byn.Awrtc;
using Byn.Awrtc.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main example and test app for usage of WebRTC Video Chat.
/// It can be used directly as a plug and play example via the CallApp prefab.
/// 
/// CallAppBackend rewrites the plugin example class CallApp according to the specific remote support requirements.
/// </summary>
public class CallAppBackend : AManager<CallAppBackend>
{
    #region properties
    /// <summary>
    /// This is a test server. Don't use in production! The server code is in a zip file in WebRtcNetwork
    /// </summary>
    public string uSignalingUrl = "ws://signaling.because-why-not.com/callapp";

    /// <summary>
    /// By default the secure version is currently only used in WebGL builds as
    /// some browsers require. Unity old mono version comes with a SSL implementation
    /// that can be quite slow and hangs sometimes
    /// </summary>
    public string uSecureSignalingUrl = "wss://signaling.because-why-not.com/callapp";

    /// <summary>
    /// If set to true only the secure signaling url will be used.
    /// </summary>
    public bool uForceSecureSignaling = false;


    /// <summary>
    /// Ice server is either a stun or a turn server used to get trough
    /// the firewall.
    /// Warning: make sure the url is in a valid format and
    /// starts with stun: or turn:
    /// 
    /// WebRTC will try many different ways to connect the peers so if
    /// this server is not available it might still be able
    /// to establish a direct connection or use the second ice server.
    /// 
    /// If you need more than two servers change the CreateNetworkConfig
    /// method.
    /// </summary>
    public string uIceServer = "stun:stun.because-why-not.com:443";

    public string uIceServerUser = "";
    public string uIceServerPassword = "";

    /// <summary>
    /// Second ice server. As I can't guarantee the test server is always online.
    /// If you need more than two servers or username / password then
    /// change the CreateNetworkConfig method.
    /// </summary>
    public string uIceServer2 = "stun:stun.l.google.com:19302";

    public string uIceServerUser2 = "";
    public string uIceServerPassword2 = "";


    /// <summary>
    /// Do not change. This length is enforced on the server side to avoid abuse.
    /// </summary>
    public const int MAX_CODE_LENGTH = 256;

    /// <summary>
    /// Call class handling all the functionality
    /// </summary>
    protected ICall mCall;


    /// <summary>
    /// Contains the configuration used for the next call
    /// </summary>
    protected MediaConfig mMediaConfig;

    //Configuration for the currently active call
    /// <summary>
    /// Set to true after Join is called.
    /// Set to false after either Join failed or the call
    /// ended / network failed / user exit
    /// 
    /// </summary>
    private bool mCallActive = false;
    private string mUseAddress = null;
    protected MediaConfig mMediaConfigInUse;
    private ConnectionId mRemoteUserId = ConnectionId.INVALID;


    private bool mAutoRejoin = false;
    private float mRejoinTime = 2;

    private bool mLocalFrameEvents = true;

    private bool isReady = false;

    /// <summary>
    /// Get the configuration for this CallApp instance.
    /// </summary>
    protected MediaConfig MMediaConfig
    {
        get
        {
            if (mMediaConfig == null)
            {
                mMediaConfig = CreateMediaConfig();
                mMediaConfigInUse = mMediaConfig;
            }

            return mMediaConfig;
        }
    }
    #endregion

    #region init
    /// <summary>
    /// set default values
    /// </summary>
    private void setToDefault()
    {
        CleanupCall();

        mCallActive = false;
        mUseAddress = null;
        mRemoteUserId = ConnectionId.INVALID;

        mAutoRejoin = false;
        mRejoinTime = 2;
        mLocalFrameEvents = true;
        isReady = false;
    }

    /// <summary>
    /// is read to send data?
    /// </summary>
    public bool IsReady
    {
        get { return isReady; }
    }
    #endregion

    #region Calls from unity
    protected override void Awake()
    {
        base.Awake();
        setToDefault();

        ActionEventManager.Subscribe<string>(EventName.WebRTCSend, Send);
    }

    protected virtual void Start()
    {
        UnityCallFactory.EnsureInit(OnCallFactoryReady, OnCallFactoryFailed);
        if (UnityCallFactory.Instance.VideoInput == null)
        {
            EventNameManager.SendEventShowARHelper("CloseApp");
        }
    }

    /// <summary>
    /// Called once the call factory is ready to be used.
    /// </summary>
    protected virtual void OnCallFactoryReady()
    {
        //set to warning for regular use
        UnityCallFactory.Instance.RequestLogLevel(UnityCallFactory.LogLevel.Info);
        RemoteCallManager.Instance.SetGuiState(true);
        isReady = true;
    }
    /// <summary>
    /// Called if the call factory failed to initialize.
    /// This is usually an asset configuration error, attempt to run a platform that isn't supported or the user
    /// managed to run the app while blocking video / audio access
    /// </summary>
    /// <param name="error">Error returned by the init process.</param>
    protected virtual void OnCallFactoryFailed(string error)
    {
        string fullErrorMsg = typeof(CallApp).Name + " can't start. The " + typeof(UnityCallFactory).Name + " failed to initialize with following error: " + error;
        Debug.LogError(fullErrorMsg);
    }


    private void OnDestroy()
    {
        CleanupCall();
        ActionEventManager.Unsubscribe<string>(EventName.WebRTCSend, Send);
        setToDefault();
    }

    /// <summary>
    /// The call object needs to be updated regularly to sync data received via webrtc with
    /// unity. All events will be triggered during the update method in the unity main thread
    /// to avoid multi threading errors
    /// </summary>
    protected virtual void Update()
    {
        if (mCall != null)
        {
            //update the call object. This will trigger all buffered events to be fired
            //to ensure it is done in the unity thread at a convenient time.
            mCall.Update();
        }
    }
    #endregion

    /// <summary>
    /// Specify the servers for communication.
    /// </summary>
    /// <returns></returns>
    protected virtual NetworkConfig CreateNetworkConfig()
    {
        NetworkConfig netConfig = new NetworkConfig();
        if (string.IsNullOrEmpty(uIceServer) == false)
            netConfig.IceServers.Add(new IceServer(uIceServer, uIceServerUser, uIceServerPassword));
        if (string.IsNullOrEmpty(uIceServer2) == false)
            netConfig.IceServers.Add(new IceServer(uIceServer2, uIceServerUser2, uIceServerPassword2));

        if (Application.platform == RuntimePlatform.WebGLPlayer || uForceSecureSignaling)
        {
            netConfig.SignalingUrl = uSecureSignalingUrl;
        }
        else
        {
            netConfig.SignalingUrl = uSignalingUrl;
        }

        if (string.IsNullOrEmpty(netConfig.SignalingUrl))
        {
            throw new InvalidOperationException("set signaling url is null or empty");
        }
        return netConfig;
    }

    /// <summary>
    /// Creates the call object and uses the configure method to activate the 
    /// video / audio support if the values are set to true.
    /// generating new frames after this call so the user can see himself before
    /// the call is connected.
    /// </summary>
    public virtual void SetupCall()
    {
        Append("Setting up ...");

        //hacks to turn off certain connection types. If both set to true only
        //turn servers are used. This helps simulating a NAT that doesn't support
        //opening ports.
        //hack to turn off direct connections
        //Byn.Net.Native.AWebRtcPeer.sDebugIgnoreTypHost = true;
        //hack to turn off connections via stun servers
        //Byn.Net.Native.WebRtcDataPeer.sDebugIgnoreTypSrflx = true;

        NetworkConfig netConfig = CreateNetworkConfig();


        Debug.Log("Creating call using NetworkConfig:" + netConfig);
        //setup the server
        mCall = UnityCallFactory.Instance.Create(netConfig);
        if (mCall == null)
        {
            Append("Failed to create the call");
            return;
        }

        mCall.LocalFrameEvents = mLocalFrameEvents;
        string[] devices = UnityCallFactory.Instance.GetVideoDevices();
        if (devices == null || devices.Length == 0)
        {
            Debug.Log("no device found or no device information available");
        }
        else
        {
            foreach (string s in devices)
                Debug.Log("device found: " + s + " IsFrontFacing: " + UnityCallFactory.Instance.IsFrontFacing(s));
        }
        Append("Call created!");
        mCall.CallEvent += Call_CallEvent;



        //make a deep clone to avoid confusion if settings are changed
        //at runtime. 
        mMediaConfigInUse = MMediaConfig.DeepClone();

        //try to pick a good default video device if the user wants to send video but
        //didn't bother to pick a specific device
        if (mMediaConfigInUse.Video && string.IsNullOrEmpty(mMediaConfigInUse.VideoDeviceName))
        {
            mMediaConfigInUse.VideoDeviceName = UnityCallFactory.Instance.GetDefaultVideoDevice();
        }

        Debug.Log("Configure call using MediaConfig: " + mMediaConfigInUse);
        mCall.Configure(mMediaConfigInUse);
        RemoteCallManager.Instance.SetGuiState(false);
    }



    /// <summary>
    /// Handler of call events.
    /// 
    /// Can be customized in via subclasses.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void Call_CallEvent(object sender, CallEventArgs e)
    {
        switch (e.Type)
        {
            case CallEventType.CallAccepted:
                //Outgoing call was successful or an incoming call arrived
                Append("Connection established");
                mRemoteUserId = ((CallAcceptedEventArgs)e).ConnectionId;
                Debug.Log("New connection with id: " + mRemoteUserId
                    + " audio:" + mCall.HasAudioTrack(mRemoteUserId)
                    + " video:" + mCall.HasVideoTrack(mRemoteUserId));
                RemoteCallManager.Instance.ConnectionEstablished();
                ActionEventManager.SendEvent(EventName.RemoteCallAccepted);
                break;
            case CallEventType.CallEnded:
                //Call was ended / one of the users hung up -> reset the app
                Append("Call ended");
                InternalResetCall();
                RemoteCallManager.Instance.EndCall();
                ActionEventManager.SendEvent(EventName.RemoteCallEnded);
                break;
            case CallEventType.ListeningFailed:
                //listening for incoming connections failed
                //this usually means a user is using the string / room name already to wait for incoming calls
                //try to connect to this user
                //(note might also mean the server is down or the name is invalid in which case call will fail as well)
                mCall.Call(mUseAddress);
                break;

            case CallEventType.ConnectionFailed:
                {
                    ErrorEventArgs args = e as ErrorEventArgs;
                    Append("Connection failed error: " + args.ErrorMessage);
                    InternalResetCall();
                }
                break;
            case CallEventType.ConfigurationFailed:
                {
                    ErrorEventArgs args = e as ErrorEventArgs;
                    Append("Configuration failed error: " + args.ErrorMessage);
                    InternalResetCall();
                }
                break;

            case CallEventType.FrameUpdate:
                {

                    //new frame received from webrtc (either from local camera or network)
                    if (e is FrameUpdateEventArgs)
                    {
                        UpdateFrame((FrameUpdateEventArgs)e);
                    }
                    break;
                }

            case CallEventType.Message:
                {
                    //text message received
                    MessageEventArgs args = e as MessageEventArgs;
                    CommunicationManager.Instance.ReceiveMsg(args.Content);
                    ActionEventManager.SendEvent<string>(EventName.RemoteMessage, args.Content);
                    break;
                }
            case CallEventType.DataMessage:
                {
                    //data message received
                    DataMessageEventArgs args = e as DataMessageEventArgs;
                    CommunicationManager.Instance.ReceiveData(args.Content);
                    ActionEventManager.SendEvent<byte[]>(EventName.RemoteData, args.Content);
                    break;
                }
            case CallEventType.WaitForIncomingCall:
                {
                    //the chat app will wait for another app to connect via the same string
                    WaitForIncomingCallEventArgs args = e as WaitForIncomingCallEventArgs;
                    Append("Waiting for incoming call address: " + args.Address);
                    break;
                }
        }

    }

    /// <summary>
    /// Destroys the call. Used if unity destroys the object or if a call
    /// ended / failed due to an error.
    /// 
    /// </summary>
    protected virtual void CleanupCall()
    {
        if (mCall != null)
        {
            mCallActive = false;
            mRemoteUserId = ConnectionId.INVALID;
            Debug.Log("Destroying call!");
            mCall.CallEvent -= Call_CallEvent;
            mCall.Dispose();
            mCall = null;
            //call the garbage collector. This isn't needed but helps discovering
            //memory bugs early on.
            Debug.Log("Triggering garbage collection");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Debug.Log("Call destroyed");
        }
    }


    /// <summary>
    /// Create the default configuration for this CallApp instance.
    /// This can be overwritten in a subclass allowing the creation custom apps that
    /// use a slightly different configuration.
    /// </summary>
    /// <returns></returns>
    public virtual MediaConfig CreateMediaConfig()
    {
        MediaConfig mediaConfig = new MediaConfig();
        //testing echo cancellation (native only)
        bool useEchoCancellation = false;
        if(useEchoCancellation)
        {
#if (!UNITY_WEBGL && !UNITY_WSA)
            var nativeConfig = new Byn.Awrtc.Native.NativeMediaConfig();
            nativeConfig.AudioOptions.echo_cancellation = true;
            nativeConfig.AudioOptions.extended_filter_aec = true;
            nativeConfig.AudioOptions.delay_agnostic_aec = true;

            mediaConfig = nativeConfig;
#endif 
        }



        //use video and audio by default (the UI is toggled on by default as well it will change on click )
        mediaConfig.Audio = true;
        mediaConfig.Video = true;
        mediaConfig.VideoDeviceName = null;

        //This format is the only reliable format that works on all
        //platforms currently.
        mediaConfig.Format = FramePixelFormat.ABGR;

        mediaConfig.MinWidth = 160;
        mediaConfig.MinHeight = 120;
        //Larger resolutions are possible in theory but
        //allowing users to set this too high is risky.
        //A lot of devices do have great cameras but not
        //so great CPU's which might be unable to
        //encode fast enough.
        mediaConfig.MaxWidth = 1920;
        mediaConfig.MaxHeight = 1080;

        //will be overwritten by UI in normal use
        mediaConfig.IdealWidth = 160;
        mediaConfig.IdealHeight = 120;
        mediaConfig.IdealFrameRate = 30;
        return mediaConfig;
    }

    /// <summary>
    /// Destroys the call object and shows the setup screen again.
    /// Called after a call ends or an error occurred.
    /// </summary>
    public virtual void ResetCall()
    {
        //outside quits. don't rejoin automatically
        mAutoRejoin = false;
        InternalResetCall();
    }

    /// <summary>
    /// Call was ended / one of the users hung up -> reset the app
    /// </summary>
    private void InternalResetCall()
    {
        CleanupCall();
        RemoteCallManager.Instance.SetGuiState(true);
        if (mAutoRejoin)
        {
            StopAllCoroutines();
            StartCoroutine(CoroutineRejoin());
        }
    }

    /// <summary>
    /// Allows to control the replay volume of the
    /// remote connection.
    /// </summary>
    /// <param name="volume">
    /// Usually between 0 and 1
    /// </param>
    public virtual void SetRemoteVolume(float volume)
    {
        if (mCall == null)
            return;
        if(mRemoteUserId == ConnectionId.INVALID)
        {
            return;
        }
        mCall.SetVolume(volume, mRemoteUserId);
    }


    /// <summary>
    /// Returns a list of video devices for the UI to show.
    /// This is used to avoid having the UI directly access the UnityCallFactory.
    /// </summary>
    /// <returns></returns>
    public string[] GetVideoDevices()
    {
        if (CanSelectVideoDevice())
        {
            List<string> devices = new List<string>();
            string[] videoDevices = UnityCallFactory.Instance.GetVideoDevices();
            devices.Add("Any");
            devices.AddRange(videoDevices);
            return devices.ToArray();
        }
        else
        {
            return new string[] { "Default" };
        }
    }

    /// <summary>
    /// Used by the UI
    /// </summary>
    /// <returns></returns>
    public bool CanSelectVideoDevice()
    {
        return UnityCallFactory.Instance.CanSelectVideoDevice();
    }

    /// <summary>
    /// Called by UI when the join buttin is pressed.
    /// </summary>
    /// <param name="address"></param>
    public virtual void Join(string address)
    {
        if (address.Length > MAX_CODE_LENGTH)
            throw new ArgumentException("Address can't be longer than " + MAX_CODE_LENGTH);
        mUseAddress = address;
        InternalJoin();
    }

    private void InternalJoin()
    {
        if (mCallActive)
        {
            Debug.LogError("Join call failed. Call is already/still active");
            return;
        }
        Debug.Log("Try listing on address: " + mUseAddress);
        mCallActive = true;
        this.mCall.Listen(mUseAddress);
    }

    private IEnumerator CoroutineRejoin()
    {
        yield return new WaitForSecondsRealtime(mRejoinTime);
        SetupCall();
        InternalJoin();
    }

    /// <summary>
    /// Called by ui to send a message.
    /// </summary>
    /// <param name="msg"></param>
    public virtual void Send(string msg)
    {
        if (this.mCall != null)
            this.mCall.Send(msg);
    }

    /// <summary>
    /// Called by ui to send data.
    /// </summary>
    /// <param name="data"></param>
    public virtual void Send(byte[] data)
    {
        if (this.mCall != null)
        {
            this.mCall.Send(data, false);
        }
    }

    /// <summary>
    /// Turns on sending audio for the next call.
    /// </summary>
    /// <param name="value"></param>
    public void SetAudio(bool value)
    {
        MMediaConfig.Audio = value;
    }
    /// <summary>
    /// Turns on sending video for the next call.
    /// </summary>
    /// <param name="value"></param>
    public void SetVideo(bool value)
    {
        MMediaConfig.Video = value;
    }
    /// <summary>
    /// Allows to set a specific video device.
    /// This isn't supported on WebGL yet.
    /// </summary>
    /// <param name="deviceName"></param>
    public void SetVideoDevice(string deviceName)
    {
        MMediaConfig.VideoDeviceName = deviceName;
    }

    /// <summary>
    /// Changes the target resolution that will be used for
    /// sending video streams.
    /// The closest one the camera can handle will be used.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void SetIdealResolution(int width, int height)
    {
        MMediaConfig.IdealWidth = width;
        MMediaConfig.IdealHeight = height;
    }

    /// <summary>
    /// Sets the ideal FPS.
    /// This has a lower priority than the ideal resolution.
    /// Note that the FPS aren't enforced. It pick
    /// the closest FPS the video device supports.
    /// </summary>
    /// <param name="fps"></param>
    public void SetIdealFps(int fps)
    {
        MMediaConfig.IdealFrameRate = fps;
    }

    /// <summary>
    /// True will show the local video.
    /// False will not return the video and thus
    /// save some CPU work.
    /// </summary>
    /// <param name="showLocalVideo"></param>
    public void SetShowLocalVideo(bool showLocalVideo)
    {
        mLocalFrameEvents = showLocalVideo;
    }
    
    /// <summary>
    /// Can be used to make the app automatically reconnect
    /// if a sudden disconnect occurred or the other side ends
    /// the connection.
    /// </summary>
    /// <param name="rejoin"></param>
    /// <param name="rejoinTime"></param>
    public void SetAutoRejoin(bool rejoin, float rejoinTime = 2)
    {
        mAutoRejoin = rejoin;
        mRejoinTime = rejoinTime;
    }

    /// <summary>
    /// Forwarded to the call factory.
    /// Returns the loudspeaker status on mobile devices.
    /// 
    /// </summary>
    /// <returns></returns>
    public bool GetLoudspeakerStatus()
    {
        //check if call is created to ensure this isn't called before initialization
        if (mCall != null)
        {
            return UnityCallFactory.Instance.GetLoudspeakerStatus();
        }
        return false;
    }

    /// <summary>
    /// Sets the loudspeaker mode via the call factory.
    /// </summary>
    /// <param name="state"></param>
    public void SetLoudspeakerStatus(bool state)
    {
        //check if call is created to ensure this isn't called before initialization
        if (mCall != null)
        {
            UnityCallFactory.Instance.SetLoudspeakerStatus(state);
        }
    }

    /// <summary>
    /// Set to true to mute the microphone.
    /// </summary>
    /// <param name="state"></param>
    public void SetMute(bool state)
    {
        //check if call is created to ensure this isn't called before initialization
        if (mCall != null)
        {
            mCall.SetMute(state);
        }
    }

    /// <summary>
    /// True if the microphone is muted (or sending audio isn't active).
    /// </summary>
    /// <returns></returns>
    public bool IsMute()
    {
        //check if call is created to ensure this isn't called before initialization
        if (mCall != null)
        {
            return mCall.IsMute();
        }
        return true;
    }

    /// <summary>
    /// new frame received from webrtc (either from local camera or network)
    /// </summary>
    /// <param name="frameUpdateEventArgs"></param>
    protected virtual void UpdateFrame(FrameUpdateEventArgs frameUpdateEventArgs)
    {
        //the avoid wasting CPU time the library uses the format returned by the browser -> ABGR little endian thus
        //the bytes are in order R G B A
        //Unity seem to use this byte order but also flips the image horizontally (reading the last row first?)
        //this is reversed using UI to avoid wasting CPU time

        if (frameUpdateEventArgs.IsRemote)
        {
            CommunicationManager.Instance.UpdateRemoteTexture(frameUpdateEventArgs.Frame, frameUpdateEventArgs.Format);
        }
    }

    /// <summary>
    /// append internal status messages to the debug log UI
    /// </summary>
    /// <param name="txt"></param>
    private void Append(string txt)
    {
        ChatManager.Instance.AppendDebug(txt);
    }
}
