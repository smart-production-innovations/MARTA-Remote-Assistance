using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ProjectionTarget
{
    Point,
    Plane
}

/// <summary>
/// Buffer rotation and position from the default Unity Transform script.
/// </summary>
public class TransformParameter
{
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }

    public TransformParameter(Vector3 position, Vector3 rotation)
    {
        Position = position;
        Rotation = rotation;
    }

    public TransformParameter(Transform transform) : this(transform.position, transform.eulerAngles)
    {
    }
}

/// <summary>
/// One of several implementations for using an annotation as anchor point content.
/// Annotations are projected on a projection plane. The default plane geometry is used and the texture is transformed. This action is very slow.
/// Deprecated projection type. Use the class AnchorAnnotationProjection3D instead.
/// </summary>
[RequireComponent(typeof(Snapshot))]
public class AnchorAnnotationProjection : AnchorAnnotationDistanceScale
{
    #region properties
    protected Vector3 PointPosition;
    protected Vector3 PointRotation;
    protected Vector3 PlanePosition;
    protected Vector3 PlaneRotation;
    protected Vector3 projectorPosition;
    protected Vector3 projectorRotation;
    protected Texture2D ProjectedTexture;
    protected List<ARLayerPlane> detectedPlanes;
    protected ProjectionTarget projectionTarget = ProjectionTarget.Point;

    /// <summary>
    /// position of the projector (camera position from which the snapshot was taken)
    /// </summary>
    public Vector3 ProjectorPosition
    {
        get { return projectorPosition; }
    }

    /// <summary>
    /// rotation of the projector (camera position from which the snapshot was taken)
    /// </summary>
    public Vector3 ProjectorRotation
    {
        get { return projectorRotation; }
    }

    /// <summary>
    /// Found AR planes in space
    /// </summary>
    public List<ARLayerPlane> DetectedPlanes
    {
        get
        {
            if (detectedPlanes == null)
                detectedPlanes = ARFrameworkManager.Instance.ARLayers.GetAllPlanes();

            return detectedPlanes;
        }
    }

    /// <summary>
    /// For the conversion of the 2D click position in the live video into 3D coordinates, 
    /// different distances arise depending on the distance of the click point to the center of the video. 
    /// The distortion is similar to a spherical environment map.
    /// </summary>
    protected Vector3 PivotDelta3D
    {
        get
        {
            Vector3 delta = Vector3.zero;
            delta.x = ScreenClickCenterPivotPosition.x;
            delta.y = ScreenClickCenterPivotPosition.y;
            return delta * DistanceScaleFactor.x;
        }
    }

    private Vector3 displayRotationLocal = Vector3.zero, displayRotation = Vector3.zero;
    private bool firstCall = true;
    public virtual void Update()
    {
        // remember initial rotation
        if (displayRotationLocal != transform.localEulerAngles 
            || displayRotation != transform.eulerAngles
            || firstCall)
        {
            firstCall = false;
            displayRotationLocal = transform.localEulerAngles;
            displayRotation = transform.eulerAngles;
        }
    }
    #endregion

    #region get/set
    /// <summary>
    /// return the screenshot overlay texture
    /// </summary>
    /// <returns></returns>
    public override Texture2D GetTexture()
    {
        return ProjectedTexture;
    }

    /// <summary>
    /// display the given texture as anchor point content
    /// </summary>
    /// <param name="tex">texture</param>
    public override void SetTexture(Texture2D tex, bool permanentSave = true)
    {
        ProjectedTexture = tex;
        CreateProjection(permanentSave);
        if (permanentSave) isEmpty = false;
    }

    /// <summary>
    /// set the position of the projector (camera position from which the snapshot was taken)
    /// </summary>
    /// <param name="projectorPosition">camera position</param>
    /// <param name="projectorRotation">camera rotation</param>
    public void SetProjector(Vector3 projectorPosition, Vector3 projectorRotation)
    {
        this.projectorPosition = projectorPosition;
        this.projectorRotation = projectorRotation;
    }

    /// <summary>
    /// Set the position of the selected AR projection plane
    /// </summary>
    /// <param name="planePosition">position of the plane</param>
    /// <param name="planeRotation">rotation of the plane</param>
    public void SetPlane(Vector3 planePosition, Vector3 planeRotation)
    {
        PlanePosition = planePosition;
        PlaneRotation = planeRotation;
        SetProjectionTarget(ProjectionTarget.Plane);
    }

    /// <summary>
    /// Define the position of the feature point. The annotation is anchored to a feature point if no AR plain is found.
    /// </summary>
    /// <param name="pointPosition">position of the feature point</param>
    /// <param name="pointRotation">rotation of the feature point</param>
    public void SetPoint(Vector3 pointPosition, Vector3 pointRotation)
    {
        PointPosition = pointPosition;
        PointRotation = pointRotation;
        SetProjectionTarget(ProjectionTarget.Point);
    }

    /// <summary>
    /// Manual switching between plane and feature point projection.
    /// </summary>
    /// <param name="projectionTarget">plane or feature point</param>
    public void SetProjectionTarget(ProjectionTarget projectionTarget)
    {
        this.projectionTarget = projectionTarget;
    }

    /// <summary>
    /// Project the 2d annotation from the projector position onto the selected projection plane.
    /// Save the projection to a texture. 
    /// </summary>
    /// <param name="permanentSave">permanent or temporary save of the annotation</param>
    protected void CreateProjection(bool permanentSave = true)
    {
        switch (projectionTarget)
        {
            case ProjectionTarget.Point:
                transform.localScale = DistanceScaleFactor;
                transform.position = PointPosition;
                transform.eulerAngles = PointRotation;
                anchorDisplayImage.transform.localPosition -= PivotDelta3D;
                base.SetTexture(ProjectedTexture, permanentSave);
                break;
            case ProjectionTarget.Plane:
                transform.localScale = DefaultScaleValue;
                ProjectionHelper.Instance.CreateProjection(PlanePosition, PlaneRotation, ProjectorPosition, ProjectorRotation, ProjectedTexture, ref anchorDisplayImage, Anchor);
                break;
            default:
                break;
        }
    }
    #endregion
}
