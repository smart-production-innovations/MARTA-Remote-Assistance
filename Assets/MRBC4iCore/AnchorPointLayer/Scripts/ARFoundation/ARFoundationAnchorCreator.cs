#if ARFoundation

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(AnchorPointManager))]
public class ARFoundationAnchorCreator : AnchorCreator
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

    void Start()
    {
        if (sessionOrigin == null)
            sessionOrigin = FindObjectOfType<ARSessionOrigin>();
    }


    public override Transform CreatePoseDriver(Pose pose)
    {
        var point = PointMgr.TryAddReferencePoint(pose);        
        if(point != null)
            return point.transform;
        return null;
    }
}

#endif