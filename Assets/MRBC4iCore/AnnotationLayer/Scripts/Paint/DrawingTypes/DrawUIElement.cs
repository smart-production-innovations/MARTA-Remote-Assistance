using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Pictograms can be raised in different ways on the canvas.
/// </summary>
public enum DragAndDropDrawingMode
{
    Corners, //The corners of a rectangle match the drag and drop point.
    CenterToEdge //Arrows are aligned from the tip in the pulling direction.

}

/// <summary>
/// Manges drawing of ui elements like images for pictograms or text into the drawing canvas.
/// </summary>
/// <typeparam name="T">Type of the final class to get right instance typecast for the singleton pattern</typeparam>
public class DrawUIElement<T> : DrawingCoordinatesBase<T> where T : Component
{
    #region properties
    public RectTransform imagePrefab;
    public RectTransform displayParent;

    protected Vector2 mouseDownPosition;
    protected bool isDrawing;
    protected RectTransform currentImageDrawing;
    protected RectTransform currentEditDrawing;

    protected DrawingLayer currentDrawingLayer;
    protected DragAndDropDrawingMode dragAndDropDrawingMode = DragAndDropDrawingMode.Corners;

    private int minimumSize = 5;
    #endregion

    #region unity loop
    override protected void Awake()
    {
        base.Awake();

        ActiveDrawingState = DrawingState.Inactive;
    }

    public override void OnEnable()
    {
        base.OnEnable();

        mouseDownPosition = Vector2.zero;
        isDrawing = false;
    }
    #endregion

    #region drawing
    /// <summary>
    /// Pictograms can be raised in different ways on the canvas. Define how to draw them.
    /// </summary>
    /// <param name="value"></param>
    public void setDragAndDropDrawingMode(DragAndDropDrawingMode value)
    {
        dragAndDropDrawingMode = value;
    }

    /// <summary>
    /// create a new drawing layer with a connected new ui element
    /// </summary>
    protected virtual void createUIElement()
    {
        currentImageDrawing = GameObject.Instantiate(imagePrefab, displayParent.transform);
        currentImageDrawing.sizeDelta = new Vector2(0, 0);
        currentImageDrawing.localPosition = Vector3.zero;

        currentDrawingLayer = DrawingLayerContainer.Instance.add(currentImageDrawing);
    }

    /// <summary>
    /// Set manager edit state to update existing drawing layer
    /// </summary>
    /// <param name="uiElement">connected ui element of the drawing layer which should be updated</param>
    public virtual void editUIElement(RectTransform uiElement)
    {
        currentEditDrawing = uiElement;

        ActiveDrawingState = (uiElement != null ? DrawingState.Move : DrawingState.Inactive);
    }

    override public void AddMousePositionToQueue()
    {

    }

    /// <summary>
    /// create a new drawing layer for each drawing action
    /// </summary>
    override public void UpdateDrawing()
    {
        try
        {
            // Is the user holding down the left mouse button?
            bool mouse_held_down = Input.GetMouseButton(0);

            // Mouse is released
            if (!mouse_held_down && isDrawing)
            {
                isDrawing = false;

                //if drawing element is to small to see it, delete the layer
                if (currentImageDrawing.rect.width < minimumSize && currentImageDrawing.rect.height < minimumSize)
                    currentDrawingLayer.Delete();

                currentImageDrawing = null;
            }

            if (mouse_held_down)
            {
                bool insideDrawingArea = true;
                Vector2 pixel_pos = getPositionOnDrawingArea(out insideDrawingArea);

                //is click inside drawing area?
                if (!insideDrawingArea)
                {
                    return;
                }

                if (isDrawing)
                {
                    //Resize ui element while touch is hold on
                    //The corners of a rectangle match the drag and drop point.
                    if (dragAndDropDrawingMode == DragAndDropDrawingMode.Corners)
                    {
                        currentImageDrawing.pivot = Vector2.zero;
                        currentImageDrawing.sizeDelta = new Vector2(Mathf.Abs(mouseDownPosition.x - pixel_pos.x), Mathf.Abs(mouseDownPosition.y - pixel_pos.y));
                        currentImageDrawing.localPosition = new Vector2(Mathf.Min(mouseDownPosition.x, pixel_pos.x), Mathf.Min(mouseDownPosition.y, pixel_pos.y));
                    }
                    else //Arrows are aligned from the tip in the pulling direction.
                    {
                        currentImageDrawing.pivot = new Vector2(0, 0.5f);
                        currentImageDrawing.localPosition = mouseDownPosition;

                        var dir = (pixel_pos - mouseDownPosition).normalized;
                        var angleDir = -Vector2.SignedAngle(dir, Vector2.right);
                        currentImageDrawing.eulerAngles = new Vector3(0, 0, angleDir);
                        currentImageDrawing.sizeDelta = Vector2.one * Vector2.Distance(mouseDownPosition, pixel_pos);
                    }
                }
                else
                {
                    //create ui element when new touch happens
                    isDrawing = true;
                    mouseDownPosition = pixel_pos;
                    createUIElement();
                }
            }
        }
        catch (MissingReferenceException e)
        {
            isDrawing = false;
            currentDrawingLayer.Delete();
            currentImageDrawing = null;
        }
    }

    /// <summary>
    /// edit (move) ui element for selected drawing layer
    /// </summary>
    public override void EditDrawing()
    {
        // Is the user holding down the left mouse button?
        bool mouse_held_down = Input.GetMouseButton(0);

        if (mouse_held_down)
        {
            bool insideDrawingArea = true;
            Vector2 pixel_pos = getPositionOnDrawingArea(out insideDrawingArea);
            //is click inside drawing area
            if (!insideDrawingArea)
            {
                return;
            }

            //move ui element to touch position in drawing canvas
            currentEditDrawing.localPosition = pixel_pos;
        }

    }

    /// <summary>
    /// set empty drawing area by deleting all drawing layers
    /// </summary>
    override public void ResetCanvas()
    {
        DrawingLayerContainer.Instance.clear();
    }

    /// <summary>
    /// let the drawings disappear over the time
    /// </summary>
    public override void HideDrawingOverTime()
    {
        var list = GetDrawingLayers();
        bool mouse_held_down = Input.GetMouseButton(0);
        foreach (var item in list)
        {
            var changeAlpha = (!item.isDefaultLayer && (!item.Equals(currentDrawingLayer) || !mouse_held_down));

            if (changeAlpha)
            {
                var graphicElement = item.PlacementObject.GetComponent<Graphic>();
                if (graphicElement)
                {
                    var color = graphicElement.color;
                    if (color.a > 0)
                    {
                        var delta = transparencyDeltaValue / 255f;
                        var threshold = visibilityThreshold / 255f;
                        var alpha = color.a - delta;
                        if (alpha < threshold) alpha = 0;
                        color.a = alpha;
                        graphicElement.color = color;
                    }
                    else if (color.a <= 0)
                    {
                        Destroy(item.PlacementObject.gameObject);
                        Destroy(item.gameObject);
                    }
                }
            }
        }
    }

    public override void UpdateDrawingAlpha()
    {

    }
    #endregion
}
