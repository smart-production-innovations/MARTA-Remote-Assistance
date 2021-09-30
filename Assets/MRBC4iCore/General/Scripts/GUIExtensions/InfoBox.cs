using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays an information text
/// </summary>
public class InfoBox : MonoBehaviour
{
    public Text Text;
    public Image Image;

    /// <summary>
    /// Sets the information text
    /// </summary>
    /// <param name="text">information text</param>
    /// <param name="color">background color</param>
    public void SetInfo(string text, Color color)
    {
        Text.text = text;
        Image.color = color;
    }
}

