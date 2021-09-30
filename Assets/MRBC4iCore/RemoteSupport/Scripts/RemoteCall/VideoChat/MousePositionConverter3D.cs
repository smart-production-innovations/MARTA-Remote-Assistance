using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Convert mouse position for expert mouse pointer visualization on the client device
/// </summary>
public class MousePositionConverter3D : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler//, IPointerClickHandler
{
    protected virtual void hidePointer()
    {
        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.StopParticleAnnotation, ""));
    }

    protected virtual void updatePointer()
    {
    }

    /// <summary>
    /// is the pointer usage active?
    /// </summary>
    private bool UsePointer
    {
        get
        {
            if (!DrawingSettings.HasInstance)
                return (Input.GetMouseButton((int)PointerEventData.InputButton.Left) || Input.GetMouseButton((int)PointerEventData.InputButton.Right) || Input.touchCount > 0);
            else
            {
                switch (DrawingSettings.Instance.AnnotationType)
                {
                    case AnnotationType.Pointer:
                        return (Input.GetMouseButton((int)PointerEventData.InputButton.Left) || Input.GetMouseButton((int)PointerEventData.InputButton.Right) || Input.touchCount > 0);
                    case AnnotationType.Drawing:
                        return false;
                    case AnnotationType.Both:
                        if (StatusProperties.Values.ExpertHasCursorAnnotationActive && !StatusProperties.Values.LowBandwidthActive)
                        {
                            if (Input.GetMouseButton((int)PointerEventData.InputButton.Left) ||
                                (!StatusProperties.Values.ExpertHasDrawingAnnotationActive && Input.GetMouseButton((int)PointerEventData.InputButton.Right)))
                            {
                                return true;
                            }
                        }
                        break;
                    default:
                        break;
                }

                return false;
            }
        }
    }

    private void Update()
    {
        updatePointer();
    }

    /// <summary>
    /// get mouse or touch down event
    /// </summary>
    /// <param name="ped">Current event data</param>
    public void OnPointerDown(PointerEventData ped)
    {
        StopAllCoroutines();
        if (UsePointer)
        {
            StartCoroutine(startHoldTimer(ped));
        }
    }

    /// <summary>
    /// get mouse or touch up event
    /// </summary>
    /// <param name="ped">Current event data</param>
    public void OnPointerUp(PointerEventData ped)
    {
        StopAllCoroutines();
        hidePointer();
    }

    /// <summary>
    /// get mouse or touch exit event
    /// </summary>
    /// <param name="ped">Current event data</param>
    public void OnPointerExit(PointerEventData ped)
    {
        StopAllCoroutines();
        hidePointer();
    }

    /// <summary>
    /// get mouse or touch enter event
    /// </summary>
    /// <param name="ped">Current event data</param>
    public void OnPointerEnter(PointerEventData ped)
    {
        StopAllCoroutines();
        if (UsePointer)
        {
            StartCoroutine(startHoldTimer(ped));
        }
    }

    /// <summary>
    /// permanent send mouse position for cursor visualization to the client
    /// </summary>
    /// <param name="ped">Current event data</param>
    /// <returns></returns>
    private IEnumerator startHoldTimer(PointerEventData ped)
    {
        if (BandwidthManager.Instance.SupportModeType != SupportModeType.LowBandwidthMode)
        {
            while (true)
            {
                Vector2 mousePosInImage;
                var rectTransform = GetComponent<RectTransform>();

                var shiftDelta = new Vector2(rectTransform.rect.width * rectTransform.pivot.x, rectTransform.rect.height * rectTransform.pivot.y);

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, ped.position, ped.pressEventCamera, out mousePosInImage))
                {
                    mousePosInImage += shiftDelta;

                    var viewPointCoord = Vector2.zero;
                    viewPointCoord.x = mousePosInImage.x / rectTransform.rect.size.x;
                    viewPointCoord.y = mousePosInImage.y / rectTransform.rect.size.y;

                    setNewPointerPosition(viewPointCoord);
                }
                yield return new WaitForSeconds(0.05F);
            }
        }
    }

    protected virtual void setNewPointerPosition(Vector2 viewPointCoord)
    {
        var cmdParam = Commands.getCoordinatesString(viewPointCoord.x, viewPointCoord.y);
        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.ConvertInto3DMousePosition, cmdParam));
    }
}
