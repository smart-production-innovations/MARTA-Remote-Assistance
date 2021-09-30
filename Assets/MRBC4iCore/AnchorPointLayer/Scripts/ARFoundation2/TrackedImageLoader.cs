#if ARFoundation2
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// When an image target is detected, load anchor points (with AnchorPointLoader)
/// The anchor points are always positioned relative to the image target
/// </summary>
public class TrackedImageLoader : MonoBehaviour
{
    [Tooltip("Attach Image Manager from the AR Session Origin")]
    public ARTrackedImageManager trackedImageManager;

    public GameObject AnchorPrefab;

    private ReferenceDetector referenceDetector;
    private const AnchorPoint.AnchorType anchorType = AnchorPoint.AnchorType.ImageTarget;


    void Awake()
    {
        if (trackedImageManager == null)
            trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
    }

    void Start()
    {
        referenceDetector = new ReferenceDetector();
        referenceDetector.Setup();
    }


    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
            OnAddedImage(trackedImage);
    }


    private void OnAddedImage(ARTrackedImage trackedImage)
    {
        var poseDriver = trackedImage.gameObject.transform;
        var name = trackedImage.referenceImage.name;
        Debug.Log("Found AR Image " + name);
        var loadedAnchors = referenceDetector.LoadOnReferenceDetected(name, anchorType, poseDriver.GetPose());

        var imageAnchor = referenceDetector.CreateOrAssignReference(name, anchorType, () => poseDriver);


        // create visualization
        if (imageAnchor != null && AnchorPrefab != null)
        {
            GameObject.Instantiate(AnchorPrefab, imageAnchor.transform);
        }

        foreach (var anchor in loadedAnchors)
        {
            if(anchor.PoseDriver == null)
            {
                var newPoseDriver = new GameObject(anchor.name).transform;
                newPoseDriver.parent = poseDriver;
                newPoseDriver.position = anchor.transform.position;
                newPoseDriver.rotation = anchor.transform.rotation;
                anchor.PoseDriver = newPoseDriver;
            }
        }
    }

}

#endif