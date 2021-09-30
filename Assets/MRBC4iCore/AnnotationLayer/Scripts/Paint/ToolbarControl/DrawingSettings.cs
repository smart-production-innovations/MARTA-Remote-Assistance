using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum DrawingColor
{
    red,
    orange,
    yellow,
    magenta,
    violet,
    blue,
    cyan,
    green,
    brown,
    gray,
    black,
    white
}

public enum DrawingType
{
    None,
    FreeHand,
    Image,
    Text
}

public enum DrawingState
{
    Add,
    Move,
    Inactive
}

public enum AnnotationType
{
    Pointer,
    Drawing,
    Both
}


/// <summary>
/// Helper methods used to set drawing settings
/// </summary>
public class DrawingSettings : AManager<DrawingSettings>
{
    #region properties
    public Color selectedColor, defaultColor, initDrawingColor = Color.red;
    public Image showSelectedColor;
    public Image activePenColor;
    public Image activeImageColor;
    public Image activeTextColor;
    public GameObject colorPanel;

    private float transparency = 1f;
    private Color lastColor;
    private DrawingType drawingType = DrawingType.FreeHand;
    private DrawingState state = DrawingState.Add;

    public AnnotationType annotationType = AnnotationType.Both;

    public AnnotationType AnnotationType
    {
        get { return annotationType; }
        set
        {
            annotationType = value;
            StatusProperties.Values.DefaultValueLiveDrawing = (value == AnnotationType.Both);
            ToolProperties.SetAllItemsActive(true);
        }
    }
    #endregion

    #region unity loop
    override protected void Awake()
    {
        base.Awake();
        lastColor = initDrawingColor;
        SetMarkerColor(lastColor);
    }
    #endregion

    #region marker color
    /// <summary>
    /// convert DrawingColor enum to color object
    /// </summary>
    /// <param name="colorName">drawing color name</param>
    /// <returns>color object</returns>
    public Color getColorCode(DrawingColor colorName)
    {
        Color c = Color.white;
        switch (colorName)
        {
            case DrawingColor.red:
                c = Color.red;
                break;
            case DrawingColor.orange:
                c = new Color(1, 0.5f, 0);
                break;
            case DrawingColor.yellow:
                c = Color.yellow;
                break;
            case DrawingColor.magenta:
                c = Color.magenta;
                break;
            case DrawingColor.violet:
                c = new Color(0.5f, 0, 1f);
                break;
            case DrawingColor.blue:
                c = Color.blue;
                break;
            case DrawingColor.cyan:
                c = Color.cyan;
                break;
            case DrawingColor.green:
                c = Color.green;
                break;
            case DrawingColor.brown:
                c = new Color(0.5f, 0.25f, 0);
                break;
            case DrawingColor.gray:
                c = Color.grey;
                break;
            case DrawingColor.black:
                c = Color.black;
                break;
            case DrawingColor.white:
                c = Color.white;
                break;
            default:
                break;
        }

        return c;
    }

    /// <summary>
    /// Change the drawing color to the new value
    /// </summary>
    /// <param name="colorName">new color name</param>
    public void SetMarkerColor(DrawingColor colorName)
    {
        Color c = getColorCode(colorName);
        c.a = transparency;
        SetMarkerColor(c);
        DrawFreeHand.Instance.SetPenBrush();
        if (showSelectedColor && showSelectedColor.GetComponent<Toggle>())
            showSelectedColor.GetComponent<Toggle>().isOn = false;
        else if (colorPanel) colorPanel.SetActive(false);
    }

    /// <summary>
    /// change the drawing color to the new value
    /// </summary>
    /// <param name="new_color">new color</param>
    public void SetMarkerColor(Color new_color)
    {
        if (!DrawFreeHand.Instance.IsEraserActive())
            lastColor = DrawFreeHand.Instance.DrawingColor;

        DrawingManager.InterfaceInstance.SetAllColor(new_color);

        if (showSelectedColor && new_color.a > 0)
        {
            var color = new_color;
            color.a = 1;
            showSelectedColor.color = color;
            markActiveDrawingToolIcon();
        }
    }

    public Color MarkerColor
    {
        get { return DrawingManager.InterfaceInstance.GetAllColor(); }
        set { SetMarkerColor(value); }
    }


    /// <summary>
    /// change the drawing color to the new value
    /// </summary>
    /// <param name="red">rgb red</param>
    /// <param name="green">rgb green</param>
    /// <param name="blue">rgb blue</param>
    public void SetMarkerColor(float red, float green, float blue)
    {
        SetMarkerColor(new Color(red, green, blue));
    }

