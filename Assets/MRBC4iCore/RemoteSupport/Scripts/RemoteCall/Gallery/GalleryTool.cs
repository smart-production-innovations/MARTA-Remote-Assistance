using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mark UI elements which are only used or not used in the gallery mode.
/// </summary>
public class GalleryTool : FeatureTool<GalleryTool>
{
    /// <summary>
    /// define the default value of the marker action
    /// </summary>
    protected override bool lastActivatiyCalculationValueInitValue
    {
        get
        {
            return false;
        }
    }
}
