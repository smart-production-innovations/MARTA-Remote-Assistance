using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// manages drawing of images like pictograms into the drawing area
/// </summary>
public class DrawImage : DrawUIElement<DrawImage>
{
    public Sprite sprite;
    /// <summary>
    /// define the sprite pictogram which will be created by the next touch action
    /// </summary>
    /// <param name="value">sprite image</param>
    public void setSprite(Sprite value)
    {
        sprite = value;
    }

    /// <summary>
    /// create a new image element into the drawing area
    /// </summary>
    protected override void createUIElement()
    {
        base.createUIElement();
        var img = currentImageDrawing.GetComponent<Image>();
        if (img)
        {
            img.sprite = sprite;
            img.color = DrawingColor;
        }

        currentDrawingLayer.rename("Image: " + sprite.name);
    }

    protected override DrawingLayer[] GetDrawingLayers()
    {
        var list = base.GetDrawingLayers();
        list = list.Where(x => x.LayerType == LayerType.Image).ToArray();
        return list;
    }
}
