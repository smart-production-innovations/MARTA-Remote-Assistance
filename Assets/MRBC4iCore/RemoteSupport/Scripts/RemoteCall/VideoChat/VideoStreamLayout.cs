using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// manage layout change for display video (resize window or device orientation changed)
/// </summary>
public class VideoStreamLayout : MonoBehaviour
{
    /// <summary>
    /// connected video size placeholder to the layout element
    /// </summary>
    private VideoSize[] videos;
    private VideoSize[] Videos
    {
        get
        {
            if (videos == null)
                videos = GetComponentsInChildren<VideoSize>(true);
            return videos;
        }
    }

    /// <summary>
    /// target device orientation changed
    /// </summary>
    void OnRectTransformDimensionsChange()
    {
        foreach (var video in Videos)
        {
            video.calcImageRatio();
        }
    }
}