    /// <summary>
    /// Should the screen drawing annotation be cached and send constantly to the other devices?
    /// </summary>
    /// <param name="value">true: cache the screen drawing annotation while the drawing activity remains active</param>
    public void SetContinuousDataSend(bool value)
    {
        DrawingManager.InterfaceInstance.HideDrawingOverTime(value);
    }
    #endregion

    // new_width is radius in pixels
    #region marker properties
    /// <summary>
    /// set the thickness of the free drawing pen
    /// </summary>
    /// <param name="new_width">pen thickness</param>
    public void SetMarkerWidth(int new_width)
    {
        DrawFreeHand.Instance.Pen_Width = new_width;
    }

    /// <summary>
    /// set the thickness of the free drawing pen
    /// </summary>
    /// <param name="new_width">pen thickness</param>
    public void SetMarkerWidth(float new_width)
    {
        SetMarkerWidth((int)new_width);
    }

    /// <summary>
    /// set the alpha value of the drawing color
    /// </summary>
    /// <param name="amount">alpha value</param>
    public void SetTransparency(float amount)
    {
        transparency = amount;

        if (!DrawFreeHand.Instance.IsEraserActive())
        {
            Color c = DrawFreeHand.Instance.DrawingColor;
            c.a = amount;
            SetMarkerColor(c);
        }
    }
    #endregion

    /// <summary>
    /// Call these functions to change the pen settings
    /// </summary>
    #region set pen color
    public void SetMarkerRed()
    {
        SetMarkerColor(DrawingColor.red);
    }
    public void SetMarkerGreen()
    {
        SetMarkerColor(DrawingColor.green);
    }
    public void SetMarkerBlue()
    {
        SetMarkerColor(DrawingColor.blue);
    }
    public void SetMarkerYellow()
    {
        SetMarkerColor(DrawingColor.yellow);
    }
    public void SetMarkerMagenta()
    {
        SetMarkerColor(DrawingColor.magenta);
    }
    public void SetMarkerCyan()
    {
        SetMarkerColor(DrawingColor.cyan);
    }
    public void SetMarkerWhite()
    {
        SetMarkerColor(DrawingColor.white);
    }
    public void SetMarkerBlack()
    {
        SetMarkerColor(DrawingColor.black);
    }
    public void SetMarkerGray()
    {
        SetMarkerColor(DrawingColor.gray);
    }
    public void SetMarkerOrange()
    {
        SetMarkerColor(DrawingColor.orange);
    }
    public void SetMarkerBrown()
    {
        SetMarkerColor(DrawingColor.brown);
    }
    public void SetMarkerViolet()
    {
        SetMarkerColor(DrawingColor.violet);
    }
    #endregion

    #region pen tool
    /// <summary>
    /// select the pen tool
    /// </summary>
    public void SetPen()
    {
        SetActiveColorTool();
    }

    /// <summary>
    /// set the active color to the selected tool
    /// </summary>
    public void SetActiveColorTool()
    {
        if (DrawFreeHand.Instance.IsEraserActive())
        {
            lastColor.a = transparency;
            SetMarkerColor(lastColor);
        }
        DrawFreeHand.Instance.SetPenBrush();
    }

    /// <summary>
    /// when a tool is deselected, the icon color is reset to the default color
    /// </summary>
    private void ResetSelectedTool()
    {
        var icons = transform.parent.GetComponentsInChildren<Image>(true);
        foreach (var icon in icons)
        {
            if (icon.color == selectedColor)
                icon.color = defaultColor;
        }
    }

    /// <summary>
    /// select the eraser tool
    /// </summary>
    public void SetEraser()
    {
        SetMarkerColor(DrawFreeHand.Instance.Reset_Colour);
    }

    /// <summary>
    /// set semitransparent eraser
    /// </summary>
    public void PartialSetEraser()
    {
        SetMarkerColor(new Color(255f, 255f, 255f, 0.5f));
    }
    #endregion

