using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Annotation content script for anchor point.
/// Projection of the 2d drawing onto a plane in 3d space. This plane can be a reconstructed AR plane or a plane parallel to the camera.
/// Calculate a new mesh geometry for each annotation. The mesh calculates the distortion for the transformation from 2d to 3d space.
/// Recommended projection type.
/// </summary>
[RequireComponent(typeof(Snapshot))]
public class AnchorAnnotationProjection3D : AnchorImage3D, IAnchorAnnotationBase
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

    protected Snapshot snapshot;
    protected bool isEmpty;

    public AnnotationOwner AnnotationOwner { get; set; } = AnnotationOwner.Client;

    public RawImage NonARDisplay { get; set; }


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

    private Vector3 displayRotationLocal = Vector3.zero, displayRotation = Vector3.zero;
    private bool firstCall = true;
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
        snapshot.Instantiate();
        isEmpty = true;
    }

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

        if (NonARDisplay)
        {
            NonARDisplay.texture = tex;
        }
    }

    /// <summary>
    /// Get the screenshot which is connected to the annotation
    /// </summary>
    /// <returns></returns>
    public Snapshot GetSnapshot()
    {
        if (snapshot == null)
            snapshot = GetComponent<Snapshot>();
        return snapshot;
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
    /// Set the position of the selected AR projection plane
    /// </summary>
    /// <param name="planePosition">position of the plane</param>
    /// <param name="planeRotation">rotation of the plane</param>
    public void SetPlane(Transform plane)
    {
        PlanePosition = plane.localPosition;
        PlaneRotation = plane.localEulerAngles;
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
        PointRotation = pointRotation + new Vector3(-90, 0, 0);
        SetProjectionTarget(ProjectionTarget.Point);
    }

    /// <summary>
    /// Manual switching between plane and feature point projection.
    /// Calculate a new mesh geometry. The mesh calculates the distortion for the 2d projection on the selected 3d plane.
    /// </summary>
    /// <param name="projectionTarget">plane or feature point</param>
    public void SetProjectionTarget(ProjectionTarget projectionTarget)
    {
        this.projectionTarget = projectionTarget;
        if (StatusProperties.Values.ARActive)
        {
            if (StatusProperties.Values.ExpertHasProjectionLayerOption)
            {
                switch (projectionTarget)
                {
                    case ProjectionTarget.Point:
                        // Calculate a new mesh geometry. The mesh calculates the scaling for the 2D projection anchored in the feater point parallel to the camera.
                        ProjectionMapper.Instance.generateNewMesh(PointPosition, PointRotation, ProjectorPosition, ProjectorRotation, ProjectedTexture,
                           Anchor, GetComponent<MeshFilter>());
                        break;
                    case ProjectionTarget.Plane:
                        // Calculate a new mesh geometry. The mesh calculates the distortion for the 2d projection on the selected 3d plane.
                        ProjectionMapper.Instance.generateNewMesh(PlanePosition, PlaneRotation, ProjectorPosition, ProjectorRotation, ProjectedTexture,
                           Anchor, GetComponent<MeshFilter>());
                        break;
                    default:
                        break;
                }
            }
            else
            {
                // Calculate the default mesh geometry for automatic plane selection.
                ProjectionMapper.Instance.generateNewMesh(PointPosition, PointRotation, ProjectorPosition, ProjectorRotation, ProjectedTexture,
                   Anchor, GetComponent<MeshFilter>());
            }
        }
    }

    /// <summary>
    /// Save the annotation unchanged as a projection texture.  
    /// The individually calculated mesh includes the distortion. This eliminates the calculation intensive texture projection.
    /// </summary>
    /// <param name="permanentSave">permanent or temporary save of the annotation</param>
    protected void CreateProjection(bool permanentSave = true)
    {
        base.SetTexture(ProjectedTexture, permanentSave);
    }
    #endregion
}
