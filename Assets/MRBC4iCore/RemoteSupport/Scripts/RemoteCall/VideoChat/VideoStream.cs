using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// display received video data from other device
/// </summary>
[RequireComponent(typeof(RectTransform), typeof(RawImage))]
public class VideoStream : MonoBehaviour, IPointerDownHandler
{
    public bool createAnchorOnClick = true;
    private RectTransform rectTransform;

    /// <summary>
    /// is the drawing usage active?
    /// </summary>
    private bool UseDrawing
    {
        get
        {
            if (!createAnchorOnClick) return false;

            if (DrawingSettings.HasInstance)
            {

                switch (DrawingSettings.Instance.AnnotationType)
                {
                    case AnnotationType.Pointer:
                        return false;
                    case AnnotationType.Drawing:
                        return (Input.GetMouseButton((int)PointerEventData.InputButton.Left) || Input.GetMouseButton((int)PointerEventData.InputButton.Right) || Input.touchCount > 0);
                    case AnnotationType.Both:
                        if (StatusProperties.Values.ExpertHasDrawingAnnotationActive && !StatusProperties.Values.LowBandwidthActive)
                        {
                            if (Input.GetMouseButton((int)PointerEventData.InputButton.Right) ||
                                (!StatusProperties.Values.ExpertHasCursorAnnotationActive && Input.GetMouseButton((int)PointerEventData.InputButton.Left)))
                            {
                                return true;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            return false;
        }
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Mouse down to set annotation anchor point on other device
    /// </summary>
    /// <param name="ped"></param>
    public void OnPointerDown(PointerEventData ped)
    {
        if (UseDrawing)
        {
            Vector2 mousePosInImage;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, ped.position, ped.pressEventCamera, out mousePosInImage))
                return;

            var shiftDelta = new Vector2(rectTransform.rect.width * rectTransform.pivot.x, rectTransform.rect.height * rectTransform.pivot.y);
            mousePosInImage += shiftDelta;

            var mousePosInClient = Vector2.zero;
            mousePosInClient.x = mousePosInImage.x / rectTransform.rect.size.x;
            mousePosInClient.y = mousePosInImage.y / rectTransform.rect.size.y;

            var cmdParam = Commands.getCoordinatesString(mousePosInClient.x, mousePosInClient.y);

            cmdParam += ";" + DrawingRemoteManager.Instance.DrawingOverlayWidth + ";" + DrawingRemoteManager.Instance.DrawingOverlayScaleFactor;

            EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.MouseDown, cmdParam));
        }
    }
}
