using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// One of several implementations for using an annotation as anchor point content.
/// Basic implementation for anchoring the drawing to a feature point. The annotation is scaled according to the distance between camera and feature point.
/// Deprecated projection type. Use the class AnchorAnnotationProjection3D instead.
/// </summary>
[RequireComponent(typeof(Snapshot))]
public class AnchorAnnotationDistanceScale : AnchorAnnotationCanvasBase
{
    #region properties
    protected Vector2 screenClickPosition;
    protected Vector2 screenClickCenterPivotPosition;
    protected Vector2 relativeScreenClickCenterPivotPosition;
    protected float drawingAreaScale = 1;
    protected float distanceToSnapshot = 1;

    /// <summary>
    /// save screen clicking position for later editing
    /// </summary>
    public Vector2 ScreenClickPosition
    {
        get
        {
            return screenClickPosition;
        }

        set
        {
            screenClickPosition = value;
            ScreenClickCenterPivotPosition = screenClickPosition - new Vector2(Screen.width / 2, Screen.height / 2);
        }
    }

    /// <summary>
    /// save screen clicking pivot position for later editing and calculation of drawing overlay position
    /// </summary>
    public virtual Vector2 ScreenClickCenterPivotPosition
    {
        get
        {
            return screenClickCenterPivotPosition;
        }

        set
        {
            screenClickCenterPivotPosition = value;
            RelativeScreenClickCenterPivotPosition = ScreenClickCenterPivotPosition / new Vector2(Screen.width / 2, Screen.height / 2);
        }
    }

    /// <summary>
    /// save screen clicking pivot position for later editing and calculation of drawing overlay position
    /// </summary>
    public virtual Vector2 RelativeScreenClickCenterPivotPosition
    {
        get
        {
            return relativeScreenClickCenterPivotPosition;
        }

        set
        {
            relativeScreenClickCenterPivotPosition = value;
        }
    }

    /// <summary>
    /// scale factor of the drawing area
    /// </summary>
    public float DrawingAreaScale
    {
        get
        {
            return drawingAreaScale;
        }
        set
        {
            drawingAreaScale = value;
            transform.localScale = DistanceScaleFactor;
        }
    }

    /// <summary>
    /// distance from the AR camera to the position were the drawing is anchored
    /// </summary>
    public float DistanceToSnapshot
    {
        get
        {
            return distanceToSnapshot;
        }
        set
        {
            distanceToSnapshot = value;
            transform.localScale = DistanceScaleFactor;
        }
    }

    public Vector3 distaceScaleDefaultScaleValue = Vector3.zero;
    /// <summary>
    /// Default scale factor
    /// </summary>
    public Vector3 DefaultScaleValue
    {
        get
        {
            if (distaceScaleDefaultScaleValue == Vector3.zero)
                distaceScaleDefaultScaleValue = transform.localScale;
            return distaceScaleDefaultScaleValue;
        }
    }
    
    /// <summary>
    /// scale the drawing canvas depending on the distance to the AR camera when the annotation is created
    /// </summary>
    public Vector3 DistanceScaleFactor
    {
        get
        {
            var scaleDistanceFactor = CameraHelper.getDistanceScaleFactor(DistanceToSnapshot, DefaultScaleValue);
            return scaleDistanceFactor;
        }
    }

    #endregion
}
