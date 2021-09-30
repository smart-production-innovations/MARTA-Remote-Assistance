using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// For the user study the comparison of different configurations was planned. 
/// Apart from the test conditions, the presets are used by the expert to store the default settings.
/// The different configuration options are stored as preset options that can be selected at program start. 
/// The presets summarize the configurations to be tested under a selectable name. 
/// </summary>
public class PresetOption : MonoBehaviour
{
    private string presetName;
    /// <summary>
    /// Selectable preset name
    /// </summary>
    public string PresetName
    {
        get { return presetName; }
        set
        {
            presetName = value;
            var text = GetComponentInChildren<Text>();
            if (text)
                text.text = value;
        }
    }

    /// <summary>
    /// load the preset configuration for the preset name
    /// </summary>
    public void loadPreset()
    {
        StatusProperties.Values.loadPreset(presetName, true);
        PresetOverview.Instance.Hidde();
    }
}
