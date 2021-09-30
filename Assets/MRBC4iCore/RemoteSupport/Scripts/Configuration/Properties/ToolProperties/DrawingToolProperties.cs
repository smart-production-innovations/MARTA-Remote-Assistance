using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mark UI elements that are only active when the selected flags (ToolFeature) are set to active. 
/// Override the custom action to set the default value of a specific tool in the drawing toolbar.
/// </summary>
[RequireComponent(typeof(DrawingTool))]
public class DrawingToolProperties : ToolProperties
{
    /// <summary>
    /// Define the calculation for the custom type of actions.
    /// The state affects the default value of a specific tool in the drawing toolbar.
    /// The default value is set each time the drawing toolbar gets active (whenever an annotation is created or modified).
    /// </summary>
    /// <param name="state">marker state</param>
    protected override void CustomExecuteMarkerType(bool state)
    {
        var item = GetComponent<DrawingTool>();
        if (item)
        {
            item.activeWhenDrawingStarts = state;
        }
    }
}
