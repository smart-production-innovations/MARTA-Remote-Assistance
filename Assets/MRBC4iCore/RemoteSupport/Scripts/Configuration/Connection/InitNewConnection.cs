using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// manage UI display while connection to a second device
/// </summary>
public class InitNewConnection : MonoBehaviour
{
    public InputText inputPanel;
    public GameObject waitPanel;

    public void OnEnable()
    {
        if (inputPanel)
            inputPanel.ResetText();

        ActivateInput();
    }

    /// <summary>
    /// connection waiting animation is visible
    /// </summary>
    public void ActivateWait()
    {
        Activate(false);
    }

    /// <summary>
    /// insert connection key panel is visible
    /// </summary>
    public void ActivateInput()
    {
        Activate(true);
    }

    /// <summary>
    /// set active connection state. change UI depending on the state.
    /// </summary>
    /// <param name="newInput">If true inset key panel is active. If false wait animation is active.</param>
    private void Activate(bool newInput)
    {
        if (inputPanel) inputPanel.gameObject.SetActive(newInput);
        if (waitPanel) waitPanel.SetActive(!newInput);
    }
}
