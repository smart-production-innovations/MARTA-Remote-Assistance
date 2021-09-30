using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Callback type when trying to set a new anchor point
/// </summary>
public enum AnchorState
{
    NotFound, //anchor point could not be set because on the click position are no AR features in the camera image
    Success //anchor point creation was successful
}

/// <summary>
/// Calculate anchor points for the coordinates send from the expert application
/// </summary>
public class AnchorPointSynchronisation : MonoBehaviour
{

    void Awake()
    {
        ActionEventManager.Subscribe<byte[]>(EventName.SetImageToLastAnchor, SetImageToLastAnchor);
        ActionEventManager.Subscribe<AnnotationImageData>(EventName.SetImageToAnchor, SetImageToAnchor);
        ActionEventManager.Subscribe<ARAnchorData>(EventName.SetARAnchor, values);
    }

    void OnDestroy()
    {
        ActionEventManager.Unsubscribe<byte[]>(EventName.SetImageToLastAnchor, SetImageToLastAnchor);
        ActionEventManager.Unsubscribe<AnnotationImageData>(EventName.SetImageToAnchor, SetImageToAnchor);
        ActionEventManager.Unsubscribe<ARAnchorData>(EventName.SetARAnchor, values);
    }

    /// <summary>
    /// Calculate anchor points for the coordinates send from the expert application and send back state infos
    /// </summary>
    /// <param name="coordinates">coordinates send from the expert application</param>
    public void values(ARAnchorData data)
    {
        Vector2Int coordinates = data.coordinate;

        if (coordinates.x >= 0 && coordinates.y >= 0 && coordinates.x <= Screen.width && coordinates.y <= Screen.height)
        {
            var anchor = AnnotationManager.Instance.createEmptyAnnotation(coordinates, data.drawingAreaScale, annotationOwner: AnnotationOwner.Server);
            if (anchor != null)
            {
                List<ARLayerPlane> planeList = null;
                var cameraTransform = new TransformParameter(CameraHelper.ARCamera.transform);
                if (anchor is AnchorAnnotationProjection3D)
                {
                    var anchorProjection = ((AnchorAnnotationProjection3D)anchor);
                    planeList = anchorProjection.DetectedPlanes;
                    cameraTransform.Position = anchorProjection.ProjectorPosition;
                    cameraTransform.Rotation = anchorProjection.ProjectorRotation;
                }

                EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.AnchorState, AnchorState.Success.ToString()));
                EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.AnchorId, anchor.Anchor.Id.ToString()));
                CommunicationManager.Instance.SendData(new AnchorImageData(anchor.Anchor.Id, planeList: planeList, cameraTransform: cameraTransform, anchorTransform: anchor.transform));
            }
            else
            {
                EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.AnchorState, AnchorState.NotFound.ToString()));
                EventNameManager.SendEventShowARHelper("ScanEnvironment");
                EventNameManager.SendEventAppendDebug("Scan Environment");
            }
        }
    }

    /// <summary>
    /// set the image data to the last anchor point
    /// </summary>
    /// <param name="bytes">image data byte array</param>
    public void SetImageToLastAnchor(byte[] bytes)
    {
        AnnotationManager.Instance.SaveImageToLastAnchor(bytes);
    }

    /// <summary>
    /// set the image data to the last anchor point
    /// </summary>
    /// <param name="bytes">image data byte array</param>
    public void SetImageToAnchor(AnnotationImageData annotationData)
    {
        AnnotationManager.Instance.SaveImageToAnchor(annotationData.AnchorId, annotationData.imageData, 
            annotationData.scaleFactor, annotationData.permanentSave);
    }
}
