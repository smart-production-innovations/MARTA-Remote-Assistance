using UnityEngine;
using Unity.Collections;
using System.IO;
using System.Collections.Generic;
using Byn.Awrtc.Native;
using Byn.Awrtc.Unity;
using System.Collections;
using System.Linq;
using System;
using UnityEngine.UI;

#if UNITY_2018_3_OR_NEWER
using UnityEngine.Rendering;
#else
using UnityEngine.Experimental.Rendering;
#endif

/// <summary>
/// capture the camera view to a texture and send it to the other devices
/// </summary>
public class CameraCapture : MonoBehaviour
{
#region properties
    private float mLastSample;
    Queue<AsyncGPUReadbackRequest> _requests = new Queue<AsyncGPUReadbackRequest>();
    private Texture2D mTexturePortraitn, mTextureLandscape;
    private bool isFPSRenderFrame = false;
    private int captureWidth, captureHeight;
    private int sendWidth = 720, sendHeight = 480; 
    private Camera captureCamera;

    private List<int> bandwidthFPS = new List<int>();
    private float timeStempVideoFrameReceived = -1;
    private float maxObservationPeriod = 1;
    private bool remoteVideoFrameReceived = false;
    public ulong updateFrameCount = 0, receiveFrameCount = 0;

    bool coroutineRunning = false;
    private bool isDeviceAdded = false;

    /// <summary>
    /// Name used to access it later via MediaConfig
    /// </summary>
    public string _DeviceName = "GPUCapture";

    /// <summary>
    /// FPS the virtual device is suppose to have.
    /// (This isn't really used yet except to filter
    /// out this device if MediaConfig requests specific FPS)
    /// </summary>
    private int _Fps = 24;

    public int MinFPS = 12, MaxFPS = 24;
    public int MinLongSide = 720;

    /// <summary>
    /// Interface for video device input.
    /// </summary>
    private NativeVideoInput mVideoInput;

    private Texture2D reducedTexture;

    /// <summary>
    /// webrtc bandwidth options: auto calculate resolution of the texture
    /// </summary>
    private bool autoQuality = false;
    public bool AutoQuality
    {
        get { return autoQuality; }
        set { autoQuality = value; }
    }

    /// <summary>
    /// webrtc bandwidth options: auto calculate fps of the video signal
    /// </summary>
    private bool autoFPS = true;
    public bool AutoFPS
    {
        get { return autoFPS; }
        set { autoFPS = value; }
    }

    /// <summary>
    /// webrtc bandwidth options: fps of the video signal
    /// </summary>
    public int FPS
    {
        get { return _Fps; }
        set { _Fps = value; }
    }

    /// <summary>
    /// webrtc bandwidth options: video send mode (constant video stream or low bandwidth single image frames)
    /// </summary>
    private bool lowBandwidthMode = false;
    public bool LowBandwidthMode
    {
        get { return lowBandwidthMode; }
        set
        {
            lowBandwidthMode = value;
        }
    }

    /// <summary>
    /// Device name for sending in portrait mode
    /// </summary>
    public string DeviceNamePortrait
    {
        get
        {
            return _DeviceName + "Portrait";
        }
    }

    /// <summary>
    /// Device name for sending in landscape mode
    /// </summary>
    public string DeviceNameLandscape
    {
        get
        {
            return _DeviceName + "Landscape";
        }
    }

    /// <summary>
    /// command sting for actual video size
    /// </summary>
    public string VideoSizeCommand
    {
        get
        {
            loadOptimalVideoSize();
            syncRotation(Screen.width, Screen.height);
            return Commands.getCoordinatesString(CaptureCamera.pixelWidth, CaptureCamera.pixelHeight);
        }
    }

    /// <summary>
    /// active video device name
    /// unfortunately device change at runtime dose not work
    /// </summary>
    private string activeDeviceName;
    private string ActiveDeviceName
    {
        get
        {
            var oldDevice = activeDeviceName;

            if (CaptureCamera.pixelWidth < CaptureCamera.pixelHeight)
                activeDeviceName = DeviceNamePortrait;
            else
                activeDeviceName = DeviceNameLandscape;

            if (oldDevice != activeDeviceName)
            {
                EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.VideoSize, VideoSizeCommand));
            }

