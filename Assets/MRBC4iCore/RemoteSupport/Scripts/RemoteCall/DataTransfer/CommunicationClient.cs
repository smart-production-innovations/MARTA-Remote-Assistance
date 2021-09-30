using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

/// <summary>
/// manage the video and status command communication between the devices for the client device
/// </summary>
public class CommunicationClient : CommunicationManager
{
    public GameObject MousePosition3DMarker;
    private float showMousePosition3DMarkerTime = 0;
    private float fallBackDistance = 0;

    #region unity loop
    private void Start()
    {
        // When starting the application, the remote device reports its technical requirements to the expert application. 
        // This enables the same expert application to be used for different device types. 
        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.CalculationMode, StatusProperties.Values.CalculationMode.ToString()));
        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.SupportsARMode, StatusProperties.Values.SupportsARMode.ToString()));
        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.IsSmartGlassCommunication, StatusProperties.Values.IsSmartGlassCommunication.ToString()));
        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.DeviceResolution, Commands.getCoordinatesString(ResolutionManager.Instance.lWidth, ResolutionManager.Instance.lHeight)));
        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.ARFieldOfView, Commands.getFloatString(StatusProperties.Values.FieldOfView)));
    }

    private void Update()
    {
        MousePositionCalculation.updatePointer(MousePosition3DMarker, Time.time - showMousePosition3DMarkerTime);
    }
    #endregion

    #region message commands
    /// <summary>
    /// Handle status command message mouse down on the client to create an anchor point.
    /// Delayed by one frame, so the saved image does not contain the MousePosition3DMarker.
    /// </summary>
    /// <param name="param">normalized mouse position in actual camera frame</param>
    protected override void ReceiveMouseDown(string param)
    {
        MousePositionCalculation.hidePointer(MousePosition3DMarker);

        StartCoroutine(ReceiveMouseDownDelayed(() =>
        {
            base.ReceiveMouseDown(param);
        }));
    }

    /// <summary>
    /// Execute an Action delayed by one frame.
    /// </summary>
    /// <param name="delayedAction">the action which should execute delayed</param>
    /// <returns></returns>
    private IEnumerator ReceiveMouseDownDelayed(Action delayedAction)
    {
        yield return null;
        delayedAction();
    }

    /// <summary>
    /// Handle status command message delete anchor. Delete the last anchor point.
    /// </summary>
    /// <param name="param">anchor id</param>
    protected override void ReceiveDeleteAnchor(string param)
    {
        base.ReceiveDeleteAnchor(param);
        var args = param.Split('/');

        int id;
        if (int.TryParse(args[0], out id))
            AnnotationManager.Instance.RemovedAnchor(id);
        else
            AnnotationManager.Instance.RemoveSelectedAnchor();

        if (args.Length > 1 && args[1] == CommandMsgType.CallForGalleryItems.ToString())
            ReceiveCallForGalleryItmes("");
    }

    /// <summary>
    /// Handle status command message cancel anchor. Delete the last anchor point.
    /// </summary>
    protected override void ReceiveCancelAnchor(string param)
    {
        base.ReceiveCancelAnchor(param);
        int id;
        if (int.TryParse(param, out id))
            AnnotationManager.Instance.CancelDrawing(id);
        else
            AnnotationManager.Instance.CancelDrawing();
    }

    /// <summary>
    /// Handle status command message call for anchor data. Send back AnchorImageData.
    /// </summary>
    /// <param name="param">anchor id</param>
    protected override void ReceiveCallForAnchorData(string param, int relativIndex = 0, bool needSnapshot = false)
    {
        base.ReceiveCallForAnchorData(param, relativIndex, needSnapshot);

        int id;
        AnchorImage anchor = null;
        if (int.TryParse(param, out id))
        {
            anchor = AnnotationManager.Instance.getAnchor(id, relativIndex);
        }

        if (anchor == null)
        {
            anchor = AnnotationManager.Instance.getFirstAnchor();
        }

        if (anchor)
        {
            var iAnchor = anchor as IAnchorAnnotationBase;

            if (StatusProperties.Values.IsSmartGlass)
            {
                AnchorGalleryDetailManagerClient.Instance.GetGalleryItem(anchor.Anchor.Id);
            }

            var clickPoint = Vector2.zero;
            if (anchor is AnchorAnnotationDistanceScale)
                clickPoint = ((AnchorAnnotationDistanceScale)anchor).RelativeScreenClickCenterPivotPosition;

            List<ARLayerPlane> planeList = null;
            var cameraTransform = new TransformParameter(CameraHelper.ARCamera.transform);
            if (anchor is AnchorAnnotationProjection3D)
            {
                var anchorProjection = ((AnchorAnnotationProjection3D)anchor);
                planeList = anchorProjection.DetectedPlanes;
                cameraTransform.Position = anchorProjection.ProjectorPosition;
                cameraTransform.Rotation = anchorProjection.ProjectorRotation;
            }

            if (needSnapshot)
            {
                //Send the connected snapshot for the annotation drawing to the server.
                //The image size must not exceed the maximum byte count for data transmission.
                //Send snapshot in blocks to avoid maximum size limit.
                StartCoroutine(SendDataBlocks(iAnchor.GetSnapshot().SnapshotTexture, anchor.Anchor.Id, anchor.GetPivot(), clickPoint));
            }
            else
            {
                //send annotation drawing to the server
                SendData(new AnchorImageData(anchor.Anchor.Id, anchor.GetPivot(), clickPoint,
                        imageData: ImageHelper.convertToByteArray(anchor.GetTexture()), 
                        planeList: planeList, cameraTransform: cameraTransform, anchorTransform: anchor.transform));
            }
        }
        else
        {
            EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.NoAnchorDataFound, ""));
        }
    }

    /// <summary>
    /// Handle status command message call for gallery items. Send back a list of anhorIds.
    /// </summary>
    /// <param name="param"></param>
    protected override void ReceiveCallForGalleryItmes(string param)
    {
        base.ReceiveCallForGalleryItmes(param);

        var anchors = AnchorPointManager.Instance.GetAllAnchorPoints();
        var anchorIds = anchors.Select(x => x.Id).ToList();
        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.GalleryItems, anchorIds.getArrayString()));
    }

    /// <summary>
    /// Handle status command message convert 2d screen mouse position to 3d marker
    /// </summary>
    /// <param name="param">; separated bandwidth options. possible parameters viewPointCoord;drawingAreaScale</param>
    protected override void ReceiveConvertInto3DMousePosition(string param)
    {
        base.ReceiveConvertInto3DMousePosition(param);

        var paramList = param.Split(';');
        var viewPointCoord = Commands.parseCoordinates(paramList[0]);
        var distance = MousePositionCalculation.setNewPointerPosition(MousePosition3DMarker, viewPointCoord, fallBackDistance);
        if (distance > 0)
        {
            showMousePosition3DMarkerTime = Time.time;
            fallBackDistance = distance;
        }
    }

    /// <summary>
    /// Expert selected a specific plane to draw on
    /// </summary>
    /// <param name="param">plane and projector transform</param>
    protected override void ReceiveProjectionPlaneSelected(string param)
    {
        base.ReceiveProjectionPlaneSelected(param);
        var pList = param.Split(';');
        if (pList.Length == 6)
        {
            int id;
            if (int.TryParse(pList[0], out id))
            {
                var anchor = AnnotationManager.Instance.getAnchor(id);

                if (anchor && anchor is AnchorAnnotationProjection3D)
                {
                    var anchorProjection = (AnchorAnnotationProjection3D)anchor;
                    bool setToPoint = true;

                    bool defaultLayer = false;
                    if (bool.TryParse(pList[1], out defaultLayer))
                    {
                        if (!defaultLayer)
                        {
                            var planPos = Commands.parseVector3(pList[2]);
                            var planRot = Commands.parseVector3(pList[3]);
                            anchorProjection.SetPlane(planPos, planRot);
                            setToPoint = false;
                        }
                    }
                    
                    if (setToPoint)
                    {
                        anchorProjection.SetProjectionTarget(ProjectionTarget.Point);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Expert chances AR mode
    /// </summary>
    /// <param name="param">true: AR is active / false: webcam is active and AR annotations inactive</param>
    protected override void ReceiveARMode(string param)
    {
        base.ReceiveARMode(param);

        bool active = true;
        if (bool.TryParse(param, out active))
        {
            StatusProperties.Values.ARActive = active;
        }
    }

    /// <summary>
    /// Handle status command message DrawingColor
    /// </summary>
    /// <param name="param">coordinates string</param>
    protected override void ReceiveDrawingColor(string param)
    {
        base.ReceiveDrawingColor(param);
        var color  = Commands.parseColor(param);

        if (MousePosition3DMarker)
        {
            MousePositionCalculation.setColor(MousePosition3DMarker, color);
        }
    }

    /// <summary>
    /// Handle status command message DrawingPoint
    /// </summary>
    /// <param name="param">coordinates string</param>
    protected override void ReceiveDrawingPoint(string param)
    {
        base.ReceiveDrawingPoint(param);
    }

    /// <summary>
    /// Handle status command message ParticleAnnotationType
    /// </summary>
    /// <param name="param">ParticleAnnotationType</param>
    protected override void ReceiveParticleAnnotationType(string param)
    {
        base.ReceiveParticleAnnotationType(param);
        var t = (ParticleAnnotationType)Enum.Parse(typeof(ParticleAnnotationType), param);
        MousePositionCalculation.setType(MousePosition3DMarker, t);
    }

    /// <summary>
    /// Handle status command message ClearAllParticle
    /// </summary>
    /// <param name="param">none</param>
    protected override void ReceiveClearAllParticle(string param)
    {
        base.ReceiveClearAllParticle(param);
        MousePositionCalculation.clear(MousePosition3DMarker);
    }

    /// <summary>
    /// Handle status command message StopParticleAnnotation
    /// </summary>
    /// <param name="param">none</param>
    protected override void ReceiveStopParticleAnnotation(string param)
    {
        base.ReceiveStopParticleAnnotation(param);
        MousePositionCalculation.hidePointer(MousePosition3DMarker, immediate: false);
    }
    #endregion

    #region video chat
    /// <summary>
    /// Set the name of the video device which data should be send to the other devices.
    /// </summary>
    public override void SetActiveVideoDevice()
    {
        base.SetActiveVideoDevice();
        if (captureDevice)
        {
            var videoDevName = captureDevice.CallDeviceName;
            SetActiveVideoDevice(videoDevName);
        }
    }

    /// <summary>
    /// Set the name of the video device which data should be send to the other devices.
    /// </summary>
    /// <param name="videoDevName">device name</param>
    public override void SetActiveVideoDevice(string videoDevName)
    {
        base.SetActiveVideoDevice(videoDevName);
        CallAppBackend.Instance.SetVideoDevice(videoDevName);
    }

    /// <summary>
    /// Receive PNG encoded image byte array from an other device.
    /// </summary>
    /// <param name="data">PNG encoded image byte array</param>
    protected override void DataReceived(byte[] data)
    {
        base.DataReceived(data);

        var annotationData = data.Deserialize<AnnotationImageData>();

        EventNameManager.SendEventImageToAnchor(annotationData);
    }
#endregion
}
