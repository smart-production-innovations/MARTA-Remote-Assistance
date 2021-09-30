using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// display unique connection key while connecting to another device
/// </summary>
public class SetKey : MonoBehaviour
{
    private Text keyText;
    private Text KeyText
    {
        get
        {
            if (!keyText)
                keyText = GetComponent<Text>();
            return keyText;
        }
    }

    void Awake()
    {
        ActionEventManager.Subscribe<string>(EventName.UniqueKeyCalculated, UniqueKeyCalculated);
    }

    void OnDestroy()
    {
        ActionEventManager.Unsubscribe<string>(EventName.UniqueKeyCalculated, UniqueKeyCalculated);
    }

    /// <summary>
    /// display unique key on waiting screen and copy key to clipboard
    /// </summary>
    /// <param name="key">unique key</param>
    private void UniqueKeyCalculated(string key)
    {
        if (KeyText) KeyText.text = key;
        CopyToClipboard();
    }

    /// <summary>
    /// display unique key on waiting screen
    /// </summary>
    /// <param name="key">unique key</param>
    public void SetKeyValue(string key)
    {
        UniqueKeyCalculated(key);
    }

    /// <summary>
    /// copy unique key to clipboard
    /// </summary>
    public void CopyToClipboard()
    {
        if (!keyText) return;

        TextEditor te = new TextEditor();
        te.text = keyText.text;
        te.SelectAll();
        te.Copy();
    }
}
