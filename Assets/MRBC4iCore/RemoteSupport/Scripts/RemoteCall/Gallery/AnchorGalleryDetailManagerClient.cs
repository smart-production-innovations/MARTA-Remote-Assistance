using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// manages the functionality for interacting with the gallery UI on the client. The gallery contains all annotations which are created in this call.
/// </summary>
public class AnchorGalleryDetailManagerClient : AnchorGalleryDetailManager
{
    public RawImage screenshot;
    public RawImage annotation;
    private int displayIndex;

    protected override void OnEnable()
    {
        ResolutionManager.OnLResolutionChanged += AdjustGalleryResolution;
        showAnchorImage();
    }

    protected override void OnDisable()
    {
        ResolutionManager.OnLResolutionChanged -= AdjustGalleryResolution;

        base.OnDisable();
    }

    /// <summary>
    /// load next gallery entry
    /// </summary>
    public override void GoToNext()
    {
        base.GoToNext();

        displayIndex++;
        if (displayIndex >= AnchorPointManager.Instance.GeAnchorCount()) displayIndex = 0;
        showAnchorImage();
    }

    /// <summary>
    /// load previous gallery entry
    /// </summary>
    public override void GoToPrevious()
    {
        base.GoToPrevious();

        displayIndex--;
        if (displayIndex < 0) displayIndex = AnchorPointManager.Instance.GeAnchorCount() - 1;
        showAnchorImage();
    }

    /// <summary>
    /// load specific gallery entry
    /// </summary>
    public override void GoToId(int id)
    {
        base.GoToId(id);

        displayIndex = id;
        if (displayIndex < 0)
        {
            displayIndex = AnchorPointManager.Instance.GeAnchorCount() - 1;
        }
        else if (displayIndex >= AnchorPointManager.Instance.GeAnchorCount())
        {
            displayIndex = 0;
        }
        showAnchorImage();
    }

    /// <summary>
    /// load gallery entry on display index position
    /// </summary>
    /// <param name="anchorId">anchor id of the entry with should be loaded</param>
    public override void GetGalleryItem(int anchorId, int displayTime = -1)
    {
        base.GetGalleryItem(anchorId, displayTime);

        displayIndex = AnchorPointManager.Instance.GetAnchorPointIndex(anchorId);
        showAnchorImage();
    }

    /// <summary>
    /// Adjust the resolution of the gallery entry.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    private void AdjustGalleryResolution(int width, int height)
    {
        StartCoroutine(UpdateAnchorImage());
    }

    /// <summary>
    /// Adjust the resolution of the gallery entry.
    /// </summary>
    /// <returns></returns>
    IEnumerator UpdateAnchorImage()
    {
        yield return null;
        showAnchorImage();
    }

    /// <summary>
    /// load UI content for active gallery item
    /// </summary>
    public override void showAnchorImage()
    {
        base.showAnchorImage();

        if (screenshot || annotation)
        {
            if (AnchorPointManager.Instance.GeAnchorCount() > 0)
            {
                var anchor = AnchorPointManager.Instance.GetAnchorPointOfIndex(displayIndex).GetComponentInChildren<AnchorImage>(true);
                if (anchor)
                {
                    var iAnchor = anchor as IAnchorAnnotationBase;

                    // display snapshot
                    if (screenshot)
                    {
                        screenshot.texture = iAnchor.GetSnapshot().SnapshotTexture;

                        // Scale the snapshot according to the current device orientation.
                        RectTransform transform = screenshot.rectTransform;
                        RectTransform parent = transform.parent.GetComponent<RectTransform>();
                        
                        screenshot.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                        screenshot.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

                        float parentAspect = parent.rect.width / parent.rect.height;
                        float screenshotAspect = screenshot.texture.width / (float)screenshot.texture.height;

                        if (parentAspect > screenshotAspect)
                        {
                            float sizeX = parent.rect.height * screenshotAspect;
                            float sizeY = parent.rect.height;
                            screenshot.rectTransform.sizeDelta = new Vector2(sizeX, sizeY);
                        }
                        else
                        {
                            float sizeX = parent.rect.width;
                            float sizeY = parent.rect.width / screenshotAspect;
                            screenshot.rectTransform.sizeDelta = new Vector2(sizeX, sizeY);
                        }
                    }

                    // display annotation
                    if (annotation)
                    {
                        annotation.texture = anchor.GetTexture();
                        if (StatusProperties.Values.CalculationMode == CalculationMode.ClickPointPlane)
                        {
                            // Deprecated:
                            // With click point anchoring, the annotation drawing is placed centred in the click point.

                            annotation.rectTransform.pivot = anchor.GetPivot();
                            annotation.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                            annotation.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                            float sizeX = 0.5f * annotation.texture.width * screenshot.rectTransform.rect.width / screenshot.texture.width;
                            float sizeY = 0.5f * annotation.texture.height * screenshot.rectTransform.rect.height / screenshot.texture.height;

                            annotation.rectTransform.sizeDelta = new Vector2(StatusProperties.Values.DrawingResizeFactor * sizeX, StatusProperties.Values.DrawingResizeFactor * sizeY);

                            var localPosition = Vector2.zero;
                            if (anchor is AnchorAnnotationDistanceScale)
                                localPosition = ((AnchorAnnotationDistanceScale)anchor).RelativeScreenClickCenterPivotPosition;
                            localPosition *= new Vector2(0.5f * screenshot.rectTransform.rect.width, 0.5f * screenshot.rectTransform.rect.height);
                            annotation.rectTransform.localPosition = localPosition;
                        }
                        else 
                        {
                            // Recommended:
                            // In projection mode, the entire screen area is used for annotating the screenshot. 
                            // The transformation of the 2D annotation into 3D space is done later by projection.
                            annotation.rectTransform.sizeDelta = screenshot.rectTransform.sizeDelta;
                            annotation.rectTransform.localPosition = Vector3.zero;
                        }
                    }
                }
            }
        }
    }

}
