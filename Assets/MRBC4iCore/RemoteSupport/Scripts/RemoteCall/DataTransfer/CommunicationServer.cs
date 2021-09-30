using UnityEngine;
using System.Collections;
using Byn.Awrtc;
using UnityEngine.UI;
using Byn.Awrtc.Unity;
using System.Linq;
using System;

/// <summary>
/// manage the video and status command communication between the devices for the expert device
/// </summary>
public class CommunicationServer : CommunicationManager
{
    #region properties
    [Header("Resources")]
    public Texture2D uNoCameraTexture;

    private bool drawingRemoteManagerWaitForSnapshot = false;

    /// <summary>
    /// Texture of the remote video
    /// </summary>
    protected Texture2D mRemoteVideoTexture = null;

    protected VideoSize[] videos;
    protected VideoStream[] videoStreams;
    /// <summary>
    /// List of all video display UI elements. Several UI elements can display the video (default video display and reduced video display in the drawing view).
    /// </summary>
    protected VideoStream[] VideoStreams
    {
        get
        {
            if (videoStreams == null)
                videoStreams = SearchHelper.FindSceneObjectsOfTypeAll<VideoStream>();

            return videoStreams;
        }
    }

    #endregion

    #region loop
    protected override void Awake()
    {
        base.Awake();
        // find all video display UI elements.
        videos = SearchHelper.FindSceneObjectsOfTypeAll<VideoSize>();
        videoStreams = SearchHelper.FindSceneObjectsOfTypeAll<VideoStream>();
    }

    void Update()
    {
    }

    private void OnEnable()
    {
        // If there is more than one preset configuration, the preset selection menu should be opened after the connection is established.
        // Otherwise the preset configuration is loaded automatically.
        if (StatusProperties.Values.Presets.Length > 1)
            PresetOverview.Instance.Show();
        else
            StatusProperties.Values.loadPreset(informDevices: true);

        // initialize the gallery helper for a new session
        AnchorGalleryServerHelpe.initGallerHelper();
    }
    #endregion

    #region message commands
    /// <summary>
    /// Handle status command message anchor state. Was the anchor point command successful?
    /// </summary>
    /// <param name="param">anchor state</param>
    protected override void ReceiveAnchorState(string param)
    {
        base.ReceiveAnchorState(param);
        if (param == AnchorState.Success.ToString())
        {
            DrawingRemoteManager.Instance.DisplayEmptyDrawingArea();
            if (DrawingSettings.Instance.AnnotationType == AnnotationType.Drawing)
            {
                DrawingRemoteManager.InterfaceInstance.HideDrawingOverTime(true);
                EventNameManager.SendEventAnchorCreated();
            }
        }
        else
        {
            ChatManager.Instance.AppendStatus("Anchor could not be set. Smartphone user has to scan environment. Or click on yellow feature points.");
            EventNameManager.SendEventShowARHelper("ScanEnvironment");
        }
    }


    /// <summary>
    /// Handle status command message anchor state. Was the anchor point command successful?
    /// </summary>
    /// <param name="param">anchor state</param>
    protected override void ReceiveAnchorId(string param)
    {
        base.ReceiveAnchorId(param);
        int id;
        if (int.TryParse(param, out id))
        {
            ReceiveAnchorId(id);

            if (DrawingManager.InterfaceInstance.SnapshotTexture)
                AnchorGalleryServerHelpe.addNewSnapshotToDictionary(id, DrawingRemoteManager.makeSnapshot(DrawingManager.InterfaceInstance.SnapshotTexture));
        }
    }

    /// <summary>
    /// note anchor id which is currently being processed
    /// </summary>
    /// <param name="id">anchor id which is currently being processed</param>
    private void ReceiveAnchorId(int id)
    {
        RemoteHelperImage.AnchorId = id;
    }

    /// <summary>
    /// Handle status command message no anchor data found.
    /// Server send command call for anchor data. But there are no anchors on the client. Client answers with no anchor data found.
    /// </summary>
    /// <param name="param">; separated bandwidth options. possible parameters Quality=;FPS=;Mode=</param>
    protected override void ReceiveNoAnchorDataFound(string param)
    {
        base.ReceiveNoAnchorDataFound(param);

        if (CallSettings.HasInstance)
            CallSettings.Instance.LeaveGallery(true);
    }

