using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mark UI elements that are only active when the selected flags (ToolFeature) are set to active. 
/// Override the custom action to set the default value of a drop-down.
/// </summary>
[RequireComponent(typeof(Dropdown))]
public class DropdownProperties : ToolProperties
{
    public int selectedIndex = 1;
    public int unselectedIndex = 0;
    public bool invokeOnSet = false;
    private int sessionID = -1;

    /// <summary>
    /// Define the calculation for the custom type of actions.
    /// The state affects the default value of a drop-down.
    /// <param name="state">marker state</param>
    protected override void CustomExecuteMarkerType(bool state)
    {
        var item = GetComponent<Dropdown>();
        if (item)
        {
            if (sessionID != StatusProperties.Values.SessionID)
            {
                item.value = (state ? selectedIndex : unselectedIndex);
                if (invokeOnSet && gameObject.activeSelf && !gameObject.activeInHierarchy)
                    item.onValueChanged.Invoke(item.value);
                sessionID = StatusProperties.Values.SessionID;
            }
        }
    }
}
