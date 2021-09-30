using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// reference detector provides functions for loading anchors based on a 
/// detected reference, e.g. a marker or image target
/// </summary>
public class ReferenceDetector
{

    private List<AnchorPointLoader> loaders;
    private AnchorPointManager anchorManager;

    private List<AnchorPoint> tempAnchors = new List<AnchorPoint>();

    /// <summary>
    /// setup should be called during a Start-method
    /// </summary>
    public void Setup()
    {
        anchorManager = AnchorPointManager.Instance;

        loaders = new List<AnchorPointLoader>();
        foreach (var loader in anchorManager.GetComponents<AnchorPointLoader>())
        {
            loaders.Add(loader);
        }
        
    }


    public AnchorPoint[] LoadOnReferenceDetected(string name, AnchorPoint.AnchorType type, Pose pose)
    {
        // tempAnchors will contain all newly loaded anchors based on the new reference
        tempAnchors.Clear();
        AnchorPointManager.Instance.Loaded += OnAnchorsLoaded;
        
        // test for reference name in all loaders
        foreach (var loader in loaders)
        {
            if(loader.enabled)
            {
                loader.DetectedPossibleAnchorPoint(name, type, pose);
            }
        }

        AnchorPointManager.Instance.Loaded -= OnAnchorsLoaded;

        var loadedAnchors = tempAnchors.ToArray();
        tempAnchors.Clear();
        return loadedAnchors;
    }

    private void OnAnchorsLoaded(IEnumerable<AnchorPoint> anchors)
    {
        tempAnchors.AddRange(anchors);
    }


    public AnchorPoint GetReference(string name)
    {
        foreach (var anchor in anchorManager.GetAllAnchorPoints())
        {
            if (anchor.AnchorName == name)
            {
                return anchor;
            }
        }
        return null;
    }


    public AnchorPoint CreateReference(string name, AnchorPoint.AnchorType type, Pose pose, Transform poseDriver)
    {
        var anchor = anchorManager.AddAnchorPoint(pose.position, pose.rotation, poseDriver);
        anchor.AnchorName = name;
        anchor.Type = type;
        return anchor;
    }


    public AnchorPoint CreateOrAssignReference(string name, AnchorPoint.AnchorType type, Func<Transform> nativeAnchorFunc)
    {
        // check if there is already a reference anchor for this name
        var existingAnchor = GetReference(name);


        if (existingAnchor == null || existingAnchor.PoseDriver == null)
        {
            // create pose driver based on native image anchor
            var nativeAnchor = nativeAnchorFunc();

            if (existingAnchor == null)
            {
                existingAnchor = CreateReference(name, type, nativeAnchor.GetPose(), nativeAnchor);
            }
            else
            {
                existingAnchor.PoseDriver = nativeAnchor;
            }
        }

        return existingAnchor;

    }
}

