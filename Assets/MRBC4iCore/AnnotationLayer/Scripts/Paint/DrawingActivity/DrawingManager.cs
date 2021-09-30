using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using UnityEditor;

/// <summary>
/// interface defines all general drawing activities for the annotation layer
/// </summary>
public interface IDrawingManager
{
    /// <summary>
    /// state of the drawing manger. DrawingActive = true -> user is currently in drawing mode of an annotation. DrawingActive = false -> manager is waiting for are new annotation click.
    /// </summary>
    bool DrawingActive { get; set; }

    /// <summary>
    /// Anchor ID of the drawing being processed
    /// </summary>
    int ActiveAnchorId { get; set; }

    /// <summary>
    /// is manager in add or edit mode
    /// </summary>
    AnnotationDrawingMode Mode { get; set; }

    /// <summary>
    /// List of all drawing type manager (freehand, text, images)
    /// </summary>
    List<IDrawingCoordinatesBase> DrawingCoordinatesList { get; }

    /// <summary>
    /// Get the texture of the screen snapshot of the actual drawing activity
    /// </summary>
    Texture SnapshotTexture { get; }

    /// <summary>
    /// set color change to all drawing types
    /// </summary>
    /// <param name="color">new color</param>
    void SetAllColor(Color color);

    /// <summary>
    /// get active color from drawing types
    /// </summary>
    Color GetAllColor();

    /// <summary>
    /// Set all drawing types inactive
    /// </summary>
    void SetAllInactive();

    /// <summary>
    /// Clear all drawn elements of all drawing types
    /// </summary>
    void HideDrawingOverTime(bool value);
}

/// <summary>
/// manages all drawing activities for the annotation layer
/// </summary>
public class DrawingManager : DrawingManager<DrawingManager>
{
}

/// <summary>
/// manages all drawing activities for the annotation layer
/// </summary>
/// <typeparam name="T">Type of the final class to get right instance typecast</typeparam>
public class DrawingManager<T> : AInterfaceClickManager<T, IDrawingManager>, IDrawingManager where T : Component, IDrawingManager
{
    #region properties
    //game object which contains all drawing tool ui elements
    public GameObject drawingGUI;
    //game object which displays the snapshot texture for the active annotation drawing
    public RawImage SnapshotPlaceholder;
    //game object which displays the snapshot texture for the active annotation drawing
    public RawImage SnapshotPlaceholderForDataSend;
    //game object which contains the overlay drawing image
    public RectTransform drawingOverlay;

    //render texture in which the annotation drawing will be temporary saved
    public RenderTexture TemporaryRenderTexture;

    public Camera DrawCamera;
    public DrawFreeHand drawFreeHand;
    public VideoSize videoSize;

    public RawImage drawingOverlayImg;


    /// <summary>
    /// create a new render texture
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    private void CreateTemporaryRenderTexture(int width, int height)
    {
        TemporaryRenderTexture = RenderTexture.GetTemporary(StatusProperties.Values.DrawingResizeFactor * width, StatusProperties.Values.DrawingResizeFactor * height, 24);

        drawingOverlayImg.texture = TemporaryRenderTexture;
        DrawCamera.targetTexture = TemporaryRenderTexture;

        //init a Texture2D object for saving the final annotation image
        mTexture = new Texture2D(StatusProperties.Values.DrawingResizeFactor * width, StatusProperties.Values.DrawingResizeFactor * height, TextureFormat.ARGB32, false);
    }
    
    protected Texture2D mTexture;
    //Should the screen drawing annotation be cached?
    private bool continuousDataSend = false;
    //Time between automatic saving
    protected float continuousDataSendDeltaTime = 0.5f;

    public int activeAnchorId = -1;
    /// <summary>
    /// Anchor ID of the drawing being processed
    /// </summary>
    public int ActiveAnchorId
    {
        get
        {
            return activeAnchorId;
        }
        set
        {
            activeAnchorId = value;
        }
    }

    /// <summary>
    /// ensure that in one unity loop an annotation is not saved and a new one is created at the same time.
    /// </summary>
    private bool readyForNextDrawing = true;
    protected bool ReadyForNextDrawing
    {
        get
        {
            return (readyForNextDrawing && !DrawingActive);
        }
    }

