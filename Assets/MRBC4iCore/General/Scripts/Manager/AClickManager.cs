using UnityEngine;
using System.Collections;

/// <summary>
/// Single instance basic class for managing core features that listen to mouse click or touch events. Implements the singleton pattern for calling the manager from any core feater script.
/// </summary>
/// <typeparam name="T">Type of the final class to get right instance typecast</typeparam>
public abstract class AClickManager<T> : AManager<T> where T : Component
{
    protected virtual void Update()
    {
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            UpdateMouseEvents();
        }
        else
        {
            UpdateTouchEvents();
        }

    }

    /// <summary>
    /// Short Touch leads to an new anchor point with an empty annotation
    /// </summary>
    protected virtual void UpdateMouseEvents()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            InputPositionDownEvents(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            InputPositionUpEvents(Input.mousePosition);
        }
    }

    /// <summary>
    /// Short Touch leads to an new anchor point with an empty annotation
    /// </summary>
    protected virtual void UpdateTouchEvents()
    {
        for (int i = 0; i < Input.touchCount; ++i)
        {
            var touch = Input.GetTouch(i);

            if (touch.phase.Equals(TouchPhase.Began))
            {
                if (InputPositionDownEvents(touch.position))
                    break;
            }
            else if (touch.phase.Equals(TouchPhase.Ended))
            {
                if (InputPositionUpEvents(touch.position))
                    break;
            }
        }
    }

    /// <summary>
    /// Define the action happens on touch or mouse down
    /// </summary>
    protected virtual bool InputPositionDownEvents(Vector2 screenPosition)
    {
        return false;
    }

    /// <summary>
    /// Define the action happens on touch or mouse up
    /// </summary>
    public virtual bool InputPositionUpEvents(Vector2 screenPosition)
    {
        return false;
    }
}
