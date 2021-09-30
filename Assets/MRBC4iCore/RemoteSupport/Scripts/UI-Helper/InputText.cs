using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// manage chat box input field. Send Message on Enter.
/// </summary>
public class InputText : MonoBehaviour
{
    public InputField uMessageInputField;
    public Button uMessageSendButton;
    public CustomButton uMessageSendButtonCustom;

    private UnityAction<string> checkEndKeyClicked;

    void Awake()
    {
        checkEndKeyClicked = new UnityAction<string>(InputOnEndEdit);
        uMessageInputField.onEndEdit.AddListener(checkEndKeyClicked);
    }

    private void OnDestroy()
    {
        try
        {
            uMessageInputField.onEndEdit.RemoveListener(checkEndKeyClicked);
        }
        catch (NullReferenceException e)
        {
        }
    }

    /// <summary>
    /// clear input field
    /// </summary>
    public void ResetText()
    {
        if (uMessageInputField) uMessageInputField.text = "";
    }

    /// <summary>
    /// User either pressed enter or left the text field
    /// -> if return key was pressed send the message
    /// </summary>
    public void InputOnEndEdit(string msg)
    {
        if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter) //|| 
            //(Event.current != null && Event.current.keyCode == KeyCode.None)) //true, if leaving textfield with a mouseclick, but ONLY in build (not in editor play mode) (probably not intended ?)
            )
        {
            if (uMessageSendButton) uMessageSendButton.onClick.Invoke();
            if (uMessageSendButtonCustom) uMessageSendButtonCustom.onClick.Invoke();
        }
    }
}
