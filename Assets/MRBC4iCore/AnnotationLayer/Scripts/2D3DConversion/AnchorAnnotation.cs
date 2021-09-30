using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One of several implementations for using an annotation as anchor point content.
/// Anchor the drawing to a feature point. The annotation is scaled according to the distance between camera and feature point.
/// Deprecated projection type. Use the class AnchorAnnotationProjection3D instead.
/// </summary>
public class AnchorAnnotation : AnchorAnnotationDistanceScale
{
    #region properties
    /// <summary>
    /// save screen clicking pivot position for later editing and calculation of drawing overlay position
    /// </summary>
    public override Vector2 ScreenClickCenterPivotPosition
    {
        set
        {
            base.ScreenClickCenterPivotPosition = value;
            DrawingAnnotationManager.Instance.OverlayAnchorPosition = value;
        }
    }
    #endregion

    #region unity loop
    /// <summary>
    /// initialize a new annotation
    /// </summary>
    protected override void instantiate()
    {
        base.instantiate();

        //activate drawing tools
        DrawingAnnotationManager.Instance.DrawingActive = true;
        if (DrawingManager.InterfaceInstance.ActiveAnchorId == anchorPoint.Id)
            DrawingAnnotationManager.Instance.SetSnapshotTexture(snapshot.SnapshotTexture);
    }

    void Update()
    {
        //if annotation is selected, open annotation drawing for editing
        if (anchorPoint.IsSelected && !DrawingAnnotationManager.Instance.DrawingActive)
        {
            DrawingAnnotationManager.Instance.DrawingActive = true;
            if (DrawingManager.InterfaceInstance.ActiveAnchorId == anchorPoint.Id)
                DrawingAnnotationManager.Instance.SetSnapshotTexture(snapshot.SnapshotTexture);
            AnnotationManager.Instance.LoadImageFromAnchor(this);
        }
    }
    #endregion
}
