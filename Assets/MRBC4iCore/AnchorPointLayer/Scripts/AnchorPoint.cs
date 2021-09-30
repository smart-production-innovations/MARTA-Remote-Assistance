using UnityEngine;

/// <summary>
/// An anchor point defines a pose in the 3D space.
/// It's recommended to create new anchor points only with the
/// AnchorPointManager.
/// </summary>
public class AnchorPoint : MonoBehaviour
{


    private void Update()
    {
        if (PoseDriver != null)
        {
            transform.position = PoseDriver.position;
            transform.rotation = PoseDriver.rotation;
        }
    }

    public enum AnchorType
    {
        Standard,
        ImageTarget,
        CameraImage,
        Custom,
        AzureSpatialAnchor,
    }


    /// <summary>
    /// Unique id, defined by the AnchorPointManager
    /// </summary>
    public int Id;
    /// <summary>
    /// Anchor point name. This value can be assigned after
    /// the anchor point has been created.
    /// </summary>
    public string AnchorName;
    /// <summary>
    /// Whether the anchor point is currently selected.
    /// This value is set by the AnchorPointManager.
    /// </summary>
    public bool IsSelected;

    public AnchorType Type;

    /// <summary>
    /// The anchor pose is updated from the pose driver
    /// </summary>
    public Transform PoseDriver;

    /// <summary>
    /// Pose refers to local transform of game object
    /// </summary>
    public Pose Pose
    {
        get { return new Pose(transform.localPosition, transform.localRotation); }
    }

    /// <summary>
    /// Original id when anchor point was loaded from file.
    /// This id is not persistent and won't be available when the anchor point is loaded the next time.
    /// </summary>
    public int OriginalId { get; set; }

}

