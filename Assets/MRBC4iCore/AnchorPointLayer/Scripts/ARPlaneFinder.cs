using UnityEngine;


/// <summary>
/// Interface for an ARPlaneFinder. An AR plane finder gets
/// planes from an AR framework.
/// </summary>
public abstract class ARPlaneFinder : MonoBehaviour
{
    /// <summary>
    /// Try to get a plane from an AR framework.
    /// </summary>
    /// <param name="planePose">Return value, 3D pose of a plane</param>
    /// <returns>Returns true if a plane is available, otherwise false</returns>
    public abstract bool TryGetPlanePose(out Pose planePose);
}


/// <summary>
/// Default plane finder which never returns a plane.
/// This component is used if no other plane finder is available.
/// </summary>
public class NullARPlaneFinder : ARPlaneFinder
{
    public override bool TryGetPlanePose(out Pose planePose)
    {
        planePose = Pose.identity;
        return false;
    }
}
