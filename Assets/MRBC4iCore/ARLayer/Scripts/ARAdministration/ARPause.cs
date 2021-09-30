using UnityEngine;

#if ARFoundation || ARFoundation2 || ARFoundation3
using UnityEngine.XR.ARFoundation;
#elif ARCore
using GoogleARCore;
#elif Vuforia
using Vuforia;
#endif



/// <summary>
/// This class allows to pause and resume the currently active AR-system
/// </summary>
public class ARPause : AManager<ARPause>
{
#if ARFoundation || ARFoundation2 || ARFoundation3
    ARSession arSession;
#elif ARCore
    ARCoreSession arCoreSession;
#elif Vuforia
#endif

    private void Start()
    {
        FindARSession();
    }

    /// <summary>
    /// pause the active AR-system
    /// </summary>
    public void PauseAR()
    {
        PauseResumeAR(false);
    }

    /// <summary>
    /// resume the active AR-system
    /// </summary>
    public void ResumeAR()
    {
        PauseResumeAR(true);
    }

    /// <summary>
    /// pause or resume the active AR system
    /// </summary>
    /// <param name="arEnabled">true: AR mode, false: non-AR mode</param>
    private void PauseResumeAR(bool arEnabled)
    {
        if (!this.isActiveAndEnabled)
            return;

        FindARSession();

#if ARFoundation || ARFoundation2 || ARFoundation3
        if (arSession != null)
        {
            arSession.enabled = arEnabled;
        }
#elif ARCore
        if (arCoreSession != null)
        {
            arCoreSession.enabled = arEnabled;
            var renderer = arCoreSession.GetComponentInChildren<ARCoreBackgroundRenderer>();
            if (renderer != null)
                renderer.enabled = arEnabled;
        }
#elif Vuforia
        if(arEnabled)
            CameraDevice.Instance.Start();
        else
            CameraDevice.Instance.Stop();
#endif
    }

    /// <summary>
    /// get access to the AR session
    /// </summary>
    private void FindARSession()
    {
#if ARFoundation || ARFoundation2 || ARFoundation3
        if (arSession != null)
            arSession = FindObjectOfType<ARSession>();
#elif ARCore
        if(arCoreSession == null)
            arCoreSession = FindObjectOfType<ARCoreSession>();
#endif
    }
}

