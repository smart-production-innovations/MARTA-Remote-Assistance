using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// manages the functionality for the gallery overview. The overview displays a preview image of all gallery entries. 
/// The gallery contains all annotations which are created in this call.
/// </summary>
public class AnchorGalleryOverviewManager : AManager<AnchorGalleryOverviewManager>
{
    public GalleryItem itemPrefab;
    public Transform contentContainer;

    /// <summary>
    /// Parent object which displays the gallery thumbnails
    /// </summary>
    protected Transform ContentContainer
    {
        get
        {
            if (contentContainer == null)
                contentContainer = transform;
            return contentContainer;
        }
    }

    private void OnEnable()
    {
        LoadGalleryPreviewItems();
    }

    /// <summary>
    /// get all gallery entries for preview gallery images
    /// </summary>
    public virtual void LoadGalleryPreviewItems()
    {
        // The loading of the gallery entry is done in the derivations, because the gallery view looks different at the expert and worker on site.

        // delete the previous overview entries 
        ContentContainer.DeleteAllChildren();
    }

    /// <summary>
    /// delete the annotation with the given anchorId from the anchor point layer
    /// </summary>
    /// <param name="anchorId">id of the anchor which should be deleted</param>
    public virtual void DeleteItem(int anchorId)
    {
        // this action is done in the derivations
    }
}
