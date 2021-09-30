using UnityEngine;


/// <summary>
/// A basic screen pose converter which positions the anchor at a fixed distance from the camera
/// </summary>
public class BasicScreenPoseConverter : ScreenPoseConverter
{
    /// <summary>
    /// Distance from the camera, at which the pose is generated
    /// </summary>
    public float Depth = 1.0f;

    public override bool TryGetPoseAndDistance(float screenPosX, float screenPosY, out Pose pose, out Transform plane, out float distance)
    {
        plane = null;
        distance = -1;
        if (screenPosX < 0 || screenPosY < 0 || screenPosX > Screen.width || screenPosY > Screen.height)
        {
            pose = Pose.identity;
            return false;
        }

        Camera cam = CameraHelper.ARCamera;
        Ray ray = cam.ScreenPointToRay(new Vector3(screenPosX, screenPosY));

        //project to plane parallel to camera near plane:
        distance = Depth / Vector3.Dot(cam.transform.forward, ray.direction);

        pose = new Pose(ray.origin + ray.direction * distance, cam.transform.rotation);
        //pose = new Pose(ray.origin + ray.direction * Depth, cam.transform.rotation);
        return true;
    }
}