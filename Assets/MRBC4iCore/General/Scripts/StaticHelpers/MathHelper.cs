using System;
using UnityEngine;

/// <summary>
/// mathematical helper scripts
/// </summary>
public static class MathHelper
{
    /// <summary>
    /// concert a pose tho a matrix
    /// </summary>
    /// <param name="pose"></param>
    /// <returns></returns>
    public static Matrix4x4 ToMatrix(this Pose pose)
    {
        return Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one);
    }

    /// <summary>
    /// convert a matrix to a pose
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static Pose ToPose(this Matrix4x4 matrix)
    {
        return new Pose(matrix.GetColumn(3), matrix.rotation);
    }

    /// <summary>
    /// concert a transform to a pose
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static Pose GetPose(this Transform transform)
    {
        return new Pose(transform.position, transform.rotation);
    }

    /// <summary>
    /// inverts the orientation of a pose
    /// </summary>
    /// <param name="lhs"></param>
    /// <returns></returns>
    public static Pose Inverse(this Pose lhs)
    {
        var inverseRotation = Quaternion.Inverse(lhs.rotation);
        var inversePOosition = -(inverseRotation * lhs.position);
        return new Pose(inversePOosition, inverseRotation);
    }

}