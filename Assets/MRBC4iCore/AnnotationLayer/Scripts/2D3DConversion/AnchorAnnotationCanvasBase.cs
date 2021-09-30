using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum AnnotationOwner
{
    Server,
    Client
}

public class ProjectionPlane
{
    public Vector3 PlanePosition { get; set; }
    public Vector3 PlaneRotation { get; set; }
    public Vector3 ProjectorPosition { get; set; }
    public Vector3 ProjectorRotation { get; set; }
    public Texture2D ProjectedTexture { get; set; }
}

/// <summary>
/// Basic methods and properties when using an annotation as anchor point content.
/// </summary>
public interface IAnchorAnnotationBase
{
    /// <summary>
    /// The creator of an annotation.
    /// </summary>
    AnnotationOwner AnnotationOwner { get; set; }


    /// <summary>
    /// Has the annotation a permanent saved content. 
    /// This flag is set by completing the drawing activity with the green check mark button. 
    /// Live drawing does not influence the flag. 
    /// During live drawing, the drawing is temporarily transferred to the anchor, but its status remains empty for permanent storage. 
    /// All annotations with an empty flag are deleted after leaving the drawing activity by the red cancel button.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Get the screenshot which is connected to the annotation
    /// </summary>
    /// <returns></returns>
    Snapshot GetSnapshot();
}

/// <summary>
/// Using an annotation as anchor point content.
/// Basic implementation for displaying an annotation on a default unity canvas game object
/// Deprecated projection type. Use the class AnchorAnnotationProjection3D instead.
/// AnchorAnnotationProjection3D does not inherit from AnchorAnnotationCanvasBase because it does not use AnchorImageCanvas and generates its own mesh.
/// </summary>
[RequireComponent(typeof(Snapshot))]
public class AnchorAnnotationCanvasBase : AnchorImageCanvas, IAnchorAnnotationBase
{
    #region properties
    protected Snapshot snapshot;
    protected bool isEmpty;

    public AnnotationOwner AnnotationOwner { get; set; } = AnnotationOwner.Client;


    /// <summary>
    /// Has the annotation a permanent saved content. 
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            return isEmpty;
        }
    }
    #endregion

    #region unity loop
    /// <summary>
    /// initialize a drawing activity for this annotation
    /// </summary>
    public override void OnDisplayInitEmptyContent()
    {
        base.OnDisplayInitEmptyContent();

        if (DrawingAnnotationManager.HasInstance && DrawingAnnotationManager.Instance.DrawingActive)
        {
            if (DrawingManager.InterfaceInstance.ActiveAnchorId == anchorPoint.Id)
                DrawingAnnotationManager.Instance.SetSnapshotTexture(snapshot.SnapshotTexture);
        }
    }

    /// <summary>
    /// initialize a new annotation
    /// </summary>
    protected override void instantiate()
    {
        base.instantiate();

        //create snapshot to draw over while editing
        snapshot = GetComponent<Snapshot>();
        isEmpty = true;
    }
    #endregion

    #region get/set
    /// <summary>
    /// display the given texture as anchor point content
    /// </summary>
    /// <param name="tex">texture</param>
    public override void SetTexture(Texture2D tex, bool permanentSave = true)
    {
        base.SetTexture(tex, permanentSave);
        if (permanentSave) isEmpty = false;
    }

    /// <summary>
    /// Get the screenshot which is connected to the annotation
    /// </summary>
    /// <returns></returns>
    public Snapshot GetSnapshot()
    {
        if(snapshot == null)
            snapshot = GetComponent<Snapshot>();
        return snapshot;
    }
    #endregion
}