    /// <summary>
    /// is manager in add or edit mode
    /// </summary>
    private AnnotationDrawingMode mode;
    public AnnotationDrawingMode Mode
    {
        get
        {
            return mode;
        }
        set
        {
            mode = value;
            var showPositionMark = (value == AnnotationDrawingMode.New);
            if (StatusProperties.Values.CalculationMode != CalculationMode.ClickPointPlane || !StatusProperties.Values.ARActive)
                showPositionMark = false;

            var posMark = Resources.FindObjectsOfTypeAll<PositionMark>();
            foreach (var item in posMark)
            {
                item.gameObject.SetActive(showPositionMark);
            }
        }
    }

    /// <summary>
    /// state of the drawing manger. DrawingActive = true -> user is currently in drawing mode of an annotation. DrawingActive = false -> manager is waiting for are new annotation click.
    /// </summary>
    public virtual bool DrawingActive
    {
        get
        {
            if (drawingGUI)
                return drawingGUI.activeSelf;
            return false;
        }
        set
        {
            var oldValue = drawingGUI.activeSelf;

            // Adjust the resolution of the annotation to the device resolution.
            if (drawingGUI.activeSelf != value)
            {
                if (value)
                {
                    videoSize?.calcImageRatio(ResolutionManager.Instance.pWidth, ResolutionManager.Instance.pHeight);
                }
                else
                {
                    videoSize?.calcImageRatio(ResolutionManager.Instance.lWidth, ResolutionManager.Instance.lHeight);
                }
            }


            drawingGUI.SetActive(value);
            if (!value)
            {
                SnapshotPlaceholder.enabled = false;

                if(SnapshotPlaceholderForDataSend != null)
                    SnapshotPlaceholderForDataSend.enabled = false;
            }

            //ensure that in one unity loop an annotation is not saved and a new one is created at the same time.
            readyForNextDrawing = false;

            if (!value)
                RemoteHelperImage.AnchorId = -1;

            // apply the status configuration to all associated UI elements
            if (value != oldValue)
                ToolProperties.SetAllItemsActive(true);
        }
    }

    /// <summary>
    /// Synchronize pivot point screen position of the drawing overlay with the click position of the screen
    /// </summary>
    public Vector2 OverlayAnchorPosition
    {
        get
        {
            return drawingOverlay.localPosition;
        }
        set
        {
            //In projection mode, the entire screen area is used for annotating the screenshot. The transformation of the 2D annotation into 3D space is done later by projection.
            if (StatusProperties.Values.CalculationMode == CalculationMode.ClickPointPlane)
                drawingOverlay.localPosition = value;
        }
    }

    /// <summary>
    /// pixel size of the drawing canvas
    /// </summary>
    public Vector2 DrawingOverlaySize
    {
        get
        {
            if (drawingOverlay)
            {
                var overlaySize = drawingOverlay.GetComponent<RectTransform>();
                if (overlaySize)
                {
                    return overlaySize.rect.size;
                }
            }
            return new Vector2(Screen.width, Screen.height);
        }
    }

    /// <summary>
    /// width of the drawing canvas
    /// </summary>
    public float DrawingOverlayWidth
    {
        get
        {
            if (drawingOverlay)
            {
                var overlaySize = drawingOverlay.GetComponent<RectTransform>();
                if (overlaySize)
                {
                    return overlaySize.rect.width;
                }
            }
            return Screen.width;
        }
    }

    /// <summary>
    /// screen size
    /// </summary>
    public virtual Vector2 ScreenSize
    {
        get
        {
            return new Vector2(Screen.width, Screen.height);
        }
    }
    /// <summary>
    /// drawing area (artboard) size
    /// </summary>
    public virtual Vector2 DrawingAreaSize
    {
        get
        {
            return ScreenSize;
        }
    }

    /// <summary>
    /// Size of the border of the artboard (drawing area) by the underlying screenshot
    /// </summary>
    public virtual Vector2 DrawingBoundsSize
    {
        get
        {
            return ScreenSize;
        }
    }

