using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Extends the functionality of the Unity default toggle component.
/// Trigger the on value changed work flow when the visibility changes.
/// </summary>
[RequireComponent(typeof(Toggle))]
public class ToggleHelper : MonoBehaviour
{
    private bool activeSelf = true;
    private Toggle item;
    /// <summary>
    /// connected UI toggle element
    /// </summary>
    private Toggle Item
    {
        get
        {
            if (!item)
                item = GetComponent<Toggle>();
            return item;
        }
    }

    private void OnDisable()
    {
        // Trigger the value change work flow for deselected if the check-box (toggle button) is hidden.
        activeSelf = gameObject.activeSelf;

        if (!activeSelf && Item && Item.isOn)
        {
            Item.onValueChanged.Invoke(false);
        }
    }

    private void OnEnable()
    {
        // Trigger the value change work flow for the previous selected value if the check-box (toggle button) becomes visible again.
        if (!activeSelf && Item && Item.isOn)
            Item.onValueChanged.Invoke(Item.isOn);

        activeSelf = gameObject.activeSelf;
    }
}
