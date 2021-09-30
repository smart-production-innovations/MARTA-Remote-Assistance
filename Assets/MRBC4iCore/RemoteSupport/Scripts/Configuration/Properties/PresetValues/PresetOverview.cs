using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For the user study the comparison of different configurations was planned. 
/// Apart from the test conditions, the presets are used by the expert to store the default settings.
/// Display of all preset options for the selection of the desired configuration at program start by the expert.
/// </summary>
public class PresetOverview : AManager<PresetOverview>
{
    public GameObject displayGameObject;
    public PresetOption preset;

    private void Awake()
    {
        // clear preset list
        foreach (PresetOption child in GetComponentsInChildren<PresetOption>())
        {
            GameObject.Destroy(child.gameObject);
        }

        // load a list of all defined preset options
        foreach (var item in StatusProperties.Values.Presets)
        {
            var presetItem = GameObject.Instantiate(preset, transform);
            presetItem.PresetName = item;
        }
    }

    /// <summary>
    /// hide the preset selection menu
    /// </summary>
    public void Hidde()
    {
        if (displayGameObject)
            displayGameObject.gameObject.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    /// <summary>
    /// show the preset selection menu
    /// </summary>
    public void Show()
    {
        if (displayGameObject)
            displayGameObject.gameObject.SetActive(true);
        else
            gameObject.SetActive(true);
    }
}
