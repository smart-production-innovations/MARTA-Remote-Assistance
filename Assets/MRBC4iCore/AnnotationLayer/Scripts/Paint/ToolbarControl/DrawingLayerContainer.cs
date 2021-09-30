using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// List of ui tool bar drawing layer. Each pictogram and annotation text is temporary save in a separately drawing layer to can be edited later.
/// </summary>
public class DrawingLayerContainer : AManager<DrawingLayerContainer>
{
    #region properties
    //prefab for instantiation of a new drawing layer
    public DrawingLayer layerPrefab;
    #endregion

    #region edit
    /// <summary>
    /// clear all drawing layer except the default layer
    /// </summary>
    public void clear()
    {
        foreach (DrawingLayer child in GetComponentsInChildren<DrawingLayer>())
        {
            if (!child.isDefaultLayer)
            {
                child.Delete();
            }
        }
    }

    /// <summary>
    /// add a new layer
    /// </summary>
    /// <param name="name">layer name</param>
    /// <param name="placementObject">connected ui element</param>
    /// <returns></returns>
    public DrawingLayer add(string name, RectTransform placementObject)
    {
        var layer = GameObject.Instantiate(layerPrefab, transform);
        layer.Init(name, placementObject);
        return layer;
    }

    /// <summary>
    /// add a new layer
    /// </summary>
    /// <param name="placementObject">connected ui element</param>
    /// <returns></returns>
    public DrawingLayer add(RectTransform placementObject)
    {
        return add(null, placementObject);
    }

    /// <summary>
    /// get a list of all DrawingLayers
    /// </summary>
    /// <returns></returns>
    public DrawingLayer[] layerList()
    {
        return GetComponentsInChildren<DrawingLayer>();
    }
    #endregion
}
