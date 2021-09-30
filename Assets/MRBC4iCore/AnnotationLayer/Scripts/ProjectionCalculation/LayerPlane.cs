using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manual AR projection plane correction allows the user to choose between all AR planes found within the current camera field of view.
/// Define the visualization content of one AR plane.
/// </summary>
public class LayerPlane : MonoBehaviour
{
    #region properties
    public RawImage uiImage;

    /// <summary>
    /// preview image for plane selection
    /// </summary>
    public Texture LayerTexture
    {
        get { return uiImage.texture; }
        set
        {
            uiImage.texture = value;
        }
    }

    public bool isDefaultLayer { get; set; } = true;
    public Vector3 PlanePosition { get; set; }
    public Vector3 PlaneRotation { get; set; }
    public Toggle layerOptionToggle;
    #endregion

    #region edit
    /// <summary>
    /// select this AR plane as new projection plane
    /// </summary>
    public void LayerSelected()
    {
        var cmdParam = RemoteHelperImage.AnchorId + ";" + isDefaultLayer + ";" + Commands.getCoordinatesString(PlanePosition) + ";" + Commands.getCoordinatesString(PlaneRotation) + ";" + 
            Commands.getCoordinatesString(ARPlaneDisplayManager.Instance.getCameraPosition()) + ";" + Commands.getCoordinatesString(ARPlaneDisplayManager.Instance.getCameraRotation());
        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.ProjectionPlaneSelected, cmdParam));

        ARPlaneDisplayManager.Instance.setDrawingProjectionLocation(PlanePosition, PlaneRotation);
        LayerPlaneContainer.Instance.Display(false);

        if (layerOptionToggle)
            layerOptionToggle.isOn = false;
    }
    #endregion
}
