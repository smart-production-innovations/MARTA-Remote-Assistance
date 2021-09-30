using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ImageAspect : MonoBehaviour
{
    [SerializeField]
    private float aspectRation = 1;
    public float AspectRation
    {
        get { return aspectRation; }
        set
        {
            aspectRation = value;
        }
    }
    private float height = 0;

    private void Update()
    {
        if (height != GetComponent<RectTransform>().rect.height)
        {
            setAspect();
            height = GetComponent<RectTransform>().rect.height;
        }
    }

    private void setAspect()
    {
        var sizeDelta = GetComponent<RectTransform>().sizeDelta;
        sizeDelta.x = GetComponent<RectTransform>().rect.height * aspectRation;
        GetComponent<RectTransform>().sizeDelta = sizeDelta;
    }
}
