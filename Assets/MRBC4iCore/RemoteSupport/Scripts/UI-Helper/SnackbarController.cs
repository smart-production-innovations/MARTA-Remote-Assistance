using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// inform the user about status changes
/// </summary>
public class SnackbarController : MonoBehaviour
{
    public bool fadeOutAutomatically = true;
    public bool hideAutomatically = false;
    private float displayTime = 5f;
    private float fadeTme = 2f;
    private float fadeFPS = 25f;
    private float lerpRate = 1.0f;
    private RectTransform rect;
    private Vector2 startPos;
    private Vector2 endPos;
    private bool startPosCalculated = false;
    private Text uiTextElement;

    /// <summary>
    /// connected UI element
    /// </summary>
    public Text UITextElement
    {
        get
        {
            if (!uiTextElement)
                uiTextElement = GetComponentInChildren<Text>();
            return uiTextElement;
        }
    }

    /// <summary>
    /// snack bar info text
    /// </summary>
    public string Text
    {
        get
        {
            if (UITextElement)
                return UITextElement.text;
            return "";
        }
        set
        {
            if (UITextElement)
                UITextElement.text = value;
        }
    }

    /// <summary>
    /// default display position of the snack bar
    /// </summary>
    private Vector2 StartPos
    {
        get
        {
            if (!startPosCalculated)
                setDefaultPos();
            return startPos;
        }
    }

    void Awake()
    {
    }

    private void OnEnable()
    {
        if (fadeOutAutomatically)
            StartCoroutine("DisplaySnackbar");

        if (hideAutomatically)
            StartCoroutine("HideAutomaticallySnackbar");
    }

    /// <summary>
    /// calculate the default display position of the snack bar
    /// </summary>
    private void setDefaultPos()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
        endPos = new Vector2(startPos.x, -80);
        startPosCalculated = true;
    }

    /// <summary>
    /// Display the status info for displayTime seconds
    /// </summary>
    /// <returns></returns>
    IEnumerator DisplaySnackbar()
    {
        var startPos = StartPos;
        rect.anchoredPosition = startPos;
        var lerpProg = 0f;

        //moving the snack bar out of the screen after displayTime sec
        yield return new WaitForSecondsRealtime(displayTime);

        var deltaTime = 1f / (fadeFPS * fadeTme);

        while (lerpProg < 1)
        {
            lerpProg += lerpRate * deltaTime;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, lerpProg);
            yield return new WaitForSecondsRealtime(deltaTime);
        }

        gameObject.SetActive(false);
    }

    IEnumerator HideAutomaticallySnackbar()
    {
        yield return new WaitForSecondsRealtime(displayTime);
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// used by the OK button on the snack bar
    /// </summary>
    public void DismissSnackbar()
    {
        gameObject.SetActive(false);
    }
}
