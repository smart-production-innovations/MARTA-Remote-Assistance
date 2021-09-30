using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manual AR projection plane correction allows the user to choose between all AR planes found within the current camera field of view.
/// List of all possible AR planes for a screenshot. 
/// </summary>
public class LayerPlaneContainer : AManager<LayerPlaneContainer>
{
    #region properties
    //Prefab to visualize a found AR plane for manual plane selection
    public LayerPlane layerPrefab;
    public GameObject displayPanel;
    public Toggle layerOptionToggle;
    #endregion

    #region edit
    /// <summary>
    /// clear all AR planes from the previous screenshot.
    /// </summary>
    public void clear()
    {
        foreach (LayerPlane child in GetComponentsInChildren<LayerPlane>())
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// add an AR plane
    /// </summary>
    /// <param name="texture">preview image for plane selection</param>
    /// <returns></returns>
    public LayerPlane add(Texture2D texture)
    {
        var layer = GameObject.Instantiate(layerPrefab, transform);
        layer.LayerTexture = texture;
        layer.layerOptionToggle = layerOptionToggle;
        return layer;
    }

    public LayerPlane add(Texture2D texture, Vector3 position, Vector3 rotation, bool isDefaultLayer = false)
    {
        var layer = add(texture);
        layer.isDefaultLayer = isDefaultLayer;
        layer.PlanePosition = position;
        layer.PlaneRotation = rotation;
        return layer;
    }

    /// <summary>
    /// get a list of all possible AR planes
    /// </summary>
    /// <returns></returns>
    public LayerPlane[] layerList()
    {
        return GetComponentsInChildren<LayerPlane>();
    }

    public void Display(bool value)
    {
        if (displayPanel)
            displayPanel.SetActive(value);
    }
    #endregion
}
