#if ARCore

using GoogleARCore;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AnchorPointManager))]
public class ARCorePlaneFinder : ARPlaneFinder
{
    void Start()
    {
    }

    public override bool TryGetPlanePose(out Pose planePose)
    {
        if (Session.Status == SessionStatus.Tracking)
        {
            List<DetectedPlane> allPlanes = new List<DetectedPlane>();
            Session.GetTrackables<DetectedPlane>(allPlanes, TrackableQueryFilter.All);
            if(allPlanes.Count > 0)
            {
                planePose = allPlanes[0].CenterPose;
                return true;
            }
        }

        planePose = Pose.identity;
        return false;
    }
}

#endif