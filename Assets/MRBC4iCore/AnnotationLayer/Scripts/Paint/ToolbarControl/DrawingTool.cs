using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Highlight all tools in the Drawing Toolbar.
/// Define the default state of this tools.
/// </summary>
public class DrawingTool : MonoBehaviour
{
    public bool activeWhenDrawingStarts = false;

    private void OnEnable()
    {
        if (activeWhenDrawingStarts && GetComponent<Toggle>())
        {
            GetComponent<Toggle>().isOn = true;
        }
    }
}
