#if ARFoundation2

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(AnchorPointManager))]
public class ARFoundation2ScreenPoseConverter : ScreenPoseConverter
{
    public ARRaycastManager raycastManager;

    void Start()
    {
        if (raycastManager == null)
            raycastManager = FindObjectOfType<ARRaycastManager>();
    }

    public override bool TryGetPoseAndDistance(float screenPosX, float screenPosY, out Pose pose, out Transform plane, out float distance)
    {
        plane = null;
        distance = -1;
        if (screenPosX < 0 && screenPosY < 0 && screenPosX < Screen.width && screenPosY > Screen.height)
        {
            pose = Pose.identity;
            return false;
        }

        Vector2 touchPoint = new Vector2(screenPosX, screenPosY);
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(touchPoint, hits, TrackableType.All))
        {
            var hitPose = hits[0].pose;
            distance = hits[0].distance;

            pose = new Pose(hitPose.position, CameraHelper.ARCamera.transform.rotation);
            return true;
        }

        pose = Pose.identity;
        return false;
    }
}
#endif