using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// manage pop up panels which are connected to toggle buttons
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class PopUpManager : MonoBehaviour
{
    public Toggle toggle;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        bool mouse_held_down = Input.GetMouseButton(0);
        if (mouse_held_down)
        {
            //hide the pop up dialog if user interacts with elements outside the pop up
            Vector2 mousePos = Input.mousePosition;
            bool inside = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePos);
            if (!inside)
            {
                //gameObject.SetActive(inside);
                //if (toggle) toggle.isOn = inside;
                StopAllCoroutines();
                StartCoroutine(DisablePopUp());
            }

        }
    }

    IEnumerator DisablePopUp()
    {
        // wait until UI interactions are handled
        yield return new WaitForSeconds(0.1f);

        gameObject.SetActive(false);
        if (toggle) toggle.isOn = false;
    }
}