    /// <summary>
    /// Handle status command message gallery items. Get a list of anhorIds.
    /// </summary>
    /// <param name="param"></param>
    protected override void ReceiveGalleryItmes(string param)
    {
        base.ReceiveGalleryItmes(param);

        var list = Commands.parseArray<int>(param);
        AnchorGalleryOverviewManagerServer.ServerInstance.LoadGalleryPreviewItems(list);
    }

    /// <summary>
    /// Client tells installed projection mode to expert. Both must handle annotations with the same mode
    /// </summary>
    /// <param name="param">projection mode: enum CalculationMode</param>
    protected override void ReceiveProjectionMode(string param)
    {
        base.ReceiveProjectionMode(param);

        CalculationMode mode;
        if (Enum.TryParse<CalculationMode>(param, out mode))
        {
            StatusProperties.Values.CalculationMode = mode;
        }
    }

    /// <summary>
    /// Client tells expert if AR mode is supported from the app installation.
    /// </summary>
    /// <param name="param">projection mode: enum CalculationMode</param>
    protected override void ReceiveSupportsARMode(string param)
    {
        base.ReceiveSupportsARMode(param);

        bool isSupported;
        if (bool.TryParse(param, out isSupported))
        {
            StatusProperties.Values.SupportsARMode = isSupported;
        }
    }

    /// <summary>
    /// Client tells expert if device is a smart glass.
    /// </summary>
    /// <param name="param">projection mode: enum CalculationMode</param>
    protected override void ReceiveIsSmartGlassCommunication(string param)
    {
        base.ReceiveIsSmartGlassCommunication(param);

        bool value;
        if (bool.TryParse(param, out value))
        {
            StatusProperties.Values.IsSmartGlassCommunication = value;
        }
    }

    /// <summary>
    /// Handle status command message ARFieldOfView
    /// </summary>
    /// <param name="param">none</param>
    protected override void ReceiveARFieldOfView(string param)
    {
        var FoV = Commands.parseFloat(param);
        var cameras = SearchHelper.FindSceneObjectsOfTypeAll<Camera>(true);
        foreach (var cam in cameras)
        {
            cam.fieldOfView = FoV;
        }
    }
    #endregion

