using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// synchronize waiting key display text with key input field
/// </summary>
[RequireComponent(typeof(Text))]
public class SynchronizeText : MonoBehaviour
{
    public InputField syncField;

    private void OnEnable()
    {
        if (syncField)
            GetComponent<Text>().text = syncField.text;
    }
}