    /// <summary>
    /// scale the artboard (drawing area) to cover the entire screenshot
    /// </summary>
    public virtual float DrawingOverlayScaleFactor
    {
        get
        {
            return getDrawingOverlayScaleFactor(DrawingOverlayWidth, DrawingAreaSize.x);
        }
    }

    /// <summary>
    /// Get the texture of the screen snapshot of the actual drawing activity
    /// </summary>
    public virtual Texture SnapshotTexture
    {
        get
        {
            if (SnapshotPlaceholder)
                return SnapshotPlaceholder.texture;
            else if (SnapshotPlaceholderForDataSend)
                return SnapshotPlaceholderForDataSend.texture;

            return null;
        }
    }

    /// <summary>
    /// Should the screen drawing annotation be cached?
    /// </summary>
    public bool ContinuousDataSend
    {
        get
        {
            return continuousDataSend;
        }
        set
        {
            continuousDataSend = value;

            if (value)
            {
                StopAllCoroutines();
                StartCoroutine("ContinuousDataSendProcess");
            }
        }
    }
    #endregion

    #region unity loop
    protected override void Awake()
    {
        ResolutionManager.OnPResolutionChanged += CreateTemporaryRenderTexture;
        base.Awake();

        //determine the temporary render texture if this is not set
        if (TemporaryRenderTexture == null)
        {
            var raw = GetComponent<RawImage>();
            if (raw)
                TemporaryRenderTexture = (RenderTexture)raw.texture;
        }
    }

    private void OnDestroy()
    {
        ResolutionManager.OnPResolutionChanged -= CreateTemporaryRenderTexture;
    }

    protected override void Update()
    {
        base.Update();

        //ensure that in one unity loop an annotation is not saved and a new one is created at the same time.
        if (!readyForNextDrawing && !DrawingActive && !hasActiveTouchPoint())
        {
            readyForNextDrawing = true;
        }
    }
    #endregion

    #region touch
    /// <summary>
    /// was any mouse button pressed?
    /// </summary>
    protected bool hasActiveTouchPoint()
    {
        switch (DrawingSettings.Instance.AnnotationType)
        {
            case AnnotationType.Pointer:
                return false;
            case AnnotationType.Drawing:
            case AnnotationType.Both:
                if (SystemInfo.deviceType == DeviceType.Desktop)
                {
                    return Input.GetMouseButton((int)PointerEventData.InputButton.Left) || 
                        Input.GetMouseButton((int)PointerEventData.InputButton.Right) || 
                        Input.GetMouseButton((int)PointerEventData.InputButton.Middle);
                }
                else
                {
                    return (Input.touchCount > 0);
                }
                break;
            default:
                break;
        }

        return false;
    }

    /// <summary>
    /// Define the action happens on touch or mouse down
    /// </summary>
    protected override bool InputPositionDownEvents(Vector2 screenPosition)
    {
        base.InputPositionDownEvents(screenPosition);

        var valid = !DrawingGUI.MouseInsideAnyDrawingGUI;

        //ensure that in one unity loop an annotation is not saved and a new one is created at the same time.
        if (ReadyForNextDrawing && valid)
        {
            OverlayAnchorPosition = screenPosition - DrawingBoundsSize / 2; ;
        }
        return valid;
    }

    /// <summary>
    /// activates a empty drawing canvas
    /// </summary>
    public virtual void DisplayEmptyDrawingArea()
    {
        ResolutionManager.Instance.ChangePResolution();

        //initial drawing type for new annotation is freehand drawing
        DrawingSettings.Instance.SetPen();
        DrawingSettings.Instance.SetActiveDrawingTypeFreeHand();

        Mode = AnnotationDrawingMode.New;

        DrawingActive = true;

        matchSize();
    }

    /// <summary>
    /// activates a empty drawing canvas
    /// </summary>
    public virtual void DisplayEmptyDrawingArea(Vector2 clickPoint)
    {
        this.DisplayEmptyDrawingArea();
        OverlayAnchorPosition = clickPoint * (DrawingBoundsSize / 2);
        matchSize();
    }