    #region active drawing type
    /// <summary>
    /// sets if drawing on touch is active or inactive because user interact with drawing tool bar
    /// </summary>
    /// <param name="active">is active</param>
    public void SetActive(bool active)
    {
        DrawingManager.InterfaceInstance.SetAllInactive();
        DrawingState setState = (active ? state : DrawingState.Inactive);

        switch (drawingType)
        {
            case DrawingType.None:
                break;
            case DrawingType.FreeHand:
                DrawFreeHand.Instance.ActiveDrawingState = setState;
                break;
            case DrawingType.Image:
                SetActiveColorTool();
                DrawImage.Instance.ActiveDrawingState = setState;
                break;
            case DrawingType.Text:
                SetActiveColorTool();
                DrawText.Instance.ActiveDrawingState = setState;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// is drawing on touch is active or inactive because user interact with drawing tool bar
    /// </summary>
    /// <returns>is active</returns>
    public bool isActive()
    {
        DrawingState getState = DrawingState.Inactive;

        switch (drawingType)
        {
            case DrawingType.None:
                break;
            case DrawingType.FreeHand:
                getState = DrawFreeHand.Instance.ActiveDrawingState;
                break;
            case DrawingType.Image:
                getState = DrawImage.Instance.ActiveDrawingState;
                break;
            case DrawingType.Text:
                getState = DrawText.Instance.ActiveDrawingState;
                break;
            default:
                break;
        }

        return (getState != DrawingState.Inactive);
    }

    /// <summary>
    /// change the active drawing tool (free hand, pictogram or text)
    /// </summary>
    /// <param name="type">which drawing tool</param>
    /// <param name="state">in which state (add or edit)</param>
    public void SetActiveDrawingType(DrawingType type, DrawingState state = DrawingState.Add)
    {
        this.state = state;
        drawingType = type;
        SetActive(true);
        markActiveDrawingToolIcon();
    }

    /// <summary>
    /// the active drawing tool is identified by the active drawing color
    /// </summary>
    private void markActiveDrawingToolIcon()
    {
        activePenColor.color = defaultColor;
        activeImageColor.color = defaultColor;
        activeTextColor.color = defaultColor;

        if (showSelectedColor)
        {
            switch (drawingType)
            {
                case DrawingType.None:
                    break;
                case DrawingType.FreeHand:
                    activePenColor.color = showSelectedColor.color;
                    break;
                case DrawingType.Image:
                    activeImageColor.color = showSelectedColor.color;
                    break;
                case DrawingType.Text:
                    activeTextColor.color = showSelectedColor.color;
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// change the active drawing tool  to free hand
    /// </summary>
    public void SetActiveDrawingTypeFreeHand()
    {
        SetActiveDrawingType(DrawingType.FreeHand);
    }

    /// <summary>
    /// change the active drawing tool  to image
    /// </summary>
    public void SetActiveDrawingTypeImage()
    {
        SetActiveDrawingType(DrawingType.Image);
    }

    /// <summary>
    /// change the active drawing tool  to text
    /// </summary>
    public void SetActiveDrawingTypeText()
    {
        SetActiveDrawingType(DrawingType.Text);
    }
    #endregion

    #region image properties
    /// <summary>
    /// draw a new image on the drawing canvas
    /// </summary>
    /// <param name="value">which image should be drawn</param>
    public void setSprite(Sprite value)
    {
        DrawImage.Instance.setSprite(value);
    }

    /// <summary>
    /// Pictograms can be raised in different ways on the canvas. Define how to draw them.
    /// </summary>
    public void setImageDragAndDropDrawingMode(DragAndDropDrawingMode value)
    {
        DrawImage.Instance.setDragAndDropDrawingMode(value);
    }
    public void setImageDragAndDropDrawingModeCorners()
    {
        setImageDragAndDropDrawingMode(DragAndDropDrawingMode.Corners);
    }
    public void setImageDragAndDropDrawingModeCenterToEdege()
    {
        setImageDragAndDropDrawingMode(DragAndDropDrawingMode.CenterToEdge);
    }
    #endregion

    #region text
    /// <summary>
    /// draw a new text on the drawing canvas
    /// </summary>
    /// <param name="text"></param>
    public void setText(string text)
    {
        DrawText.Instance.setText(text);
    }
    #endregion

    #region edit
    /// <summary>
    /// sets if moving on touch is active or inactive because user interact with drawing tool bar
    /// </summary>
    /// <param name="active">is active</param>
    /// <param name="uiElement">select ui element which should be edited</param>
    public void EditActive(bool active, RectTransform uiElement)
    {
        SetActive(active);

        if (state == DrawingState.Move)
        {
            switch (drawingType)
            {
                case DrawingType.None:
                    break;
                case DrawingType.FreeHand:
                    break;
                case DrawingType.Image:
                    if (uiElement) DrawImage.Instance.editUIElement(uiElement);
                    break;
                case DrawingType.Text:
                    if (uiElement) DrawText.Instance.editUIElement(uiElement);
                    break;
                default:
                    break;
            }
        }
    }

    #endregion

    #region state
    /// <summary>
    /// active drawing state (add or move)
    /// </summary>
    public DrawingState State
    {
        get
        {
            return state;
        }
    }

    /// <summary>
    /// moving mode for given ui element active
    /// </summary>
    /// <param name="uiElement">select ui element which should be edited</param>
    public void BeginMove(RectTransform uiElement)
    {
        if (uiElement.GetComponentInChildren<Text>())
            SetActiveDrawingType(DrawingType.Text, DrawingState.Move);
        else
            SetActiveDrawingType(DrawingType.Image, DrawingState.Move);
        EditActive(true, uiElement);
    }
    #endregion
}