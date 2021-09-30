using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the change between AR and non AR mode
/// </summary>
public class ARModeManager : AManager<ARModeManager>
{
    public GameObject arCameraLayer;
    public GameObject webCameraLayer;
    public SnackbarController snackbar;
    private int modeChangedCount = 0;

    public bool IsARModeActive
    {
        get
        {
            return arCameraLayer.activeSelf;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        SetARMode(StatusProperties.Values.ARActive, displaySnackbar: false);
    }

    /// <summary>
    /// sets the active mode
    /// </summary>
    /// <param name="active">true: AR mode, false: non-AR mode</param>
    public void SetARMode(bool active, bool displaySnackbar = true)
    {
        if (IsARModeActive != active)
        {
            if (arCameraLayer) arCameraLayer.SetActive(active);
            if (webCameraLayer) webCameraLayer.SetActive(!active);

            if (snackbar && displaySnackbar && modeChangedCount > 0)
            {
                snackbar.Text = (active ? "MR on" : "MR off");
                snackbar.gameObject.SetActive(true);
            }

            if (displaySnackbar) modeChangedCount++;
        }
    }
}
