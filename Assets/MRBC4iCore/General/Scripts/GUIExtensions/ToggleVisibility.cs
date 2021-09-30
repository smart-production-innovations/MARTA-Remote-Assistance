using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// toggles between the visibility of a game object
/// </summary>
public class ToggleVisibility : MonoBehaviour
{
    /// <summary>
    /// defines whether the status should be inverted, which affects the UI behavior
    /// </summary>
    public bool invert = false;

    /// <summary>
    /// change the visibility of the game object
    /// </summary>
    /// <param name="setActive">new visibility state</param>
    public void toggle(bool setActive)
    {
        if (invert)
            setActive = !setActive;

        gameObject.SetActive(setActive);
    }
}
