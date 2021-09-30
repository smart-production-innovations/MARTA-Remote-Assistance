#if Vuforia
using Vuforia;
using UnityEngine;

/// <summary>
/// Load anchor points from file when the trackable is detected for the first time
/// </summary>
public class TrackableLoader : DefaultTrackableEventHandler
{

    private ReferenceDetector referenceCreator;


    private const AnchorPoint.AnchorType anchorType = AnchorPoint.AnchorType.ImageTarget;

    private bool isTracked;
    private bool handled;

    protected override void Start()
    {
        referenceCreator = new ReferenceDetector();
        referenceCreator.Setup();

        base.Start();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }


    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();

        isTracked = true;


        if (handled)
            return;

        handled = true;

        var trackableName = mTrackableBehaviour.TrackableName;
        var pose = new Pose(mTrackableBehaviour.transform.position, mTrackableBehaviour.transform.rotation);

        var loadedAnchors = referenceCreator.OnReferenceDetected(trackableName, anchorType,pose);



        var existingAnchor = referenceCreator.GetReference(trackableName);

        if (existingAnchor == null || existingAnchor.PoseDriver == null)
        {
            if (existingAnchor == null)
            {
                existingAnchor = referenceCreator.CreateReference(trackableName, anchorType, pose, mTrackableBehaviour.transform);
            }
            else
            {
                existingAnchor.PoseDriver = mTrackableBehaviour.transform;
            }
        }

        // todo: create relative pose drivers for all newly loaded anchors
        /*if (loadedAnchors != null)
        {
            foreach (var anchor in loadedAnchors)
            {
                if (anchor.PoseDriver == null)
                    anchor.PoseDriver = image.CreateAnchor(anchor.Pose).transform;
            }
        }*/
        
    }

    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();
        isTracked = false;
    }

    public bool IsTracked
    {
        get { return isTracked;  }
    }

}
#endif