    /// <summary>
    /// activates a drawing canvas for editing a existing annotation
    /// </summary>
    public virtual void DisplayDrawingArea(Vector2 pivot, Vector2 clickPoint, Texture2D annotation = null, Texture snapshot = null, int snapshotOrientation = 1)
    {
        if (annotation != null)
        {
            ResolutionManager.Instance.ChangePResolution(annotation.width / StatusProperties.Values.DrawingResizeFactor, annotation.height / StatusProperties.Values.DrawingResizeFactor);
        }

        if (annotation || (!annotation && !snapshot))
        {
            //init drawing type for new annotation is freehand drawing
            DrawingSettings.Instance.SetPen();
            DrawingSettings.Instance.SetActiveDrawingTypeFreeHand();

            Mode = AnnotationDrawingMode.Edit;

            DrawingActive = true;
        }

        if (snapshot) SetSnapshotTextureForDataSend(snapshot, orientation: snapshotOrientation);

        if (annotation)
        {
            DrawFreeHand.Instance.ResetCanvas();
            DrawFreeHand.Instance.loadDrawing(annotation, pivot);
        }

        OverlayAnchorPosition = clickPoint * (DrawingBoundsSize / 2);


        matchSize();
    }

    /// <summary>
    /// synchronize size and position of the video panel with the size and position of the drawing panel
    /// </summary>
    public virtual void matchSize()
    {
        float sizeX = SnapshotPlaceholder.rectTransform.rect.width;
        float sizeY = SnapshotPlaceholder.rectTransform.rect.height;
        //In projection mode, the entire screen area is used for annotating the screenshot. The transformation of the 2D annotation into 3D space is done later by projection.
        drawingOverlay.sizeDelta = new Vector2(StatusProperties.Values.DrawingResizeFactor * sizeX, StatusProperties.Values.DrawingResizeFactor * sizeY);
        if (StatusProperties.Values.CalculationMode == CalculationMode.Projection)
        {
            drawingOverlay.anchoredPosition = Vector3.zero;
        }
    }
    #endregion

    #region settings
    /// <summary>
    /// show snapshot texture for the active annotation drawing
    /// </summary>
    /// <param name="texture">snapshot texture</param>
    public void SetSnapshotTexture(Texture2D texture)
    {
        SnapshotPlaceholder.texture = texture;
        if (ARPlaneDisplayManager.HasInstance)
            ARPlaneDisplayManager.Instance.SetTexture(texture);
        SnapshotPlaceholder.enabled = true;
        if(SnapshotPlaceholderForDataSend != null)
            SnapshotPlaceholderForDataSend.enabled = false;
    }

    /// <summary>
    /// show snapshot texture for the active annotation drawing
    /// </summary>
    /// <param name="texture">snapshot texture</param>
    public void SetSnapshotTextureForDataSend(Texture texture, int orientation = 1)
    {
        if (SnapshotPlaceholderForDataSend != null)
        {
            SnapshotPlaceholderForDataSend.texture = texture;

            if (ARPlaneDisplayManager.HasInstance)
            {
                var flipDirection = ARPlaneDisplayManager.flipDirection.both;
                if (orientation == -1)
                    flipDirection = ARPlaneDisplayManager.flipDirection.horizontal;
                ARPlaneDisplayManager.Instance.SetTexture(texture, flipDirection);
            }
            SnapshotPlaceholderForDataSend.enabled = true;
            var uv = SnapshotPlaceholderForDataSend.uvRect;
            uv.height = orientation;
            SnapshotPlaceholderForDataSend.uvRect = uv;
        }
        SnapshotPlaceholder.enabled = false;
    }

    /// <summary>
    /// scale the artboard (drawing area) to cover the entire screenshot
    /// </summary>
    /// <param name="drawingOverlayWidth">artboard size</param>
    /// <param name="drawingWidth">screenshot size</param>
    /// <returns></returns>
    public static float getDrawingOverlayScaleFactor(float drawingOverlayWidth, float drawingWidth)
    {
        return drawingWidth / (drawingOverlayWidth / 2);
    }

    /// <summary>
    /// cache the screen drawing annotation while the drawing activity remains active
    /// </summary>
    /// <returns></returns>
    IEnumerator ContinuousDataSendProcess()
    {
        var i = 0;
        while (ContinuousDataSend && DrawingActive)
        {
            ContinuousSaveImage(false);
            yield return new WaitForSeconds(continuousDataSendDeltaTime);
        }

        ContinuousDataSend = false;
    }
    #endregion

