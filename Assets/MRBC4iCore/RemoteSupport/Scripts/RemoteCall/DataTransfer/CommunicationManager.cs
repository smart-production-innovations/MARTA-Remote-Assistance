using UnityEngine;
using System.Collections;
using System;
using Byn.Awrtc;
using System.Linq;

/// <summary>
/// manage the video and status command communication between the devices
/// </summary>
public class CommunicationManager : AManager<CommunicationManager>
{
    public SnackbarController newGalleryItemSnackbar;

    protected bool useRemoteCommunication = true;

    protected CameraCapture captureDevice;

    #region unity loop
    protected override void Awake()
    {
        base.Awake();
        captureDevice = SearchHelper.FindSceneObjectOfType<CameraCapture>();

        ActionEventManager.Subscribe<CommandMsg>(EventName.SendCommandMsg, SendCommandMsg);
        ActionEventManager.Subscribe<byte[]>(EventName.SendData, SendDataEvent);
        ActionEventManager.Subscribe<string>(EventName.SetActiveVideoDevice, SetActiveVideoDevice);
    }

    protected virtual void Update()
    {
        CommunicationMonitor.ResendMissingBlocks();
    }

    void OnDestroy()
    {
        ActionEventManager.Unsubscribe<CommandMsg>(EventName.SendCommandMsg, SendCommandMsg);
        ActionEventManager.Unsubscribe<byte[]>(EventName.SendData, SendDataEvent);
        ActionEventManager.Unsubscribe<string>(EventName.SetActiveVideoDevice, SetActiveVideoDevice);
    }

    private void OnEnable()
    {
        // apply the status configuration to all associated UI elements
        ToolProperties.SetAllItemsActive(true);
    }
    #endregion

    #region message commands
    /// <summary>
    /// send status command message
    /// </summary>
    /// <param name="cmd">status command message</param>
    public void SendCommandMsg(CommandMsg cmd)
    {
        string cmdString = Commands.getCommandString(cmd.Command, cmd.Message, cmd.Block);

        if (String.IsNullOrEmpty(cmdString))
        {
            //never send null or empty messages. webrtc can't deal with that
            return;
        }

        if (useRemoteCommunication)
            CallAppBackend.Instance.Send(cmdString);
        else
            ReceiveMsg(cmdString);
    }

