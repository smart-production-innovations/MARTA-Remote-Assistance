#if ARFoundation

using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(AnchorPointManager))]
public class ARFoundationPlaneFinder : ARPlaneFinder
{
    public ARSessionOrigin sessionOrigin;


    void Start()
    {
        if(sessionOrigin == null)
            sessionOrigin = FindObjectOfType<ARSessionOrigin>();
    }

    public override bool TryGetPlanePose(out Pose planePose)
    {
        planePose = Pose.identity;
        var arPlane = sessionOrigin.GetComponentInChildren<ARPlane>();
        if (arPlane == null)
            return false;

        planePose = arPlane.boundedPlane.Pose;
        return true;
    }
}

#endif