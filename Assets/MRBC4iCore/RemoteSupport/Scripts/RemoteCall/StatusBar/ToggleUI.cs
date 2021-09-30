using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// manage the color of the toggle button depending on the toggle state
/// </summary>
[RequireComponent(typeof(Image))]
public class ToggleUI : MonoBehaviour
{
    public bool activeUI;
    /// <summary>
    /// ui element color depending on the toggle state
    /// </summary>
    public Color UIColor
    {
        get
        {
            return GetComponent<Image>().color;
        }
        set
        {
            GetComponent<Image>().color = value;
        }
    }
}
