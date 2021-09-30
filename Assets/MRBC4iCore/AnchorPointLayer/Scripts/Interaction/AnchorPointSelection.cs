using UnityEngine;
using System.Collections;
using System.Linq;

/// <summary>
/// manages the selection of anchor point by long press
/// </summary>
public class AnchorPointSelection : AnchorPointSelection<AnchorPointSelection>
{
}

/// <summary>
/// manages the selection of anchor point by long press
/// </summary>
/// <typeparam name="T">Type of the final class to get right instance typecast</typeparam>
public class AnchorPointSelection<T> : AClickManager<T> where T : Component
{
    private float touchBeginTime;
    private const float selectionTime = 0.5f;

    #region Properties
    /// <summary>
    /// is long or short touch action
    /// </summary>
    public bool LongPress
    {
        get
        {
            return (Time.time - touchBeginTime > selectionTime);
        }
    }
    #endregion

    #region touch events
    /// <summary>
    /// Define the action happens on touch or mouse down
    /// </summary>
    protected override bool InputPositionDownEvents(Vector2 screenPosition)
    {
        base.InputPositionDownEvents(screenPosition);

        touchBeginTime = Time.time;
        return true;
    }

    /// <summary>
    /// Define the action happens on touch or mouse up
    /// </summary>
    public override bool InputPositionUpEvents(Vector2 screenPosition)
    {
        base.InputPositionUpEvents(screenPosition);

        // if long touch check if anchor point should be selected
        if (LongPress)
        {
            RaycastAnchorPoint(Input.mousePosition);
            return true;
        }
        return false;
    }

    /// <summary>
    /// check if anchor point should be selected
    /// </summary>
    /// <param name="position">touch position</param>
    /// <returns>anchor point found to select</returns>
    private bool RaycastAnchorPoint(Vector2 position)
    {
        // Construct a ray from the current touch coordinates
        Ray ray = CameraHelper.MainCamera.ScreenPointToRay(position);

        // check for intersections with 3D colliders
        RaycastHit[] hits = Physics.RaycastAll(ray);
        var isAnchorHit = anchorClicked(hits.Select(x => x.collider.gameObject).ToArray());

        if (!isAnchorHit)
        {
            // check for intersections with 2D colliders
            var hit2ds = Physics2D.GetRayIntersectionAll(ray);
            isAnchorHit = anchorClicked(hit2ds.Select(x => x.collider.gameObject).ToArray());
        }

        return isAnchorHit;
    }

    /// <summary>
    /// check if any hit object is an anchor point
    /// </summary>
    /// <param name="hitObjects">list of touched game objects</param>
    /// <returns>anchor point found to select</returns>
    private bool anchorClicked(GameObject[] hitObjects)
    {
        foreach (var hitObj in hitObjects)
        {
            var anchor = hitObj.GetComponent<AnchorPoint>();
            if (anchor == null)
            {
                anchor = hitObj.GetComponentInParent<AnchorPoint>();
            }
            if (anchor != null)
            {
                AnchorPointManager.Instance.SelectedAnchor = anchor;
                return true;
            }
        }

        return false;
    }
    #endregion
}
