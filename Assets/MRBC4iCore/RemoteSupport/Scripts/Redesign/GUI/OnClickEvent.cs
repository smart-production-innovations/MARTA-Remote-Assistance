using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OnClickEvent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent OnClick;

    private bool isDown = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDown)
        {
            isDown = false;
            OnClick.Invoke();
        }
 
    }
}
