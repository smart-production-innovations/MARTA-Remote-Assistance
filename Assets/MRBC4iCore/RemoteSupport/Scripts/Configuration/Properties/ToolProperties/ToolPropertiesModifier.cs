using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mark UI elements that are only active when the selected flags (ToolFeature) are set to active. 
/// Status properties are not only read to determine the visibility or selection value of this game object, but also modified to affect other game objects.
/// </summary>
public class ToolPropertiesModifier : ToolProperties
{
    /// <summary>
    /// List of status properties that should be modified
    /// </summary>
    public PropertyFlag[] modifyFeature = new PropertyFlag[1];
    /// <summary>
    /// connected toggle UI elements that set the value of the status properties in the modifyFeature list 
    /// </summary>
    private Toggle toggle;

    /// <summary>
    /// modifies the value of the status properties defined in the modifyFeature list 
    /// </summary>
    public bool ModifyFeatrueValue
    {
        get
        {
            bool status = true;
            foreach (var feature in modifyFeature)
            {
                status = (status && StatusProperties.Values.GetKey(feature));
            }
            return status;
        }
        set
        {
            foreach (var feature in modifyFeature)
                StatusProperties.Values.SetKey(feature, value);
        }
    }

    private void Awake()
    {
        // find the connected toggle button
        toggle = GetComponentInChildren<Toggle>();
        if (toggle)
        {
            // listen on the onValueChanged event to modify the value of the status properties defined in the modifyFeature list 
            toggle.onValueChanged.AddListener(ToggleValueChanged);
        }
    }

    private void OnDestroy()
    {
        if (toggle)
        {
            toggle.onValueChanged.RemoveListener(ToggleValueChanged);
        }
    }

    private void OnEnable()
    {
        if (!toggle)
            toggle = GetComponentInChildren<Toggle>();

        if (toggle)
        {
            // listen on the onValueChanged event to modify the value of the status properties defined in the modifyFeature list 
            toggle.isOn = ModifyFeatrueValue;
        }
    }


    /// <summary>
    /// listen on the onValueChanged event to modify the value of the status properties defined in the modifyFeature list 
    /// </summary>
    /// <param name="isOn"></param>
    void ToggleValueChanged(bool isOn)
    {
        ModifyFeatrueValue = isOn;
    }
}
