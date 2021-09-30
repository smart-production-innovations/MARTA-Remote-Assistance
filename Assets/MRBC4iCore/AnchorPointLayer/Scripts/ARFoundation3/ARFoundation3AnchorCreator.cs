#if ARFoundation3

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(AnchorPointManager))]
public class ARFoundation3AnchorCreator : AnchorCreator
{
    public ARSessionOrigin sessionOrigin;
    private ARAnchorManager pointMgr;
    private ARAnchorManager PointMgr
    {
        get
        {
            if (!pointMgr)
            {
                pointMgr = sessionOrigin.GetComponent<ARAnchorManager>();
                if (!pointMgr) pointMgr = SearchHelper.FindSceneObjectOfType<ARAnchorManager>();
                if (!pointMgr) pointMgr = sessionOrigin.gameObject.AddComponent<ARAnchorManager>();
            }
            return pointMgr;
        }
    }

    void Start()
    {
        if (sessionOrigin == null)
            sessionOrigin = FindObjectOfType<ARSessionOrigin>();
    }

    public override Transform CreatePoseDriver(Pose pose)
    {
        var point = PointMgr.AddAnchor(pose);
        if (point != null)
        {
            return point.transform;
        }
        return null;
    }

    public override Transform ReplacePoseDriver(Pose pose, Transform oldReference)
    {
        if (oldReference != null)
        {
            var refPoint = oldReference.GetComponent<ARAnchor>();
            if (refPoint)
                PointMgr.RemoveAnchor(refPoint);
        }
        var point = PointMgr.AddAnchor(pose);
        if (point != null)
            return point.transform;
        return null;
    }
}

#endif