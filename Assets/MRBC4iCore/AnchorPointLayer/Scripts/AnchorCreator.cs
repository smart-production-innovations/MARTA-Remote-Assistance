using UnityEngine;


/// <summary>
/// Interface for an Anchor-creator, which creates an anchor
/// in the underlying AR-framework
/// </summary>
public abstract class AnchorCreator : MonoBehaviour
{
    /// <param name="pose">3D pose of anchor</param>
    public abstract Transform CreatePoseDriver(Pose pose);
    public abstract Transform ReplacePoseDriver(Pose pose, Transform oldReference);
}


/// <summary>
/// Default anchor creator doesn't create anything and returns null
/// This component is used if no other anchor creator is available.
/// </summary>
public class NullAnchorCreator : AnchorCreator
{
    public override Transform CreatePoseDriver(Pose pose)
    {
        return null;
    }

    public override Transform ReplacePoseDriver(Pose pose, Transform oldReference)
    {
        return null;
    }

}
