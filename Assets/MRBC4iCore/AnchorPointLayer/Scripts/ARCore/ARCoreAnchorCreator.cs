#if ARCore

using GoogleARCore;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AnchorPointManager))]
public class ARCoreAnchorCreator : AnchorCreator
{
    void Start()
    {
    }

    public override Transform CreatePoseDriver(Pose pose)
    {
        var arCoreAnchor = Session.CreateAnchor(pose);
        return arCoreAnchor.transform;
    }
}

#endif