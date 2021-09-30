using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Basic drawing features
/// </summary>
public interface IDrawingCoordinatesBase
{
    Color DrawingColor { get; set; }
    DrawingState ActiveDrawingState { get; set; }

    void ResetCanvas();
    void SetHideDrawingOverTime(bool value);
}

/// <summary>
/// Manager script for basic drawing features like convert screen clicking coordinates to canvas drawing pixel texture coordinates. 
/// </summary>
/// <typeparam name="T">Type of the final class to get right instance typecast for the singleton pattern</typeparam>
[System.Serializable]
public abstract class DrawingCoordinatesBase<T> : AManager<T>, IDrawingCoordinatesBase where T : Component
{
    #region properties
    public LayerMask Drawing_Layers;
    public RectTransform drawingBoarders;
    public Camera cam;
    public bool Reset_Canvas_On_Play = true;

    protected Sprite drawable_sprite;
    protected Vector2 drawable_bounds, drawable_rect;

    private float lastMouseDown = -1, hintDeltaTime = 5;
    protected bool showARHelper = true;
    private bool arHelperActive = false;
    protected bool hideDrawingOverTime = false;

    protected int transparencyDeltaValue = 15;
    protected float transparencyDeltaTime = 0.1f;
    protected float updateMousePositionTime = 0.001f;
    protected float visibilityThreshold = 120;

    /// <summary>
    /// active drawing color
    /// </summary>
    public Color DrawingColor { get; set; } = Color.red;

    /// <summary>
    /// active drawing state (add, move or inactive) for all drawing activities of this drawing type
    /// </summary>    
    private DrawingState drawingState;
    public DrawingState ActiveDrawingState
    {
        get
        {
            return drawingState;
        }
        set
        {
            drawingState = value;
        }
    }

    /// <summary>
    /// drawing activities are recorded by this camera
    /// </summary>
    public Camera Cam
    {
        get
        {
            if (cam == null)
                return CameraHelper.MainCamera;

            return cam;
        }
    }

    /// <summary>
    /// define if the drawing is permanent or disappears over the time
    /// </summary>
    /// <param name="value">true: the drawing disappears over the time</param>
    public void SetHideDrawingOverTime(bool value)
    {
        hideDrawingOverTime = value;

        if (value)
        {
            StopCoroutine("HideDrawingOverTimeProcess");
            StartCoroutine("HideDrawingOverTimeProcess");
        }
    }

    /// <summary>
    /// let the drawings disappear over the time
    /// </summary>
    /// <returns></returns>
    IEnumerator HideDrawingOverTimeProcess()
    {
        while (hideDrawingOverTime && gameObject.activeInHierarchy)
        {
            yield return new WaitForSeconds(transparencyDeltaTime);
            HideDrawingOverTime();
        }

        hideDrawingOverTime = false;
    }
    #endregion

    #region unity default methods
    override protected void Awake()
    {
        base.Awake();
        //register by drawing manager
        DrawingManager.InterfaceInstance.DrawingCoordinatesList.Add(this);

        InitDrawing();
    }

    public virtual void InitDrawing()
    {
        //get drawing sprite from image display component of the same game object as this component
        var spriteRenderer = GetComponent<SpriteRenderer>();
        var image = GetComponent<Image>();
        var raw = GetComponent<RawImage>();
        DrawingColor = DrawingSettings.Instance.initDrawingColor;
        if (spriteRenderer)
            drawable_sprite = spriteRenderer.sprite;
        else if (image)
            drawable_sprite = image.sprite;

        //get drawing sprite boundaries
        if (drawable_sprite != null)
        {
            drawable_bounds = drawable_sprite.bounds.size;
            drawable_rect = drawable_sprite.rect.size;
        }
        else if (raw)
        {
            drawable_bounds = new Vector2(raw.texture.width, raw.texture.height);
            drawable_rect = drawable_bounds;
        }
    }

    protected virtual void OnDestroy()
    {
        //remove registration by drawing manager
        DrawingManager.InterfaceInstance.DrawingCoordinatesList.Remove(this);
    }

