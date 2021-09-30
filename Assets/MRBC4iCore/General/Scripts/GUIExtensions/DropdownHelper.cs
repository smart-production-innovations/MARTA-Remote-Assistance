using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Extends the functionality of the Unity default drop-down component.
/// Trigger the on value changed work flow when the visibility changes.
/// </summary>
[RequireComponent(typeof(Dropdown))]
public class DropdownHelper : MonoBehaviour
{
    private bool activeSelf = true;
    private Dropdown item;
    /// <summary>
    /// connected UI drop-down element
    /// </summary>
    private Dropdown Item
    {
        get
        {
            if (!item)
                item = GetComponent<Dropdown>();
            return item;
        }
    }

    private void OnDisable()
    {
        // Trigger the on value changed work flow for the first drop-down entry when the drop-down list is hidden.
        activeSelf = gameObject.activeSelf;
        if (!activeSelf && Item && Item.value != 0)
            Item.onValueChanged.Invoke(0);
    }

    private void OnEnable()
    {
        // Trigger the on value changed work flow for the previous selected value when the drop-down list becomes visible again.
        if (!activeSelf && Item && Item.value != 0)
            Item.onValueChanged.Invoke(Item.value);

        activeSelf = gameObject.activeSelf;
    }
}