    #region video chat
    /// <summary>
    /// Updates the remote video. If the frame is null it will hide the video image.
    /// </summary>
    /// <param name="frame">frame image</param>
    /// <param name="format">frame format</param>
    public override void UpdateRemoteTexture(IFrame frame, FramePixelFormat format)
    {
        base.UpdateRemoteTexture(frame, format);

        if (frame != null)
        {
            UnityMediaHelper.UpdateTexture(frame, ref mRemoteVideoTexture);
        }


        foreach (var video in VideoStreams)
        {
            var videoImage = video.GetComponent<RawImage>();
            if (frame != null)
            {
                videoImage.texture = mRemoteVideoTexture;
                //watch out: due to conversion from WebRTC to Unity format the image is flipped (top to bottom)
                //this also inverts the rotation
                videoImage.transform.rotation = Quaternion.Euler(0, 0, frame.Rotation * -1);
                StatusProperties.Values.VideoTransmissionStarted = true;
            }
            else
            {
                videoImage.texture = uNoCameraTexture;
                videoImage.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }

        if (frame != null)
        {
            SendCommandMsg(new CommandMsg(CommandMsgType.VideoFrameReceived, frame.Buffer.Length.ToString()));

            if (drawingRemoteManagerWaitForSnapshot)
            {
                AnchorGalleryServerHelpe.DisplayLoadDataView(-1);
                drawingRemoteManagerWaitForSnapshot = false;
            }

        }
    }

    /// <summary>
    /// change the display video size to the new device orientation of the sender device.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    protected override void setVideoSize(int width, int height)
    {
        base.setVideoSize(width, height);

        foreach (var video in videos)
        {
            video.ChangeImageSize(width, height);
        }
    }

    /// <summary>
    /// Receive PNG encoded image byte array from an other device.
    /// </summary>
    /// <param name="data">PNG encoded image byte array</param>
    protected override void DataReceived(byte[] data)
    {
        base.DataReceived(data);
        AnchorImageData anchorData = null;

        try
        {
            // convert byte array back to date structure
            anchorData = data.Deserialize<AnchorImageData>();
        }
        catch (System.Exception e)
        {
        }

        if (anchorData == null)
        {
            return;
        }

        bool displaySet = false;

        // display annotation
        if (anchorData.imageData != null && anchorData.imageData.Length > 0)
        {
            Texture2D annotation = ImageHelper.convertToTexture(anchorData.imageData);
            AnchorGalleryServerHelpe.DisplayReceivedData(anchorData.AnchorId, anchorData.Pivot, anchorData.ClickPoint, annotation: annotation);
            displaySet = true;
        }

        // display snapshot
        if (anchorData.snapshot != null && anchorData.snapshot.Length > 0)
        {
            Texture2D snapshot = ImageHelper.convertToTexture(anchorData.snapshot);
            AnchorGalleryServerHelpe.DisplayReceivedData(anchorData.AnchorId, anchorData.Pivot, anchorData.ClickPoint, snapshot: snapshot);
            AnchorGalleryServerHelpe.saveSnapshotToDictionary(anchorData.AnchorId, snapshot);
            displaySet = true;
        }
        // show reduced preview snapshot
        else if (anchorData.previewSnapshot != null && anchorData.previewSnapshot.Length > 0)
        {
            Texture2D snapshot = ImageHelper.convertToTexture(anchorData.previewSnapshot);
            AnchorGalleryServerHelpe.DisplayReceivedData(anchorData.AnchorId, anchorData.Pivot, anchorData.ClickPoint, snapshot: snapshot);
            AnchorGalleryServerHelpe.savePreviewSnapshotToDictionary(anchorData.AnchorId, snapshot);
            displaySet = true;
        }
        else if (anchorData.imageData != null && anchorData.imageData.Length > 0)
        {
            // search for buffered data
            var snapshot = AnchorGalleryServerHelpe.getSnapshotFromDictionary(anchorData.AnchorId);
            if (snapshot != null)
            {
                // Is the preview snapshot and the snapshot with the full resolution buffered?
                bool needSnapshot = AnchorGalleryServerHelpe.NeedSnapshotData(anchorData.AnchorId);

                if (!needSnapshot)
                {
                    // All required data is cached.
                    AnchorGalleryServerHelpe.DisplayReceivedData(anchorData.AnchorId, anchorData.Pivot, anchorData.ClickPoint, snapshot: snapshot.SnapshotTexture, snapshotOrientation: snapshot.SnapshotOrientation);
                }
                else
                {
                    // Only the preview snapshot is cached.
                    // The snapshot in full resolution must be requested.
                    AnchorGalleryServerHelpe.DisplayReceivedData(anchorData.AnchorId, anchorData.Pivot, anchorData.ClickPoint, snapshot: snapshot.PreviewSnapshotTexture, snapshotOrientation: snapshot.SnapshotOrientation);
                    EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.CallForAnchorSnapshotData, anchorData.AnchorId.ToString()));
                }
                displaySet = true;
            }
            else
            {
                // The snapshot must be requested.
                EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.CallForAnchorSnapshotData, anchorData.AnchorId.ToString()));
                drawingRemoteManagerWaitForSnapshot = true;
            }
        }
        else
        {
            // A new anchor point was successfully created. Display an empty drawing area.
            ReceiveAnchorId(anchorData.AnchorId);
            if (anchorData.HasClickPoint)
            {
                DrawingRemoteManager.Instance.DisplayEmptyDrawingArea(anchorData.ClickPoint);
                EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.CallForAnchorSnapshotData, anchorData.AnchorId.ToString()));
            }
        }

        // List of all AR planes found in the camera field of view. Initialize the calculation of the planes selection images.
        if (anchorData.planList != null && anchorData.planList.Count() > 0)
        {
            ARPlaneDisplayManager.Instance.setDrawingProjectionLocation(anchorData.AnchorPosition, anchorData.AnchorRotation + new Vector3(-90,0,0));
            ARPlaneDisplayManager.Instance.SetPlanes(anchorData.planList);
            ARPlaneDisplayManager.Instance.SetCameraRotation(anchorData.CameraRotation);
            ARPlaneDisplayManager.Instance.SetCameraPosition(anchorData.CameraPosition);
        }

        // As long as the snapshot for the gallery entry has not been transferred completely over the network to the expert, a default illustration is displayed instead of the snapshot.
        if (!displaySet && anchorData.HasClickPoint)
            AnchorGalleryServerHelpe.DisplayReceivedData(anchorData.AnchorId, anchorData.Pivot, anchorData.ClickPoint, snapshot: uNoCameraTexture);
    }
    #endregion
}
