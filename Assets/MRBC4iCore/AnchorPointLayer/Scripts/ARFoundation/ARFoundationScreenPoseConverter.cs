#if ARFoundation

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using UnityEngine.Experimental.XR;
using System.Linq;

[RequireComponent(typeof(AnchorPointManager))]
public class ARFoundationScreenPoseConverter : ScreenPoseConverter
{
    public ARSessionOrigin sessionOrigin;

    void Start()
    {
        if(sessionOrigin == null)
            sessionOrigin = FindObjectOfType<ARSessionOrigin>();
    }

    public override bool TryGetPoseAndDistance(float screenPosX, float screenPosY, out Pose pose, out Transform plane, out float distance)
    {
        plane = null;
        distance = -1;
        Vector3 touchPoint = new Vector3(screenPosX, screenPosY);
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        if (sessionOrigin)
        {
            if (sessionOrigin.Raycast(touchPoint, hits, TrackableType.All))
            {
                var hitPose = hits[0].pose;
                distance = hits[0].distance;
                var cam = GetComponentInChildren<Camera>();
                if (!cam) cam = CameraHelper.ARCamera;

                pose = new Pose(hitPose.position, cam.transform.rotation);
                return true;
            }
        }

        pose = Pose.identity;
        return false;
    }
}
#endif