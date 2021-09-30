#if ARFoundation2

using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(AnchorPointManager))]
public class ARFoundation2PlaneFinder : ARPlaneFinder
{
    public ARPlaneManager planeManager;


    void Start()
    {
        if (planeManager == null)
            planeManager = FindObjectOfType<ARPlaneManager>();
    }

    public override bool TryGetPlanePose(out Pose planePose)
    {
        foreach(var plane in planeManager.trackables)
        {
            var rotation = Quaternion.LookRotation(plane.normal);
            planePose = new Pose(plane.center, rotation);
            return true;
        }

        planePose = Pose.identity;
        return false;
    }
}

#endif