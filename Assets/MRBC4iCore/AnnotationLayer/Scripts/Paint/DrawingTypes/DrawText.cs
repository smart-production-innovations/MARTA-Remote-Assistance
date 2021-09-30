using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// manages drawing of texts into the drawing area
/// </summary>
public class DrawText : DrawUIElement<DrawText> 
{
    public string text;
    /// <summary>
    /// define the text which will be created by the next touch action
    /// </summary>
    /// <param name="value">text string</param>
    public void setText(string value)
    {
        text = value;
    }

    /// <summary>
    /// create a new text element into the drawing area
    /// </summary>
    protected override void createUIElement()
    {
        base.createUIElement();
        var txt = currentImageDrawing.GetComponent<Text>();
        if (txt)
        {
            txt.color = DrawingColor;
            txt.text = text;
        }

        currentDrawingLayer.rename("Text: " + text);
    }

    protected override DrawingLayer[] GetDrawingLayers()
    {
        var list = base.GetDrawingLayers();
        list = list.Where(x => x.LayerType == LayerType.Text).ToArray();
        return list;
    }
}