    /// <summary>
    /// Receive status command message. Handle all types of status commands.
    /// </summary>
    /// <param name="msg">command message string</param>
    public void ReceiveMsg(string msg)
    {
        if (msg.StartsWith("@"))
        {
            var commands = msg.Split('@');
            foreach (var cmd in commands)
            {
                string param;
                if (Commands.isCommand(cmd, CommandMsgType.VideoSize, out param))
                {
                    ReceiveVideoSize(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.MouseDown, out param))
                {
                    ReceiveMouseDown(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.ImageData, out param))
                {
                    ReceiveImageData(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.AnchorState, out param))
                {
                    ReceiveAnchorState(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.AnchorId, out param))
                {
                    ReceiveAnchorId(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.DataReceived, out param))
                {
                    ReceiveDataReceived(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.VideoFrameReceived, out param))
                {
                    ReceiveVideoFrameReceived(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.DeleteAnchor, out param))
                {
                    ReceiveDeleteAnchor(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.CancelAnchor, out param))
                {
                    ReceiveCancelAnchor(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.BandwidthOptions, out param))
                {
                    ReceiveBandwidthOptions(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.CallForAnchorData, out param))
                {
                    ReceiveCallForAnchorData(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.CallForAnchorSnapshotData, out param))
                {
                    ReceiveCallForAnchorData(param, needSnapshot: true);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.NoAnchorDataFound, out param))
                {
                    ReceiveNoAnchorDataFound(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.CallForNextAnchorData, out param))
                {
                    ReceiveCallForAnchorData(param, 1);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.CallForPreviousAnchorData, out param))
                {
                    ReceiveCallForAnchorData(param, -1);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.ResendBlock, out param))
                {
                    ReceiveResendBlock(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.ConvertInto3DMousePosition, out param))
                {
                    ReceiveConvertInto3DMousePosition(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.CallForGalleryItems, out param))
                {
                    ReceiveCallForGalleryItmes(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.GalleryItems, out param))
                {
                    ReceiveGalleryItmes(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.ProjectionPlaneSelected, out param))
                {
                    ReceiveProjectionPlaneSelected(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.ARMode, out param))
                {
                    ReceiveARMode(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.CalculationMode, out param))
                {
                    ReceiveProjectionMode(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.SupportsARMode, out param))
                {
                    ReceiveSupportsARMode(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.IsSmartGlassCommunication, out param))
                {
                    ReceiveIsSmartGlassCommunication(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.NewGalleryItem, out param))
                {
                    ReceiveNewGalleryItem(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.StatusProperty, out param))
                {
                    ReceiveStatusProperty(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.DeviceResolution, out param))
                {
                    ReceiveDeviceResolution(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.GalleryOpen, out param))
                {
                    if (StatusProperties.Values.IsSmartGlass)
                    {
                        ToggleGallery(true);
                    }
                }
                else if (Commands.isCommand(cmd, CommandMsgType.GalleryClose, out param))
                {
                    if (StatusProperties.Values.IsSmartGlass)
                    {
                        ToggleGallery(false);
                    }
                }
                else if (Commands.isCommand(cmd, CommandMsgType.GallerySwitchToId, out param))
                {
                    if (StatusProperties.Values.IsSmartGlass)
                    {
                        GallerySwitchItem(param);
                    }
                }
                else if (Commands.isCommand(cmd, CommandMsgType.DrawingColor, out param))
                {
                    ReceiveDrawingColor(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.DrawingPoint, out param))
                {
                    ReceiveDrawingPoint(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.ParticleAnnotationType, out param))
                {
                    ReceiveParticleAnnotationType(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.ClearAllParticle, out param))
                {
                    ReceiveClearAllParticle(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.StopParticleAnnotation, out param))
                {
                    ReceiveStopParticleAnnotation(param);
                }
                else if (Commands.isCommand(cmd, CommandMsgType.ARFieldOfView, out param))
                {
                    ReceiveARFieldOfView(param);
                }
            }
        }
        else
        {
            ReceiveChatMessage(msg);
        }
    }

    /// <summary>
    /// A new chat message was received. The corresponding workflow is triggered. 
    /// </summary>
    /// <param name="msg"></param>
    public virtual void ReceiveChatMessage(string msg)
    {
        EventNameManager.SendEventAppend(msg);
        ChatManager.Instance.MessageHint(msg);
    }

    /// <summary>
    /// open / close the gallery
    /// </summary>
    /// <param name="open"></param>
    protected virtual void ToggleGallery(bool open)
    {
        CallSettings.Instance.OnAnchorGalleryToggle(open, true);
    }

    /// <summary>
    /// switch to gallery-item with a specific id
    /// </summary>
    /// <param name="param"></param>
    protected virtual void GallerySwitchItem(string param)
    {
        var p = param.Split(';');
        if (p.Length == 1)
        {
            if (int.TryParse(p[0], out int id))
            {
                AnchorGalleryDetailManagerClient.Instance.GoToId(id);
            }
        }
    }

    /// <summary>
    /// Handle status command message video size or device orientation changed.
    /// </summary>
    /// <param name="param">new video size</param>
    protected virtual void ReceiveVideoSize(string param)
    {
        var coord = Commands.parseCoordinates(param);
        setVideoSize((int)coord.x, (int)coord.y);

        ResolutionManager.Instance.ChangeLResolution((int)coord.x, (int)coord.y);
    }

    /// <summary>
    /// Handle status command message mouse down to create an anchor point.
    /// </summary>
    /// <param name="param">normalized mouse position in actual camera frame</param>
    protected virtual void ReceiveMouseDown(string param)
    {
        var paramList = param.Split(';');
        var coord = Commands.parseCoordinates(paramList[0]);
        coord *= new Vector2(Screen.width, Screen.height);

        float drawingAreaScale = 1;

        if (paramList.Length > 2) drawingAreaScale = Commands.parseFloat(paramList[2]);

        EventNameManager.SendEventARAnchor(new Vector2Int((int)coord.x, (int)coord.y), drawingAreaScale);
    }

    /// <summary>
    /// Handle status command message image data. Show as string encoded image hint from expert application.
    /// </summary>
    /// <param name="param">string encoded image</param>
    protected virtual void ReceiveImageData(string param)
    {
        ChatManager.Instance.AppendCommand(CommandMsgType.ImageData, "ReceiveData: " + param.Substring(0, Math.Min(100, param.Length - 1)));
    }

    /// <summary>
    /// Handle status command message anchor state. Was the anchor point command successful?
    /// </summary>
    /// <param name="param">anchor state</param>
    protected virtual void ReceiveAnchorState(string param)
    {
    }

    /// <summary>
    /// Handle status command message anchor state. Was the anchor point command successful?
    /// </summary>
    /// <param name="param">anchor state</param>
    protected virtual void ReceiveAnchorId(string param)
    {
    }

    /// <summary>
    /// Handle status command message data received. Could the data be received?
    /// </summary>
    /// <param name="param">data send state</param>
    protected virtual void ReceiveDataReceived(string param)
    {
        int id;
        if (int.TryParse(param, out id))
        {
            CommunicationMonitor.DataReceiveCompletet(id);
        }
    }

    /// <summary>
    /// Handle status command message video frame received. Could the video data frame be received?
    /// </summary>
    /// <param name="param">video frame size</param>
    protected virtual void ReceiveVideoFrameReceived(string param)
    {
        captureDevice.RemoteVideoFrameReceived();
    }

    /// <summary>
    /// Handle status command message delete anchor. Delete the last anchor point.
    /// </summary>
    /// <param name="param">anchor id</param>
    protected virtual void ReceiveDeleteAnchor(string param)
    {
    }

    /// <summary>
    /// Handle status command message cancel anchor. Delete the last anchor point.
    /// </summary>
    /// <param name="param">anchor id</param>
    protected virtual void ReceiveCancelAnchor(string param)
    {
    }

    /// <summary>
    /// Handle status command message bandwidth options.
    /// </summary>
    /// <param name="param">; separated bandwidth options. possible parameters Quality=;FPS=;Mode=</param>
    protected virtual void ReceiveBandwidthOptions(string param)
    {
        var settings = SearchHelper.FindSceneObjectOfType<BandwidthManager>();
        if (settings) settings.setBandwithOptions(param);
    }

    /// <summary>
    /// Handle status command message call for anchor data. Send back AnchorImageData.
    /// </summary>
    /// <param name="param">anchor id</param>
    protected virtual void ReceiveCallForAnchorData(string param, int relativIndex = 0, bool needSnapshot = false)
    {
    }

    /// <summary>
    /// Handle status command message call for gallery items. Send back a list of anhorIds.
    /// </summary>
    /// <param name="param"></param>
    protected virtual void ReceiveCallForGalleryItmes(string param)
    {

    }

    /// <summary>
    /// Handle status command message gallery items. Get a list of anhorIds.
    /// </summary>
    /// <param name="param"></param>
    protected virtual void ReceiveGalleryItmes(string param)
    {

    }

    /// <summary>
    /// Expert selected a specific plane to draw on
    /// </summary>
    /// <param name="param">plane and projector transform</param>
    protected virtual void ReceiveProjectionPlaneSelected(string param)
    {

    }

    /// <summary>
    /// Expert chances AR mode
    /// </summary>
    /// <param name="param">true: AR is active / false: webcam is active and AR annotations inactive</param>
    protected virtual void ReceiveARMode(string param)
    {

    }

    /// <summary>
    /// Client tells installed projection mode to expert. Both must handle annotations with the same mode
    /// </summary>
    /// <param name="param">projection mode: enum CalculationMode</param>
    protected virtual void ReceiveProjectionMode(string param)
    {

    }

    /// <summary>
    /// Client tells expert if AR mode is supported from the app installation.
    /// </summary>
    /// <param name="param">projection mode: enum CalculationMode</param>
    protected virtual void ReceiveSupportsARMode(string param)
    {

    }

    /// <summary>
    /// Client tells expert if device is a smart glass.
    /// </summary>
    /// <param name="param">projection mode: enum CalculationMode</param>
    protected virtual void ReceiveIsSmartGlassCommunication(string param)
    {
    }

    /// <summary>
    /// Other devices created an new gallery item.
    /// </summary>
    /// <param name="param">projection mode: enum CalculationMode</param>
    public virtual void ReceiveNewGalleryItem(string param)
    {
        if (newGalleryItemSnackbar)
            newGalleryItemSnackbar.gameObject.SetActive(true);

        if (!StatusProperties.Values.ARActive)
        {
            int anchorId = -1;
            if (int.TryParse(param, out anchorId))
            {
                LiveviewItem.SetAllItemsActive(false);
                AnchorGalleryDetailManager.Instance.gameObject.SetActive(true);
                AnchorGalleryDetailManager.Instance.GetGalleryItem(anchorId, (StatusProperties.Values.IsSmartGlass ? 0 : -1));
            }
        }
    }

    /// <summary>
    /// Handle status command message no anchor data found.
    /// Server send command call for anchor data. But there are no anchors on the client. Client answers with no anchor data found.
    /// </summary>
    /// <param name="param">; separated bandwidth options. possible parameters Quality=;FPS=;Mode=</param>
    protected virtual void ReceiveNoAnchorDataFound(string param)
    {
    }

    /// <summary>
    /// Handle status command message resend block. Blocks get lost by network communication.
    /// When sending data over the data channel from webrtc there is maximum limit of data size. The image size must not exceed the maximum byte count for data transmission.
    /// To avoid this limit the full resolution texture will be send in data blocks.
    /// </summary>
    /// <param name="param">anchor id and / separated list of block indexes which should be resend</param>
    protected virtual void ReceiveResendBlock(string param)
    {
        var args = param.Split('/').Select(x => int.Parse(x)).ToArray();
        var id = args[0];
        StartCoroutine(ResendDataInPackages(id, args.SubArray(1, args.Length - 1)));
    }

    /// <summary>
    /// Handle status command message convert 2d screen mouse position to 3d marker
    /// </summary>
    /// <param name="param">; separated bandwidth options. possible parameters viewPointCoord;drawingAreaScale</param>
    protected virtual void ReceiveConvertInto3DMousePosition(string param)
    {
    }

    /// <summary>
    /// Handle status command message StatusProperty
    /// </summary>
    /// <param name="param">PropertyFlag;bool value</param>
    protected virtual void ReceiveStatusProperty(string param)
    {
        var values = param.Split(';');
        if (values.Length >= 1)
        {
            PropertyFlag flag;
            if (Enum.TryParse<PropertyFlag>(values[0], out flag))
            {
                bool value = true;
                if (values.Length >= 2)
                {
                    bool.TryParse(values[1], out value);
                }
                StatusProperties.Values.SetKey(flag, value, false);
            }
        }
    }

    /// <summary>
    /// Handle status command message DeviceResolution
    /// </summary>
    /// <param name="param">coordinates string</param>
    protected virtual void ReceiveDeviceResolution(string param)
    {
        var res = Commands.parseCoordinates(param);
        ResolutionManager.Instance.ChangeLResolution((int)res.x, (int)res.y);
    }

    /// <summary>
    /// Handle status command message DrawingColor
    /// </summary>
    /// <param name="param">coordinates string</param>
    protected virtual void ReceiveDrawingColor(string param)
    {
    }

    /// <summary>
    /// Handle status command message DrawingPoint
    /// </summary>
    /// <param name="param">coordinates string</param>
    protected virtual void ReceiveDrawingPoint(string param)
    {
    }

    /// <summary>
    /// Handle status command message ParticleAnnotationType
    /// </summary>
    /// <param name="param">ParticleAnnotationType</param>
    protected virtual void ReceiveParticleAnnotationType(string param)
    {

    }

    /// <summary>
    /// Handle status command message ClearAllParticle
    /// </summary>
    /// <param name="param">none</param>
    protected virtual void ReceiveClearAllParticle(string param)
    {

    }

    /// <summary>
    /// Handle status command message StopParticleAnnotation
    /// </summary>
    /// <param name="param">none</param>
    protected virtual void ReceiveStopParticleAnnotation(string param)
    {

    }

    /// <summary>
    /// Handle status command message ARFieldOfView
    /// </summary>
    /// <param name="param">none</param>
    protected virtual void ReceiveARFieldOfView(string param)
    {

    }
    #endregion

    #region video chat
    /// <summary>
    /// Set the name of the video device which data should be send to the other devices.
    /// </summary>
    public virtual void SetActiveVideoDevice()
    {
    }

    /// <summary>
    /// Set the name of the video device which data should be send to the other devices.
    /// </summary>
    /// <param name="videoDevName">device name</param>
    public virtual void SetActiveVideoDevice(string videoDevName)
    {
    }

    /// <summary>
    /// Set the size of the video device which data should be send to the other devices.
    /// </summary>
    public virtual void SetActiveVideoDeviceSize()
    {
        if (captureDevice)
        {
            SendCommandMsg(new CommandMsg(CommandMsgType.VideoSize, captureDevice.VideoSizeCommand));
        }
    }

    /// <summary>
    /// Updates the remote video. If the frame is null it will hide the video image.
    /// </summary>
    /// <param name="frame">frame image</param>
    /// <param name="format">frame format</param>
    public virtual void UpdateRemoteTexture(IFrame frame, FramePixelFormat format)
    {
    }

    /// <summary>
    /// Send PNG encoded image byte array to devices
    /// </summary>
    /// <param name="data">PNG encoded image byte array</param>
    private void SendDataEvent(byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            //never send null or empty messages. webrtc can't deal with that
            return;
        }

        if (useRemoteCommunication)
        {
            //The byte array may not exceed the maximum size for the data transmission when sending.
            CallAppBackend.Instance.Send(data);
        }
        else
        {
            ReceiveData(data);
        }
    }

    /// <summary>
    /// Convert byte array to string.
    /// </summary>
    /// <param name="data">image byte array</param>
    private void encodeDataAsText(byte[] data)
    {
        string base64String = Convert.ToBase64String(data, 0, data.Length);
        int blockSize = 50000;

        if (base64String.Length > blockSize)
        {
            for (int i = 0; i < base64String.Length; i += blockSize)
            {
                var msgPart = base64String.Substring(i, Math.Min(blockSize, base64String.Length - i));
                SendCommandMsg(new CommandMsg(CommandMsgType.ImageData, msgPart, (i / blockSize)));
            }
        }
        else
        {
            SendCommandMsg(new CommandMsg(CommandMsgType.ImageData, base64String));
        }
    }

    /// <summary>
    /// Receive PNG encoded image byte array from an other device.
    /// </summary>
    /// <param name="data">PNG encoded image byte array</param>
    public void ReceiveData(byte[] data)
    {
        var dataPackage = data.Deserialize<CommunicationData>();
        if (CommunicationMonitor.DataReceived(dataPackage))
        {
            SendCommandMsg(new CommandMsg(CommandMsgType.DataReceived, dataPackage.ID.ToString()));
            DataReceived(CommunicationMonitor.GetReceivedData(dataPackage.ID));
        }
    }

    /// <summary>
    /// Receive PNG encoded image byte array from an other device.
    /// </summary>
    /// <param name="data">PNG encoded image byte array</param>
    protected virtual void DataReceived(byte[] data)
    {
    }


    /// <summary>
    /// change the display video size to the new device orientation of the sender device.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    protected virtual void setVideoSize(int width, int height)
    {
    }
    #endregion

    #region send and receive
    /// <summary>
    /// resize the source texture.
    /// When sending data over the data channel from webrtc there is maximum limit of data size. The image size must not exceed the maximum byte count for data transmission.
    /// </summary>
    /// <param name="source">texture which should be resized</param>
    /// <param name="newWidth">target texture width</param>
    /// <param name="newHeight">target texture height</param>
    /// <returns></returns>
    public Texture2D Resize(Texture source, int newWidth, int newHeight)
    {
        var oldActiveTexture = RenderTexture.active;
        var oldFilterMode = source.filterMode;

        source.filterMode = FilterMode.Point;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Point;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        var nTex = new Texture2D(newWidth, newHeight);
        nTex.ReadPixels(new Rect(0, 0, newWidth, newWidth), 0, 0, false);
        nTex.Apply();

        RenderTexture.active = oldActiveTexture;
        source.filterMode = oldFilterMode;
        return nTex;
    }
    
    /// <summary>
    /// When sending data over the data channel from webrtc there is maximum limit of data size. The image size must not exceed the maximum byte count for data transmission.
    /// To avoid this limit and display fast results the texture is resized to a small preview image of the maximum size which could be send at once.
    /// In a further step the full resolution texture will be send in data blocks.
    /// </summary>
    /// <param name="source">source texture which should be resized</param>
    /// <returns>reduced preview texture of the maximum size which could be send at once.</returns>
    public Texture2D ResizeToOneBlockSize(Texture2D source)
    {
        var sourceArray = ImageHelper.convertToByteArray(source);

        if (sourceArray.Length > CommunicationConstants.MaxDataSizeBlock)
        {
            var scaleFactor = Mathf.Ceil(Mathf.Sqrt(sourceArray.Length / (float)CommunicationConstants.MaxDataSizeBlock) * 1.3f);
            var previewWidth = (int)Mathf.Floor(source.width / scaleFactor);
            var previewHeight = (int)Mathf.Floor(source.height / scaleFactor);
            var preview = Resize(source, previewWidth, previewHeight);
            return preview;
        }
        return null;
    }

    /// <summary>
    /// When sending data over the data channel from webrtc there is maximum limit of data size. The image size must not exceed the maximum byte count for data transmission.
    /// To avoid this limit the full resolution texture will be send in data blocks.
    /// </summary>
    /// <param name="source">source texture which should be send in blocks</param>
    /// <param name="anchorId">connected anchor id to the texture</param>
    /// <param name="pivot">pivot point of the annotation drawing</param>
    /// <param name="clickPoint">click point on the snapshot image</param>
    /// <param name="blockIndex">list of block indexes which should be send</param>
    /// <returns></returns>
    protected IEnumerator SendDataBlocks(Texture2D source, int anchorId, Vector2 pivot, Vector2 clickPoint, int[] blockIndex = null)
    {
        if (blockIndex == null || blockIndex.Length == 0)
        {
            //To display fast results the texture is resized to a small preview image of the maximum size which could be send at once.
            var preview = ResizeToOneBlockSize(source);
            if (preview != null)
            {
                var previewArray = ImageHelper.convertToByteArray(preview);
                yield return StartCoroutine(SendDataInPackages(new AnchorImageData(anchorId, pivot, clickPoint, previewSnapshot: previewArray).SerializeToByteArray()));
            }
        }

        //In a further step the full resolution texture will be send in data blocks.
        var data = ImageHelper.convertToByteArray(source);

        var imageData = new AnchorImageData(anchorId, pivot, clickPoint, snapshot: data);
        yield return StartCoroutine(SendDataInPackages(imageData.SerializeToByteArray()));

        yield return 0;
    }

    /// <summary>
    /// The anchor point data is transmitted from the mobile device on site to the expert.
    /// </summary>
    /// <param name="data">anchor point data</param>
    public void SendData(AnchorImageData data)
    {
        StartCoroutine(SendDataInPackages(data.SerializeToByteArray()));
    }

    /// <summary>
    /// The annotation data is transferred from the expert to the worker on site.
    /// </summary>
    /// <param name="data">annotation</param>
    public void SendData(AnnotationImageData data)
    {
        StartCoroutine(SendDataInPackages(data.SerializeToByteArray()));
    }

    /// <summary>
    /// When sending data over the data channel from webrtc there is maximum limit of data size. The image size must not exceed the maximum byte count for data transmission.
    /// The data must therefore be transmitted in blocks.
    /// </summary>
    /// <param name="data">data to be transmitted</param>
    /// <returns></returns>
    protected IEnumerator SendDataInPackages(byte[] data)
    {
        var blocks = CommunicationMonitor.AddToQueue(data);
        yield return StartCoroutine(SendDataInPackages(blocks));
        yield return 0;
    }

    /// <summary>
    /// When sending data over the data channel from webrtc there is maximum limit of data size. The image size must not exceed the maximum byte count for data transmission.
    /// The data must therefore be transmitted in blocks. It can happen that individual blocks are lost. These blocks must therefore be retransmitted.
    /// </summary>
    /// <param name="id">unique data id</param>
    /// <param name="index">block indexes, which are to be retransmitted</param>
    /// <returns></returns>
    protected IEnumerator ResendDataInPackages(int id, int[] index)
    {
        var blocks = CommunicationMonitor.GetDataPackages(id, index);
        yield return StartCoroutine(SendDataInPackages(blocks));
        yield return 0;
    }

    /// <summary>
    /// When sending data over the data channel from webrtc there is maximum limit of data size. The image size must not exceed the maximum byte count for data transmission.
    /// The data must therefore be transmitted in blocks.
    /// </summary>
    /// <param name="blocks">data blocks to be sent</param>
    /// <returns></returns>
    protected IEnumerator SendDataInPackages(CommunicationData[] blocks)
    {
        foreach (var block in blocks)
        {
            var data = block.SerializeToByteArray();
            EventNameManager.SendEventData(data);
            yield return new WaitForSeconds(0.05f);
        }
        yield return 0;
    }
    #endregion
}