    IEnumerator UpdateDrawingArea()
    {
        while (true) {
            if (ActiveDrawingState == DrawingState.Add)
                AddMousePositionToQueue();
            yield return new WaitForSeconds(updateMousePositionTime);
        }
        yield return new WaitForSeconds(updateMousePositionTime);
    }

    private void UpdateOrEditFrame()
    {
        bool guiUsed = DrawingGUI.MouseInsideAnyDrawingGUI;

        //update drawing activity
        if (ActiveDrawingState == DrawingState.Add)
            UpdateDrawing();

        //update edit activity
        if (ActiveDrawingState == DrawingState.Move)
            EditDrawing();

        if (Input.GetMouseButton(0))
            lastMouseDown = Time.time;

        //check if drawing tools menus are open and close them when user starts to draw
        if (ActiveDrawingState != DrawingState.Inactive && !guiUsed)
        {
            bool mouse_held_down = Input.GetMouseButton(0);
            if (mouse_held_down)
            {
                DrawingGUI.SetAllDrawingGUIsInactive();
            }
        }


        if (DrawingManager.InterfaceInstance.Mode == AnnotationDrawingMode.New && ActiveDrawingState != DrawingState.Inactive)
        {
            if (Time.time - lastMouseDown > hintDeltaTime)
            {
                if (showARHelper)
                {
                    arHelperActive = true;
                    showARHelper = false;
                }
            }
            else arHelperActive = false;
        }
        else
        {
            arHelperActive = false;
        }

        if (arHelperActive)
        {
            EventNameManager.SendEventShowARHelper("DrawOnCanvas");
        }
    }

    private int dropFrameCount = 0;

    void Update()
    {
        bool calcFrame = true;
        
        //repeat: because thread on smart phone maybe not works
        if (!StatusProperties.Values.isServer)
        {
            if (hideDrawingOverTime && gameObject.activeInHierarchy)
                HideDrawingOverTime();
            if (ActiveDrawingState == DrawingState.Add)
                AddMousePositionToQueue();

            if (Time.deltaTime > 0.3f && dropFrameCount < 1)
            {
                dropFrameCount++;
                calcFrame = false;
            }
        }
        
        if (calcFrame)
        {
            dropFrameCount = 0;
            StartCoroutine("UpdateAsync");
        }
    }

    private bool asyncUpdateIsRunning = false;
    private IEnumerator UpdateAsync()
    {
        if (!asyncUpdateIsRunning)
        {
            asyncUpdateIsRunning = true;

            //draw frame
            UpdateOrEditFrame();
            if (hideDrawingOverTime)
                UpdateDrawingAlpha();

            yield return new WaitForEndOfFrame();
            asyncUpdateIsRunning = false;
        }
    }

    virtual public void OnEnable()
    {
        // Should we reset our canvas image when we hit play in the editor?
        if (Reset_Canvas_On_Play)
            ResetCanvas();

        lastMouseDown = Time.time;
        showARHelper = true;

        StartCoroutine("UpdateDrawingArea");
    }

    private void OnDisable()
    {
        StopCoroutine("UpdateDrawingArea");
    }
    #endregion

    #region drawing
    abstract public void AddMousePositionToQueue();

    /// <summary>
    /// update drawing new activity in active drawing layer
    /// </summary>
    abstract public void UpdateDrawing();

    /// <summary>
    /// update edit activity for selected drawing layer
    /// </summary>
    abstract public void EditDrawing();

    /// <summary>
    /// clear painting area
    /// </summary>
    abstract public void ResetCanvas();

    /// <summary>
    /// let the drawings disappear over the time
    /// </summary>
    abstract public void HideDrawingOverTime();

    abstract public void UpdateDrawingAlpha();

    protected virtual DrawingLayer[] GetDrawingLayers()
    {
        return DrawingLayerContainer.Instance.layerList();
    }