    #region manage all
    /// <summary>
    /// List of all drawing type manager (freehand, text, images)
    /// </summary>
    private List<IDrawingCoordinatesBase> drawingCoordinatesList = new List<IDrawingCoordinatesBase>();
    public List<IDrawingCoordinatesBase> DrawingCoordinatesList
    {
        get
        {
            return drawingCoordinatesList;
        }
    }

    /// <summary>
    /// Set all drawing types inactive
    /// </summary>
    public void SetAllInactive()
    {
        foreach (var item in DrawingCoordinatesList)
        {
            item.ActiveDrawingState = DrawingState.Inactive;
        }
    }

    /// <summary>
    /// set color change to all drawing types
    /// </summary>
    /// <param name="color">new color</param>
    public void SetAllColor(Color color)
    {
        foreach (var item in DrawingCoordinatesList)
        {
            item.DrawingColor = color;
        }
    }

    /// <summary>
    /// get active color from drawing types
    /// </summary>
    public Color GetAllColor()
    {
        foreach (var item in DrawingCoordinatesList)
        {
            return item.DrawingColor;
        }
        return Color.red;
    }

    /// <summary>
    /// Clear all drawn elements of all drawing types
    /// </summary>
    public void ResetAll()
    {
        foreach (var item in DrawingCoordinatesList)
        {
            item.ResetCanvas();
        }
    }

    /// <summary>
    /// Clear all drawn elements of all drawing types
    /// </summary>
    public void HideDrawingOverTime(bool value)
    {
        foreach (var item in DrawingCoordinatesList)
        {
            item.SetHideDrawingOverTime(value);
        }
        ContinuousDataSend = value;
    }
    #endregion

    #region result
    /// <summary>
    /// get image data of the temporary annotation render texture
    /// </summary>
    /// <returns>byte stream of the image</returns>
    public byte[] GetImageData()
    {
        var temp = RenderTexture.active;

        //write render texture to Texture2D
        RenderTexture.active = TemporaryRenderTexture;
        mTexture.ReadPixels(new Rect(0, 0, TemporaryRenderTexture.width, TemporaryRenderTexture.height), 0, 0, false);
        mTexture.Apply();

        RenderTexture.active = temp;

        //get byte stream of the texture
        byte[] mByteBuffer = mTexture.EncodeToPNG();

        return mByteBuffer;
    }

    /// <summary>
    /// save the screen drawing annotation
    /// </summary>
    public virtual void SaveImage()
    {
        ContinuousSaveImage(true);

        DrawingActive = false;
    }

    /// <summary>
    /// cache the screen drawing annotation while the drawing activity remains active
    /// </summary>
    public virtual void ContinuousSaveImage(bool permanentSave = false)
    {
    }

    /// <summary>
    /// save the screen drawing annotation
    /// </summary>
    /// <param name="image">static ui image</param>
    public virtual void DeleteImage()
    {
        DrawingActive = false;

        if (AnchorGalleryDetailManager.HasInstance)
            AnchorGalleryDetailManager.Instance.GoToOverview();
    }

    /// <summary>
    /// cancel the drawing activities for the active annotation drawing. The last changes are discarded and the anchor point delete if it is empty.
    /// </summary>
    public virtual void CancelDrawing(bool goToGalleryOverviewIfGalleryIsOpen = false)
    {
        DrawingActive = false;

        if (goToGalleryOverviewIfGalleryIsOpen && AnchorGalleryDetailManager.HasInstance)
            AnchorGalleryDetailManager.Instance.GoToOverview();
    }
    #endregion

    #region load
    /// <summary>
    /// load image date from annotation anchor to drawing area to edit the annotation again
    /// </summary>
    /// <param name="image">annotation anchor game object</param>
    public virtual void LoadImageFromAnchor(AnchorImage image)
    {
        //init drawing type for new annotation is freehand drawing
        DrawingSettings.Instance.SetPen();
        DrawingSettings.Instance.SetActiveDrawingTypeFreeHand();
        Mode = AnnotationDrawingMode.Edit;
        DrawingActive = true;
    }
    #endregion
}
