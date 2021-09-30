#if Vuforia
using UnityEngine;
using System.Collections;
using Vuforia;

/// <summary>
/// Positions the anchor at a fixed distance from the camera,
/// only if device tracking is stable
/// </summary>
/// [RequireComponent(typeof(AnchorPointManager))]
public class VuforiaScreenPoseConverter : ScreenPoseConverter
{
    public float Depth = 0.5f;


    TrackableBehaviour.Status StatusCached = TrackableBehaviour.Status.NO_POSE;
    TrackableBehaviour.StatusInfo StatusInfoCached = TrackableBehaviour.StatusInfo.UNKNOWN;

    void Start()
    {
        DeviceTrackerARController.Instance.RegisterDevicePoseStatusChangedCallback(OnDevicePoseStatusChanged);
    }

    void OnDestroy()
    {

        DeviceTrackerARController.Instance.UnregisterDevicePoseStatusChangedCallback(OnDevicePoseStatusChanged);
    }

    void OnDevicePoseStatusChanged(TrackableBehaviour.Status status, TrackableBehaviour.StatusInfo statusInfo)
    {
        StatusCached = status;
        StatusInfoCached = statusInfo;
    }


    public override bool TryGetPoseAndDistance(float screenPosX, float screenPosY, out Pose pose, out Transform plane, out float distance)
    {
        plane = null;
        distance = Depth;

        pose = Pose.identity;

        if (screenPosX < 0 && screenPosY < 0 && screenPosX < Screen.width && screenPosY > Screen.height)
        {
            return false;
        }
        if(!TrackingStatusIsValid())
        {
            return false;
        }

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(screenPosX, screenPosY));
        pose = new Pose(ray.origin + ray.direction * Depth, Quaternion.LookRotation(ray.direction));
        return true;
    }

    public bool TrackingStatusIsValid()
    {
        var statusIsValid = StatusCached == TrackableBehaviour.Status.TRACKED ||
                            StatusCached == TrackableBehaviour.Status.EXTENDED_TRACKED;
        var statusInfoIsValid = StatusInfoCached == TrackableBehaviour.StatusInfo.NORMAL;
        return statusIsValid && statusInfoIsValid;
    }
}
#endif