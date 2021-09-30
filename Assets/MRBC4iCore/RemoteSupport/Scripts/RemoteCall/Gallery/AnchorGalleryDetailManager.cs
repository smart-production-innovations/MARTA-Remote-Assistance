using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// manages the functionality for interacting between gallery items. The gallery contains all annotations which are created in this call.
/// </summary>
public class AnchorGalleryDetailManager : AManager<AnchorGalleryDetailManager>
{
    public static event Action OnEnabled;
    public static event Action OnDisabled;

    protected virtual void OnEnable()
    {
        showAnchorImage();
        GalleryTool.SetAllItemsActive(true);

        OnEnabled?.Invoke();
    }

    protected virtual void OnDisable()
    {
        GalleryTool.SetAllItemsActive(false);

        OnDisabled?.Invoke();
    }

    /// <summary>
    /// load next gallery entry
    /// </summary>
    public virtual void GoToNext()
    {
    }

    /// <summary>
    /// load previous gallery entry
    /// </summary>
    public virtual void GoToPrevious()
    {
    }

    // <summary>
    /// load specific gallery entry
    /// </summary>
    public virtual void GoToId(int id)
    {
    }

    // <summary>
    /// load latest gallery entry
    /// </summary>
    public virtual void GoToLatest()
    {
    }


    /// <summary>
    /// load gallery entry on display index position
    /// </summary>
    /// <param name="anchorId">anchor id of the entry with should be loaded</param>
    public virtual void GetGalleryItem(int anchorId, int displayTime = -1)
    {
        // The loading of the gallery entry is done in the derivations, because the detailed gallery display looks different at the expert and worker on site.

        // With Smartglasses, the gallery is automatically exited after a pre-set period of time.
        if (displayTime > 0)
        {
            StopAllCoroutines();
            StartCoroutine(DisplayEntryForGivenTime(displayTime));
        }
        else if (displayTime == 0)
        {
            GoToOverview();
        }
    }

    /// <summary>
    /// With Smartglasses, the gallery is automatically exited after a pre-set period of time.
    /// </summary>
    /// <param name="displayTime">How long should the gallery be displayed?</param>
    /// <returns></returns>
    public IEnumerator DisplayEntryForGivenTime(float displayTime)
    {
        yield return new WaitForSeconds(displayTime);
        GoToOverview();
    }

    /// <summary>
    /// close detail view and go back to overview
    /// </summary>
    public virtual void GoToOverview()
    {
        if (!CallSettings.HasInstance || CallSettings.Instance.IsGalleryActive)
            AnchorGalleryOverviewManager.Instance.gameObject.SetActive(true);
        else
            LiveviewItem.SetAllItemsActive(true);
        AnchorGalleryDetailManager.Instance.gameObject.SetActive(false);
    }

    /// <summary>
    /// load UI content for active gallery item
    /// </summary>
    public virtual void showAnchorImage()
    {
    }
}
