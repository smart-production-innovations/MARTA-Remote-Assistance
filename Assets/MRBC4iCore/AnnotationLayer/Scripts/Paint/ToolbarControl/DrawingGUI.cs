using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Drawing tool bar UI elements have to disable drawing while interacting with them
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class DrawingGUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    #region properties
    //close tool bar ui element when user starts to draw
    public bool setInactivOnDrawing = false;
    public bool searchForConnectedToggleButton = true;

    private static DrawingGUI[] allDrawingGUIs;
    /// <summary>
    /// Find all DrawingGUI Elements
    /// </summary>
    public static DrawingGUI[] AllDrawingGUIs
    {
        get
        {
            if (allDrawingGUIs == null)
                allDrawingGUIs = SearchHelper.FindSceneObjectsOfTypeAll<DrawingGUI>();

            return allDrawingGUIs;
        }
    }

    /// <summary>
    /// Is Mouse inside any DrawingGUI Element. Disable drawing activities if the user interacts with an overlaying GUI Element.
    /// </summary>
    public static bool MouseInsideAnyDrawingGUI
    {
        get
        {
            if (DrawingSettings.HasInstance)
            {
                foreach (var gui in AllDrawingGUIs)
                {
                    if (gui.MouseInsideGUI)
                    {
                        DrawingSettings.Instance.SetActive(false);
                        return true;
                    }
                }

                DrawingSettings.Instance.SetActive(true);
            }
            return false;
        }
    }

    /// <summary>
    /// Is Mouse inside this DrawingGUI Element. Disable drawing activities if the user interacts with an overlaying GUI Element.
    /// </summary>
    public bool MouseInsideGUI
    {
        get
        {
            if (gameObject && gameObject.activeInHierarchy)
                return RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), Input.mousePosition);

            return false;
        }
    }
    #endregion

    #region settings
    /// <summary>
    /// Close overlaying GUI Elements if the users resumes the drawing activity.
    /// </summary>
    public static void SetAllDrawingGUIsInactive()
    {
        foreach (var gui in AllDrawingGUIs)
        {
            if (gui.setInactivOnDrawing)
            {
                if (gui.ToggleButton != null)
                    gui.ToggleButton.isOn = false;
                else
                    gui.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// same drawing ui elements like the color selection pop up are triggered by a toggle button
    /// </summary>
    private Toggle toggleButton = null;
    public Toggle ToggleButton
    {
        get
        {
            if (!toggleButton && searchForConnectedToggleButton)
            {
                var toggles = SearchHelper.FindSceneObjectsOfTypeAll<Toggle>();
                foreach (var item in toggles)
                {
                    
                    var eventCount = item.onValueChanged.GetPersistentEventCount();
                    for (int i = 0; i < eventCount; i++)
                    {
                        var target = item.onValueChanged.GetPersistentTarget(i);
                        GameObject targetGameObject;
                        if (target.GetType() == typeof(GameObject))
                            targetGameObject = (GameObject)target;
                        else
                            targetGameObject = ((MonoBehaviour)target).gameObject;

                        if (targetGameObject == gameObject)
                        {
                            toggleButton = item;
                            return toggleButton;
                        }
                    }
                }
            }
            return toggleButton;
        }
    }
    #endregion

    #region mouse events
    /// <summary>
    /// Disable drawing activities if the user interacts with an overlaying GUI Element.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        DrawingSettings.Instance.SetActive(false);
    }

    /// <summary>
    /// Disable drawing activities if the user interacts with an overlaying GUI Element.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.IsPointerMoving())
            DrawingSettings.Instance.SetActive(false);
    }

    /// <summary>
    /// Enable drawing activities if the user stop interacting with an overlaying GUI Element.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        DrawingSettings.Instance.SetActive(true);
    }

    /// <summary>
    /// Enable drawing activities if the user stop interacting with an overlaying GUI Element
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerUp(PointerEventData eventData)
    {
        DrawingSettings.Instance.SetActive(true);
    }
    #endregion

    #region unity loop
    /// <summary>
    /// Enable drawing activities if the user closes an overlaying GUI Element
    /// </summary>
    void OnDisable()
    {
        DrawingSettings.Instance.SetActive(true);
    }

    void Update()
    {
        if (ToggleButton != null && (!ToggleButton.gameObject.activeInHierarchy || !ToggleButton.isOn))
        {
            ToggleButton.isOn = false;
            gameObject.SetActive(false);
        }
    }
    #endregion
}
