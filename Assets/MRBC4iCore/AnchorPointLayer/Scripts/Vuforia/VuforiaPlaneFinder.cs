#if Vuforia
using UnityEngine;
using Vuforia;

[RequireComponent(typeof(AnchorPointManager))]
public class VuforiaPlaneFinder : ARPlaneFinder
{
    private Pose groundPlane;
    private bool groundPlaneSet = false;

    PlaneFinderBehaviour planeFinder;

    void Start()
    {

        planeFinder = FindObjectOfType<PlaneFinderBehaviour>();
        if(planeFinder != null)
        {
            planeFinder.OnAutomaticHitTest.AddListener(SetGroundPlane);
        }
    }

    void OnDestroy()
    {
        if (planeFinder != null)
        {
            planeFinder.OnAutomaticHitTest.RemoveListener(SetGroundPlane);
        }
    }

    public override bool TryGetPlanePose(out Pose planePose)
    {
        planePose = groundPlane;
        return groundPlaneSet;
    }


    public void SetGroundPlane(HitTestResult result)
    {
        groundPlane.position = result.Position;
        groundPlane.rotation = result.Rotation;
        groundPlaneSet = true;
    }
}
#endif