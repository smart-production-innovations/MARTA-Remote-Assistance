using UnityEngine;
using System.Collections;

/// <summary>
/// Converts a screen position to a 3D pose, e.g. based on 3D feature points
/// and planes of an AR framework
/// </summary>
public abstract class ScreenPoseConverter : MonoBehaviour
{
    /// <summary>
    /// Try to convert a screen position into a 3D pose
    /// </summary>
    /// <param name="screenPosX">Screen position in pixels</param>
    /// <param name="screenPosY">Screen position in pixels</param>
    /// <param name="pose">Return value, 3D pose</param>
    /// <returns>Returns true, if position can be coverted.</returns>
    public virtual bool TryGetPose(float screenPosX, float screenPosY, out Pose pose, out Transform plane)
    {
        float distance = -1;
        return TryGetPoseAndDistance(screenPosX, screenPosY, out pose, out plane, out distance);
    }
    public abstract bool TryGetPoseAndDistance(float screenPosX, float screenPosY, out Pose pose, out Transform plane, out float distance);
}


