using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// display received video data from other device
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class VideoSize : MonoBehaviour
{
    public bool smallVideo;
    public bool calcAutoImageSize = true;
    public RectTransform boundingBox;

    public AspectRatioFitter[] aspectFitter = null;

    private int sourceImageWidth, sourceImageHeight;

    public event Action<int, int> OnResolutionChanged;

    public bool rescaleImage = true;

    // Width of the source videos.
    public int SourceImageWidth
    {
        get
        {
            return sourceImageWidth;
        }
    }

    // Height of the source videos.
    public int SourceImageHeight
    {
        get
        {
            return sourceImageHeight;
        }
    }


    private void Start()
    {
        sourceImageWidth = 0;
        sourceImageHeight = 0;

        if (!smallVideo)
        {
            AnchorGalleryDetailManager.OnEnabled += GalleryOpened;
            AnchorGalleryDetailManager.OnDisabled += GalleryClosed;
        }

        ResolutionManager.OnLResolutionChanged += calcImageRatio;

        // Calculate the resolution of the video image.
        calcImageRatio(ResolutionManager.Instance.lWidth, ResolutionManager.Instance.lHeight);
    }

    private void OnDestroy()
    {
        if (!smallVideo)
        {
            AnchorGalleryDetailManager.OnEnabled -= GalleryOpened;
            AnchorGalleryDetailManager.OnDisabled -= GalleryClosed;
        }

        ResolutionManager.OnLResolutionChanged -= calcImageRatio;
        ResolutionManager.OnPResolutionChanged -= calcImageRatio;

    }

    void Update()
    {
        if (calcAutoImageSize)
        {
            // Dynamically calculate the resolution of the video image. By changing the orientation of the device, the resolution can change dynamically.
            RawImage image = GetComponent<RawImage>();
            if (image == null || image.texture == null)
            {
                image = GetComponentInChildren<RawImage>();
                if (image == null || image.texture == null)
                    return;
            }
            sourceImageWidth = image.texture.width;
            sourceImageHeight = image.texture.height;
            calcImageRatio();
        }

    }

    /// <summary>
    /// gallery view was opened
    /// </summary>
    private void GalleryOpened()
    {
        ResolutionManager.OnLResolutionChanged -= calcImageRatio;
        ResolutionManager.OnPResolutionChanged += calcImageRatio;
    }

    /// <summary>
    /// gallery view was closed
    /// </summary>
    private void GalleryClosed()
    {
        ResolutionManager.OnPResolutionChanged -= calcImageRatio;
        ResolutionManager.OnLResolutionChanged += calcImageRatio;
    }

    /// <summary>
    /// target device orientation changed
    /// recalculate image ration
    /// </summary>
    public void calcImageRatio()
    {
        calcImageRatio(sourceImageWidth, sourceImageHeight);
    }

    private void setAspectFilter(int width, int height)
    {
        if (aspectFitter != null)
        {
            foreach (var aspect in aspectFitter)
            {
                if (aspect)
                    aspect.aspectRatio = width / (float)height;
            }
        }
    }

    /// <summary>
    /// target device orientation changed
    /// recalculate image ration
    /// </summary>
    /// <param name="width">new width</param>
    /// <param name="height">new height</param>
    public void calcImageRatio(int width, int height)
    {
        setAspectFilter(width, height);

        if (rescaleImage)
        {
            sourceImageWidth = width;
            sourceImageHeight = height;

            RectTransform ltransform = GetComponent<RectTransform>();
            if (ltransform == null)
                return;
            RectTransform ptransform = ltransform.parent.GetComponent<RectTransform>();
            if (boundingBox)
                ptransform = boundingBox;

            if (ptransform == null)
                return;

            Vector2 parentSize = new Vector2(ptransform.rect.width, ptransform.rect.height);

            Vector2 availableDelta = (ltransform.rotation) * parentSize;
            availableDelta = Abs(availableDelta);

            float ratio = width / (float)height;

            Vector2 res = new Vector2();
            if (availableDelta.x / width < availableDelta.y / height)
            {
                res.x = availableDelta.x;
                res.y = availableDelta.x / ratio;
            }
            else
            {
                res.x = availableDelta.y * ratio;
                res.y = availableDelta.y;
            }

            if (!float.IsNaN(res.x) && !float.IsNaN(res.y))
            {
                ltransform.sizeDelta = res;
            }
        }
    }

    

    /// <summary>
    /// target device resolution changed
    /// </summary>
    public void ChangeImageSize(int width, int height)
    {
        setAspectFilter(width, height);

        calcAutoImageSize = false;
        sourceImageWidth = width;
        sourceImageHeight = height;
        
        if (DrawingRemoteManager.Instance == null || !DrawingRemoteManager.Instance.DrawingActive)
        {
        	calcImageRatio();
        }
        OnResolutionChanged?.Invoke(width, height);
        
    }

    /// <summary>
    /// Absolute value for a vector
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    Vector2 Abs(Vector2 v)
    {
        v.x = Mathf.Abs(v.x);
        v.y = Mathf.Abs(v.y);
        return v;
    }
}
