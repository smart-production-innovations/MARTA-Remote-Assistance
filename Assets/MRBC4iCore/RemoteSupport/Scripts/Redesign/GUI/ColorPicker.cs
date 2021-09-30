using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class ColorChangeEvent : UnityEvent<Color> { }

public class ColorPicker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public ColorChangeEvent OnColorChanged;
    public UnityEvent OnColorSelected;

    private RectTransform rectTransform;
    private Texture2D colors = null;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Image img = GetComponent<Image>();
        if (img == null)
        {
            Debug.LogError("ColorPicker without image component");
        }
        colors = img.sprite.texture;
    }

    private void GetColor(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localCursor))
        {
            return;
        }

        int x = (int)((localCursor.x + rectTransform.pivot.x * rectTransform.rect.width) / rectTransform.rect.width * colors.width);
        int y = (int)((localCursor.y + rectTransform.pivot.y * rectTransform.rect.height) / rectTransform.rect.height * colors.height);

        Color color = colors.GetPixel(x, y);
        color.a = 1.0f;

        OnColorChanged.Invoke(color);
    }

    public void OnDrag(PointerEventData eventData)
    {
        GetColor(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        GetColor(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnColorSelected.Invoke();
    }
}
