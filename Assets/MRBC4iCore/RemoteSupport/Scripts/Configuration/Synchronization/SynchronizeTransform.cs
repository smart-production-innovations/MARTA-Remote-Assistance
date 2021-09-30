using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// synchronize size and position of the video panel with the size and position of the drawing panel
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SynchronizeTransform : MonoBehaviour
{
    //game object which defines the boundaries
    public RectTransform drawingBounds;
    public RectTransform[] syncCildren;

    //RectTransform component of this game object
    private RectTransform thisRect;
    private RectTransform drawingBoundsHeight;

    void Awake()
    {
        thisRect = GetComponent<RectTransform>();
        if (drawingBounds && drawingBounds.parent)
            drawingBoundsHeight = drawingBounds.parent.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (drawingBounds)
        {
            if (drawingBounds.gameObject.activeSelf)
            {
                if (drawingBounds.hasChanged)
                {
                    matchSize();
                    drawingBounds.hasChanged = false;
                }
            }
            else if (drawingBoundsHeight && drawingBoundsHeight.hasChanged)
            {
                matchHeight();
                drawingBoundsHeight.hasChanged = false;
            }
        }
    }

    /// <summary>
    /// Synchronize position and size with boundary game object
    /// </summary>
    public void matchSize()
    {
        if (!drawingBounds) return;
        matchPosition(drawingBounds);
        synchronizeChildren(thisRect.rect.size, drawingBounds.rect.size);
        thisRect.sizeDelta = drawingBounds.rect.size;
    }

    /// <summary>
    /// Synchronize position and size with boundary game object
    /// </summary>
    public void matchHeight()
    {
        if (!drawingBoundsHeight) return;
        matchPosition(drawingBoundsHeight);
        calcImageRatio(drawingBoundsHeight.rect.height);
    }

    /// <summary>
    /// set position to the first corner of the given rectangle
    /// </summary>
    /// <param name="source">target coordinates for position</param>
    private void matchPosition(RectTransform source)
    {
        if (!source) return;
        if (!thisRect) thisRect = GetComponent<RectTransform>();

        Vector3[] corners = new Vector3[4];
        source.GetWorldCorners(corners);

        thisRect.position = corners[0];
    }

    /// <summary>
    /// adjust the position of the child objects depending to the change of the aspect ratio
    /// </summary>
    /// <param name="oldSize">previous size</param>
    /// <param name="newSize">target size</param>
    private void synchronizeChildren(Vector2 oldSize, Vector2 newSize)
    {
        float ratio = newSize.x / (float)oldSize.x;
        foreach (var item in syncCildren)
        {
            item.localPosition *= ratio;
        }
    }

    /// <summary>
    /// calculates the aspect ratio depending on the space available and the orientation of the client
    /// </summary>
    /// <param name="newHeight">max available height</param>
    private void calcImageRatio(float newHeight)
    {
        newHeight = Mathf.Abs(newHeight);

        float ratio = thisRect.rect.width / (float)thisRect.rect.height;
        Vector2 res = new Vector2();
        res.x = newHeight * ratio;
        res.y = newHeight;

        synchronizeChildren(thisRect.rect.size, res);
        thisRect.sizeDelta = res;

        thisRect.pivot = new Vector2(0.0f, 0.0f);
    }
}
