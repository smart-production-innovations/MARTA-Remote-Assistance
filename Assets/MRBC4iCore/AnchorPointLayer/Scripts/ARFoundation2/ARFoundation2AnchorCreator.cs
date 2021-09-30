#if ARFoundation2

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(AnchorPointManager))]
public class ARFoundation2AnchorCreator : AnchorCreator
{
    public ARSessionOrigin sessionOrigin;
    private ARReferencePointManager pointMgr;
    private ARReferencePointManager PointMgr
    {
        get
        {
            if (!pointMgr)
            {
                pointMgr = sessionOrigin.GetComponent<ARReferencePointManager>();
                if (!pointMgr) pointMgr = SearchHelper.FindSceneObjectOfType<ARReferencePointManager>();
                if (!pointMgr) pointMgr = sessionOrigin.gameObject.AddComponent<ARReferencePointManager>();
            }
            return pointMgr;
        }
    }

    //private Dictionary<Transform, ARReferencePoint> refPointDict;

    void Start()
    {
        if (sessionOrigin == null)
            sessionOrigin = FindObjectOfType<ARSessionOrigin>();
    }

    public override Transform CreatePoseDriver(Pose pose)
    {
        var point = PointMgr.AddReferencePoint(pose);
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
            var refPoint = oldReference.GetComponent<ARReferencePoint>();
            if (refPoint)
                PointMgr.RemoveReferencePoint(refPoint);
        }
        var point = PointMgr.AddReferencePoint(pose);
        if (point != null)
            return point.transform;
        return null;
    }
}

#endif