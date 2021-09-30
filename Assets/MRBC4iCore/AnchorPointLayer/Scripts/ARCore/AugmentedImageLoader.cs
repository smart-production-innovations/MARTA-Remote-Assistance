#if ARCore
using System;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

/// <summary>
/// When an image target is detected, load anchor points (with AnchorPointLoader)
/// The anchor points are always positioned relative to the image target
/// </summary>
public class AugmentedImageLoader : MonoBehaviour
{
    public GameObject AnchorPrefab;

    private List<AugmentedImage> tempAugmentedImages = new List<AugmentedImage>();

    private ReferenceDetector referenceCreator;

    private const AnchorPoint.AnchorType anchorType = AnchorPoint.AnchorType.ImageTarget;

    /// <summary>
    /// store all handled images, because each is only used once for loading or creating an anchor
    /// </summary>
    private List<int> handledImages = new List<int>();

    void Start()
    {
        referenceCreator = new ReferenceDetector();
        referenceCreator.Setup();
    }

    private void Update()
    {
        // Get updated augmented images for this frame.
        Session.GetTrackables(tempAugmentedImages, TrackableQueryFilter.Updated);


        foreach (var image in tempAugmentedImages)
        {

            if(image.TrackingState == TrackingState.Tracking && image.TrackingMethod == AugmentedImageTrackingMethod.FullTracking)
            {
                // image has been used for loading before
                if(handledImages.Contains(image.DatabaseIndex))
                {
                    continue;
                }

                var loadedAnchors = referenceCreator.LoadOnReferenceDetected(image.Name, anchorType, image.CenterPose);

                Func<Transform> nativeAnchorFunc = () => image.CreateAnchor(image.CenterPose).transform;
                var existingAnchor = referenceCreator.CreateOrAssignReference(image.Name, anchorType, nativeAnchorFunc);

                if (existingAnchor != null && AnchorPrefab != null)
                {
                    GameObject.Instantiate(AnchorPrefab, existingAnchor.transform);
                }


                // attach all newly loaded anchors to image target
                if(loadedAnchors != null)
                {
                    foreach(var anchor in loadedAnchors)
                    {
                        if(anchor.PoseDriver == null)
                            anchor.PoseDriver = image.CreateAnchor(anchor.Pose).transform;
                    }
                }

                handledImages.Add(image.DatabaseIndex);
            }

            
        }
    }


}
#endif