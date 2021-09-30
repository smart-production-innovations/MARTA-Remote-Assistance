using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// static extent for class ToggleGroup
/// </summary>
public static class ToggleGroupExtensions
{

    private static System.Reflection.FieldInfo _toggleListMember;

    /// <summary>
    /// Gets the list of toggles. Do NOT add to the list, only read from it.
    /// </summary>
    /// <param name="grp">ToggleGroup</param>
    /// <returns></returns>
    public static IList<Toggle> GetToggles(this ToggleGroup grp)
    {
        return grp.GetComponentsInChildren<Toggle>().Where(x => x.group == grp).ToList();
    }

    /// <summary>
    /// Get the count of toggle items connected to this ToggleGroup.
    /// </summary>
    /// <param name="grp">ToggleGroup</param>
    /// <returns></returns>
    public static int Count(this ToggleGroup grp)
    {
        return GetToggles(grp).Count;
    }

    /// <summary>
    /// Get the toggle item at the given index connected to this ToggleGroup.
    /// </summary>
    /// <param name="grp">ToggleGroup</param>
    /// <param name="index">index</param>
    /// <returns></returns>
    public static Toggle Get(this ToggleGroup grp, int index)
    {
        return GetToggles(grp)[index];
    }

    /// <summary>
    /// Set the active toggle item within a ToggleGroup.
    /// </summary>
    /// <param name="grp">ToggleGroup</param>
    /// <param name="value">active item</param>
    public static void SetActiveToggle(this ToggleGroup grp, int value)
    {
        var list = grp.GetToggles();
        foreach (var item in list)
        {
            var valObj = item.GetComponent<ToggleValue>();
            if (valObj && valObj.value == value) valObj.isOn = true;
            else if (grp.allowSwitchOff) valObj.isOn = false;
        }
    }
}

/// <summary>
/// bandwidth parameter types
/// </summary>
public enum BandwidthParameter
{
    Quality,
    FPS
}

/// <summary>
/// values connected to a ToggleGroup
/// </summary>
[RequireComponent(typeof(Toggle))]
public class ToggleValue : MonoBehaviour
{
    #region properties
    public BandwidthParameter bandwidthParameter;
    public int value;
    public Text label;
    public Text selection;

    /// <summary>
    /// connected toggle component
    /// </summary>
    private Toggle toggle;
    public Toggle Toggle
    {
        get
        {
            if (toggle == null)
            {
                toggle = GetComponent<Toggle>();
                toggle.onValueChanged.AddListener(ToggleValueChanged);
            }
            return toggle;
        }
    }

    /// <summary>
    /// is toggle value active
    /// </summary>
    public bool isOn
    {
        get
        {
            return Toggle.isOn;
        }
        set
        {
            Toggle.isOn = value;
        }
    }
    #endregion

    #region unity loop
    void Awake()
    {
        var t = Toggle;
    }

    private void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(ToggleValueChanged);
    }
    #endregion

    #region edit
    /// <summary>
    /// Output the new state of the Toggle into Text
    /// </summary>
    /// <param name="isOn"></param>
    void ToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            selection.text = label.text;

            if (gameObject.activeInHierarchy)
            {
                if (toggle.group)
                    toggle.group.gameObject.SetActive(false);

                var bandwidthManager = GameObject.FindObjectOfType<BandwidthManager>();
                if (bandwidthManager)
                {
                    bandwidthManager.Properties.SetActive(true);
                    switch (bandwidthParameter)
                    {
                        case BandwidthParameter.Quality:
                            bandwidthManager.SetImageQuality(value);
                            break;
                        case BandwidthParameter.FPS:
                            bandwidthManager.SetFPS(value);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
    #endregion
}
