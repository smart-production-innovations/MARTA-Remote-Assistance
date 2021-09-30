using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if ARCore
using GoogleARCore;
#endif


/// <summary>
/// Load anchor points from a file, either automatically, manually, or
/// when the pose of a known anchor has been found (e.g. an image target).
/// It's only possible to load the file once. The component is automatically
/// disabled after the anchors have been loaded.
/// </summary>
[RequireComponent(typeof(AnchorPointManager))]
public class AnchorPointLoader : MonoBehaviour
{

    /// <summary>
    /// anchor points are loaded automatically after start-up
    /// without registration to current world coordinate system
    /// </summary>
    public bool AutoLoadWithoutRegistration = false;


    /// <summary>
    /// File name in default location, can't be changed at runtime.
    /// </summary>
    public string FileName;

    /// <summary>
    /// File path 
    /// </summary>
    public string FilePath { get; set; }


    #region Private Members

    private AnchorPointManager anchorPointManager;

    /// <summary>
    /// anchors have been loaded already.
    /// </summary>
    private bool loadedAnchors = false;

    /// <summary>
    /// wait a few frames before loading to ensure everything is setup correctly.
    /// </summary>
    private int waitFrames = 5;
    private int count = 0;

    private List<SerializableAnchorPoint> preloadedAnchors;

    private List<int> loadedAnchorIds = new List<int>();

    #endregion


    #region Unity

    private void Awake()
    {
        SetDefaultFilePath(FileName);
    }

    private void Start()
    {
        anchorPointManager = GetComponent<AnchorPointManager>();
    }

    private void Update()
    {
        bool arIsTracking = true;

#if ARCore
        if(SystemInfo.deviceType != DeviceType.Desktop)
            arIsTracking = Session.Status == SessionStatus.Tracking;
#endif

        if(AutoLoadWithoutRegistration)
        {
            if(arIsTracking && count >= waitFrames)
            {
                Load();
            }
            count++;
        }
    }

    #endregion

    #region Public

    public void SetDefaultFilePath(string filename)
    {
        FilePath = Path.Combine(Application.persistentDataPath, filename);
    }

    /// <summary>
    /// Manually load the anchor points from file.
    /// There is no registration with the current world coordinate system.
    /// </summary>
    public void Load()
    {
        LoadAnchors(-1, Pose.identity);
    }

    public void Load(int anchorId, Pose currentAnchorPose)
    {
        LoadAnchors(anchorId, currentAnchorPose);
    }


    /// <summary>
    /// Check if detected anchor point is part of the anchors in the file.
    /// If yes, all anchors are loaded and transformed based on the current pose of the anchor.
    /// </summary>
    /// <param name="name">Name of detected anchor</param>
    /// <param name="type">Type of detected anchor</param>
    /// <param name="pose">Current pose of detected anchor</param>
    /// <returns>Return true if anchors are loaded</returns>
    public bool DetectedPossibleAnchorPoint(string name, AnchorPoint.AnchorType type, Pose pose)
    {
        // do nothing if component is disabled
        if (!enabled)
            return false;

        // preload anchors, but do not activate in anchor point manager
        if (preloadedAnchors == null)
            PreLoad();

        // if any of the preloaded anchors has the detected name and type,
        // load anchors for real in anchor point manager
        foreach(var anchor in preloadedAnchors)
        {
            if(anchor.Name == name && anchor.Type == type)
            {
                return LoadAnchors(anchor.Id, pose);
            }
        }

        return false;
    }

    public bool HasAnchorPointOfType(AnchorPoint.AnchorType type)
    {
        // preload anchors, but do not activate in anchor point manager
        if (preloadedAnchors == null)
            PreLoad();

        foreach (var anchor in preloadedAnchors)
        {
            if (anchor.Type == type)
            {
                return true;
            }
        }
        return false;
    }

    #endregion


    #region Private


    private void PreLoad()
    {
        preloadedAnchors = AnchorPointManager.PreLoadAnchorPoints(FilePath).ToList();
    }

    private bool LoadAnchors(int anchorId, Pose currentPose)
    {
        // make sure that anchors are only loaded once
        if (enabled && !loadedAnchors)
        {
            loadedAnchors = true;
            anchorPointManager.LoadAnchorPoints(FilePath, anchorId, currentPose, true);

            // disable and destroy loader-component
            this.enabled = false;
            //Destroy(this);

            return true;
        }
        return false;
    }

    #endregion

}