    /// <summary>
    /// is the screen touch point inside the drawing area
    /// </summary>
    /// <returns>true if inside</returns>
    public bool insideBoarderRect()
    {
        if (!drawingBoarders) return true;
        return RectTransformUtility.RectangleContainsScreenPoint(drawingBoarders, Input.mousePosition, null);
    }

    /// <summary>
    /// convert screen touch point to pixel coordinates in the drawing ares
    /// </summary>
    /// <param name="insideDrawingArea">returns if the screen touch position is inside drawing ares</param>
    /// <returns>pixel coordinates in the drawing ares</returns>
    virtual public Vector2 getPositionOnDrawingArea(out bool insideDrawingArea)
    {
        insideDrawingArea = false;
        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform)
        {
            //is inside drawing area
            insideDrawingArea = insideBoarderRect();
            if (!insideDrawingArea) return Vector2.zero;

            Vector2 mousePosInImage;
            //screen point to local point in rectangle
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, cam, out mousePosInImage))
            {
                return Vector2.zero;
            }

            //convert point in rectangle to pixel texture coordinates with center pivot point and a pixel size which corresponds to the scale of the drawing area.
            Vector2 pixel_pos = LocalToPixelCoordinates(mousePosInImage, rectTransform.rect.size);

            return pixel_pos;
        }
        else
        {
            // Convert mouse coordinates to world coordinates
            Vector2 mouse_world_position = Cam.ScreenToWorldPoint(Input.mousePosition);

            // Check if the current mouse position overlaps our image
            Collider2D hit = Physics2D.OverlapPoint(mouse_world_position, Drawing_Layers.value);
            if (hit != null && hit.transform != null)
            {
                // We're over the texture we're drawing on!
                //calculate pixel texture coordinates
                Vector2 pixel_pos = WorldToPixelCoordinates(mouse_world_position);
                insideDrawingArea = true;
                return pixel_pos;
            }

            return Vector2.zero;
        }
    }

    /// <summary>
    /// Convert world position to pixel texture coordinates
    /// </summary>
    /// <param name="world_position">world position intersect point with game object to draw on</param>
    /// <returns>pixel texture coordinates</returns>
    virtual public Vector2 WorldToPixelCoordinates(Vector2 world_position)
    {
        // Change coordinates to local coordinates of this image
        Vector3 local_pos = transform.InverseTransformPoint(world_position);
        Vector2 displayImageSize = new Vector2(Cam.scaledPixelWidth, Cam.scaledPixelHeight);
        if (drawable_bounds != null) displayImageSize = drawable_bounds;

        return LocalToPixelCoordinates(local_pos, displayImageSize);
    }

    /// <summary>
    /// Convert local point to pixel texture coordinates with center pivot point and a pixel size which corresponds to the scale of the drawing area.
    /// </summary>
    /// <param name="local_pos">local point in drawing area rectangle</param>
    /// <param name="displayImageSize">size of target drawing area</param>
    /// <returns>pixel texture coordinates</returns>
    virtual public Vector2 LocalToPixelCoordinates(Vector2 local_pos, Vector2 displayImageSize)
    {
        // Change these to coordinates of pixels
        float pixelWidth = displayImageSize.x;
        float pixelHeight = displayImageSize.y;

        //set drawing area pixel size
        if (drawable_rect != null)
        {
            pixelWidth = drawable_rect.x;
            pixelHeight = drawable_rect.y;
        }

        // calculate pixel units. Resize coordinates corresponding to the scale of the drawing area. Syndicate drawing area pixel size and scale size of game object
        float unitsToPixelsX = pixelWidth / displayImageSize.x;
        float unitsToPixelsY = pixelHeight / displayImageSize.y;

        // Need to center our coordinates and scale it to the pixel units.
        float centered_x = local_pos.x * unitsToPixelsX + pixelWidth / 2;
        float centered_y = local_pos.y * unitsToPixelsY + pixelHeight / 2;

        // Round current mouse position to nearest pixel
        Vector2 pixel_pos = new Vector2(Mathf.RoundToInt(centered_x), Mathf.RoundToInt(centered_y));
        return pixel_pos;
    }
    #endregion
}
