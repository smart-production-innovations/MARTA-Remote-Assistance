using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Events <see cref="OnLResolutionChanged"/> and <see cref="OnPResolutionChanged"/> invoked,
/// when resolution of live video or annotation size changes
/// e.g. because of device orientation change
/// </summary>
public class ResolutionManager : AManager<ResolutionManager>
{
    public static event Action<int, int> OnLResolutionChanged; //live
    public static event Action<int, int> OnPResolutionChanged; //picture (annotation)

    public int lWidth; //live
    public int lHeight;

    public int pWidth; //picture (annotation)
    public int pHeight;

    /// <summary>
    /// change the live video resolution
    /// </summary>
    /// <param name="width">new width</param>
    /// <param name="height">new height</param>
    public void ChangeLResolution(int width, int height)
    {
        lWidth = width;
        lHeight = height;

        OnLResolutionChanged?.Invoke(width, height);
    }

    /// <summary>
    /// change the picture (annotation) resolution
    /// </summary>
    public void ChangePResolution()
    {
        ChangePResolution(lWidth, lHeight);
    }

    /// <summary>
    /// change the picture (annotation) resolution
    /// </summary>
    /// <param name="width">new width</param>
    /// <param name="height">new height</param>
    public void ChangePResolution(int width, int height)
    {
        pWidth = width;
        pHeight = height;

        OnPResolutionChanged?.Invoke(width, height);
    }
}