            return DeviceNameLandscape;
        }
    }

    /// <summary>
    /// get active video device name
    /// </summary>
    public string CallDeviceName
    {
        get
        {
            AddVideoDevices();
            return ActiveDeviceName;
        }
    }

    /// <summary>
    /// send video image frame every 1/FPS to the other devices over webrtc. Calculate if a frame has to be send.
    /// </summary>
    private bool IsFPSRenderFrame
    {
        get
        {
            float deltaSample = 1.0f / _Fps;
            mLastSample += Time.deltaTime;
            if (mLastSample >= deltaSample)
            {
                mLastSample -= deltaSample;
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// return the active capture texture depending from the device orientation
    /// </summary>
    private Texture2D ActiveTexture
    {
        get
        {
            if (CaptureCamera.pixelWidth < CaptureCamera.pixelHeight)
                return mTexturePortraitn;
            return mTextureLandscape;
        }
    }

    /// <summary>
    /// get camera component assigned to CameraCapture component
    /// </summary>
    private Camera CaptureCamera
    {
        get
        {
            if (!captureCamera)
            {
                captureCamera = GetComponent<Camera>();
                if (captureCamera.targetTexture)
                {
                    syncRotation(Screen.width, Screen.height);
                }
            }
            return captureCamera;
        }
    }
#endregion

#region unity loop
    private void Awake()
    {
        _Fps = MaxFPS;

        syncRotation(Screen.width, Screen.height);
        createEmptyTextures();
    }

    void Start()
    {
        loadOptimalVideoSize();
        createEmptyTextures();

        // Add this camera to webrtc send work flow
        AddVideoDevices();
    }

    private void createEmptyTextures()
    {
        int shortSide = Mathf.Min(captureWidth, captureHeight);
        int longSide = Mathf.Max(captureWidth, captureHeight);

        mTexturePortraitn = new Texture2D(shortSide, longSide, TextureFormat.ARGB32, false);
        mTextureLandscape = new Texture2D(longSide, shortSide, TextureFormat.ARGB32, false);
        reducedTexture = new Texture2D(sendWidth, sendHeight, TextureFormat.ARGB32, false);
    }

    /// <summary>
    /// Add this camera to webrtc send work flow
    /// </summary>
    public void AddVideoDevices()
    {
        if (isDeviceAdded) return;
        loadOptimalVideoSize();

        int shortSide = Mathf.Min(sendWidth, sendHeight);
        int longSide = Mathf.Max(sendWidth, sendHeight);

        mVideoInput = UnityCallFactory.Instance.VideoInput;
        mVideoInput.AddDevice(DeviceNamePortrait, shortSide, longSide, _Fps);
        mVideoInput.AddDevice(DeviceNameLandscape, longSide, shortSide, _Fps);

        isDeviceAdded = true;
    }

    private void loadOptimalVideoSize()
    {
        // set video capture resolution
        sendWidth = captureWidth = ResolutionManager.Instance.lWidth;
        sendHeight = captureHeight = ResolutionManager.Instance.lHeight;

        int shortSide = Mathf.Min(sendWidth, sendHeight);

        if (shortSide > 1080)
        {
            var differenceFactor = 1080f / shortSide;

            sendWidth = (int)(sendWidth * differenceFactor);
            sendHeight = (int)(sendHeight * differenceFactor);
        }
    }

    private void OnDestroy()
    {
        Destroy(mTexturePortraitn);
        Destroy(mTextureLandscape);
        Destroy(reducedTexture);

        if (mVideoInput != null)
        {
            mVideoInput.RemoveDevice(DeviceNamePortrait);
            mVideoInput.RemoveDevice(DeviceNameLandscape);
        }
    }
#endregion

    /// <summary>
    /// get status message from other device that video frame was received to check send band with. If sending takes to long reduce video quality.
    /// </summary>
    public void RemoteVideoFrameReceived()
    {
        remoteVideoFrameReceived = true;
        receiveFrameCount++;
        if (timeStempVideoFrameReceived > -1)
        {

            float deltaSample = 1.0f / _Fps;
            float deltaTimeSpan = Time.time - timeStempVideoFrameReceived;
            int remoteFPS = (int)(1.0f / deltaTimeSpan);
            if (remoteFPS > MaxFPS * 2) remoteFPS = MaxFPS * 2;

            bandwidthFPS.Add(remoteFPS);
            var observationPeriod = bandwidthFPS.Sum(x => 1f / x);

            //save values of two seconds
            if (observationPeriod > maxObservationPeriod)
            {
                if (bandwidthFPS.Count > 1)
                    bandwidthFPS.RemoveAt(bandwidthFPS.Count - 1);
                remoteFPS = (int)bandwidthFPS.Average();

                // check if bandwidth for more than 60% of the frames is bad over the hole last seconds
                if (bandwidthFPS.Count(x => x >= _Fps) <= bandwidthFPS.Count * 0.4f)
                {
                    bool autoUpdateDone = false;
                    if (AutoFPS)
                    {
                        autoUpdateDone = true;
                        if (remoteFPS < _Fps)
                        {
                            if (remoteFPS >= MinFPS) _Fps = remoteFPS;
                            else _Fps = MinFPS;
                        }
                        else if (_Fps > MinFPS) _Fps -= 1;
                        else autoUpdateDone = false;
                    }

                    if (AutoQuality && !autoUpdateDone)
                    {
                        if (Mathf.Max(sendWidth, sendHeight) > MinLongSide)
                        {
                            sendWidth /= 2;
                            sendHeight /= 2;

                            if (Mathf.Max(sendWidth, sendHeight) < MinLongSide)
                            {
                                setSenderSize(MinLongSide);
                            }

                            autoUpdateDone = true;
                        }
                    }


                    bandwidthFPS.Clear();
                }
                // check if bandwidth for more than 60% of the frames is good over the hole last seconds
                else if (bandwidthFPS.Count(x => x < _Fps) <= bandwidthFPS.Count * 0.4f)
                {
                    bool autoUpdateDone = false;
                    if (AutoQuality)
                    {
                         if (sendWidth < Mathf.Max(captureWidth, captureHeight))
                        {
                            sendWidth *= 2;
                            sendHeight *= 2;

                            if (sendWidth > Mathf.Max(captureWidth, captureHeight))
                            {
                                setSenderSize(Mathf.Max(captureWidth, captureHeight));
                            }
                            autoUpdateDone = true;
                        }
                    }

                    if (AutoFPS && !autoUpdateDone)
                    {
                        autoUpdateDone = true;
                        if (remoteFPS > _Fps)
                        {
                            if (remoteFPS <= MaxFPS) _Fps = remoteFPS;
                            else _Fps = MaxFPS;
                        }
                        else if (_Fps < MaxFPS) _Fps += 1;
                        else autoUpdateDone = false;
                    }

                    bandwidthFPS.Clear();
                }
            }
        }
        timeStempVideoFrameReceived = Time.time;
    }

    /// <summary>
    /// adapt the video size for sending
    /// </summary>
    /// <param name="longSide"></param>
    public void setSenderSize(int longSide)
    {
        if (sendWidth > sendHeight)
        {
            var ration = sendHeight / (float)sendWidth;
            sendWidth = longSide;
            sendHeight = (int)(longSide * ration);
        }
        else
        {
            var ration = sendWidth / (float)sendHeight;
            sendHeight = longSide;
            sendWidth = (int)(longSide * ration);
        }
		
		syncRotation(Screen.width, Screen.height);
    }

#region render frame
    private bool calculateLowBandwidthPicture = false;
    /// <summary>
    /// In low bandwidth mode instance photo button was pressed. Send the current video frame for annotating to the other device.
    /// </summary>
    public void SendScreenshot()
    {
        calculateLowBandwidthPicture = true;
    }

    /// <summary>
    /// Tear down RenderTexture and send it to the client.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!LowBandwidthMode || calculateLowBandwidthPicture)
            SaveBitmap(source);

        Graphics.Blit(source, destination);
    }

    /// <summary>
    /// save frame to a texture
    /// </summary>
    /// <param name="source"></param>
    void SaveBitmap(RenderTexture source)
    {
        //ensure correct fps
        if (IsFPSRenderFrame)
        {
            if (!coroutineRunning)
            {
                StopAllCoroutines();
                StartCoroutine(CoroutineSendFrame(source));
            }
            else
            {
                EventNameManager.SendEventAppendDebug("Drop Frame: " + Time.frameCount);
            }
        }
    }

    /// <summary>
    /// save frame to a texture
    /// </summary>
    /// <param name="source">target frame</param>
    /// <returns></returns>
    public IEnumerator CoroutineSendFrame(RenderTexture source)
    {
        try
        {
            coroutineRunning = true;

            syncRotation(Screen.width, Screen.height);
            var mTexture = ActiveTexture;
			
			if (mTexture.width != source.width || mTexture.height != source.height)
				mTexture.Resize(source.width, source.height);

            // calculate texture for the current video frame
            mTexture.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
            mTexture.Apply();

            if (!calculateLowBandwidthPicture)
            {
                // in live video mode the current frame is immediately sent via webRTC
                sendFrameToRemoteDevice(mTexture);
            }
            else
            {
                // In single frame mode, clicking on the photographing icon transfers the video frame and create an additional anchor point for annotation by the expert.
                // The anchor point is placed on the far clipping plane of the camera .
                var anchor = AnnotationManager.Instance.createEmptyAnnotation(CameraHelper.ARCameraStaticAnchorPosition, CameraHelper.ARCamera.transform.rotation, 1, annotationOwner: AnnotationOwner.Client);
                if (anchor)
                {
                    var iAnchor = anchor as IAnchorAnnotationBase;
                    var pivot = new Vector2(0.5f, 0.5f);
                    anchor.SetPivot(pivot);

                    iAnchor.AnnotationOwner = AnnotationOwner.Server;

                    var clickPoint = Vector2.zero;
                    if (anchor is AnchorAnnotationDistanceScale)
                        clickPoint = ((AnchorAnnotationDistanceScale)anchor).RelativeScreenClickCenterPivotPosition;

                    List<ARLayerPlane> planeList = null;
                    if (anchor is AnchorAnnotationProjection3D)
                        planeList = ((AnchorAnnotationProjection3D)anchor).DetectedPlanes;

                    // The anchor point data is transmitted from the mobile device on site to the expert.
                    CommunicationManager.Instance.SendData(new AnchorImageData(anchor.Anchor.Id, pivot, clickPoint,
                            planeList: planeList, cameraTransform: new TransformParameter(CameraHelper.ARCamera.transform), anchorTransform: anchor.transform));

                    sendSnapshotToRemoteDevice(mTexture);
                }

            }
        }
        catch (Exception e)
        {

        }

        calculateLowBandwidthPicture = false;
        coroutineRunning = false;

        yield return null;
    }

    /// <summary>
    /// send device orientation to other devices
    /// </summary>
    /// <param name="source">target frame</param>
    private void syncRotation(RenderTexture source)
    {
        syncRotation(source.width, source.height);
    }

    /// <summary>
    /// send device orientation to other devices
    /// </summary>
    /// <param name="sourceWidth"></param>
    /// <param name="sourceHeight"></param>
    private void syncRotation(int sourceWidth, int sourceHeight)
    {
        if ((sourceWidth > sourceHeight && sendWidth < sendHeight) ||
            (sourceWidth < sourceHeight && sendWidth > sendHeight))
        {
            int temp = sendWidth;
            sendWidth = sendHeight;
            sendHeight = temp;
        }

        var rt = CaptureCamera.targetTexture;
        if (rt.width != sendWidth || rt.height != sendHeight)
        {
            if (sendWidth == 0) sendWidth = 100;
            if (sendHeight == 0) sendHeight = 100;
            CameraHelper.UpdateTargetTexture(rt, new RenderTexture(sendWidth, sendHeight, rt.depth, rt.format));
            RenderTexture.active = null;
            rt.Release();
        }
    }

    /// <summary>
    /// send video frame to other devices
    /// </summary>
    /// <param name="frameTexture"></param>
    public void sendFrameToRemoteDevice(Texture2D frameTexture)
    {
        //get the byte array. still looking for a way to reuse the current buffer
        //instead of allocating a new one all the time
        var mByteBuffer = frameTexture.GetRawTextureData();

        //update the internal WebRTC device
        updateFrameCount++;
        mVideoInput.UpdateFrame(ActiveDeviceName, mByteBuffer, frameTexture.width, frameTexture.height, WebRtcCSharp.VideoType.kBGRA, 0, true);
    }

    /// <summary>
    /// send single video frame to other devices
    /// </summary>
    /// <param name="frameTexture"></param>
    public void sendSnapshotToRemoteDevice(Texture2D frameTexture)
    {
        StopAllCoroutines();
        StartCoroutine(sendStaticFrameToRemoteDevice(frameTexture));
    }

    /// <summary>
    /// send single video frame to other devices
    /// </summary>
    /// <param name="frameTexture"></param>
    /// <returns></returns>
    public IEnumerator sendStaticFrameToRemoteDevice(Texture2D frameTexture)
    {
        //get the byte array. still looking for a way to reuse the current buffer
        //instead of allocating a new one all the time
        var mByteBuffer = frameTexture.GetRawTextureData();

        remoteVideoFrameReceived = false;
        while (!remoteVideoFrameReceived)
        {
            //update the internal WebRTC device
            updateFrameCount++;
            mVideoInput.UpdateFrame(ActiveDeviceName, mByteBuffer, frameTexture.width, frameTexture.height, WebRtcCSharp.VideoType.kBGRA, 0, true);
            yield return new WaitForSeconds(0.2f);
        }

        yield return null;
    }



    /// <summary>
    /// send video frame to other devices
    /// </summary>
    /// <param name="frameTexture"></param>
    public void sendFrameToRemoteDevice(byte[] data)
    {
        //update the internal WebRTC device
        updateFrameCount++;
        mVideoInput.UpdateFrame(ActiveDeviceName, data, data.Length, 1, WebRtcCSharp.VideoType.kUnknown, 0, true);
    }

#endregion
}
