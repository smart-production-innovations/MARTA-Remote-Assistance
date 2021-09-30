#if ARCore

using UnityEngine;
using GoogleARCore;

[RequireComponent(typeof(AnchorPointManager))]
public class ARCoreScreenPoseConverter : ScreenPoseConverter
{

    void Start()
    {

    }

    public override bool TryGetPoseAndDistance(float screenPosX, float screenPosY, out Pose pose, out Transform plane, out float distance)
    {
        plane = null;
        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
            TrackableHitFlags.FeaturePointWithSurfaceNormal;

        if (Frame.Raycast(screenPosX, screenPosY, raycastFilter, out hit))
        {
            var cam = GetComponentInChildren<Camera>();
            if (!cam) cam = CameraHelper.ARCamera;

            pose = new Pose(hit.Pose.position, cam.transform.rotation);
            distance = hit.Distance
            return true;
        }

        pose = Pose.identity;
        distance = -1;
        return false;
    }
}
#endif