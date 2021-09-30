using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum LayerType
{
    None,
    FreeHand,
    Image,
    Text
}

/// <summary>
/// Each pictogram and annotation text is temporary save in a separately drawing layer to can be edited later.
/// </summary>
public class DrawingLayer : MonoBehaviour
{
    private static int nextLayerId = 0;
    private static int getNextLayerId()
    {
        DrawingLayer.nextLayerId++;
        return DrawingLayer.nextLayerId;
    }

    #region properties
    private int layerId  = -1;

    public int LayerId
    {
        get
        {
            if (layerId == -1)
                layerId = DrawingLayer.getNextLayerId();
            return layerId;
        }
    }

    private LayerType layerType = LayerType.None;
    public LayerType LayerType
    {
        get
        {
            if (layerType == LayerType.None)
            {
                if (isDefaultLayer)
                    layerType = LayerType.FreeHand;
                else if (PlacementObject.GetComponent<Text>())
                    layerType = LayerType.Text;
                else if (PlacementObject.GetComponent<Image>())
                    layerType = LayerType.Image;
            }
            return layerType;
        }
    }


    //ui element connected with the drawing layer
    public RectTransform placementObject;
    //name of the drawing layer
    private Text uiText;
    /// <summary>
    /// Get the name of the drawing layer
    /// </summary>
    private Text UIText
    {
        get
        {
            if (!uiText)
                uiText = GetComponentInChildren<Text>();
            return uiText;
        }
    }

    /// <summary>
    /// the default drawing layer for free hand drawing can not be deleted by the user
    /// </summary>
    public bool isDefaultLayer
    {
        get
        {
            return (PlacementObject.GetComponent<DrawFreeHand>() != null);
        }
    }

    /// <summary>
    /// Get the drawing object which is connected to this layer
    /// </summary>
    public RectTransform PlacementObject
    {
        get
        {
            if (placementObject == null)
                placementObject = SearchHelper.FindSceneObjectOfType<DrawFreeHand>().GetComponent<RectTransform>();
            return placementObject;
        }
    }
    #endregion

    #region edit
    /// <summary>
    /// initialize the drawing layer.
    /// </summary>
    /// <param name="name">name of the layer</param>
    /// <param name="placementObject">ui element connected with the drawing layer</param>
    public void Init(string name, RectTransform placementObject)
    {
        layerId = DrawingLayer.getNextLayerId();
        rename(name);
        this.placementObject = placementObject;
    }

    /// <summary>
    /// rename the drawing layer
    /// </summary>
    /// <param name="name">new name</param>
    public void rename(string name)
    {
        if (UIText != null && name != null)
            UIText.text = name;
    }

    /// <summary>
    /// delete the drawing layer and its content
    /// </summary>
    public void Delete()
    {
        if (isDefaultLayer)
        {
            //clear free hand drawing by reseting the image to transparent
            DrawFreeHand.Instance.ResetCanvas();
        }
        else
        {
            //destroy the connected ui element and the layer definition
            GameObject.Destroy(PlacementObject.gameObject);
            GameObject.Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// change the position of the layer content ui element
    /// </summary>
    public void Move()
    {
        DrawingSettings.Instance.BeginMove(PlacementObject);
    }
    #endregion

    public override int GetHashCode()
    {
        return LayerId.GetHashCode();
    }

    public override bool Equals(object other)
    {
        if (other is DrawingLayer)
        {
            var otherLayer = (DrawingLayer)other;
            return LayerId.Equals(otherLayer.LayerId);
        }
        return false;
    }
}
