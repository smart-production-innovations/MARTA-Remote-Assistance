using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// manages the functionality for the gallery overview on the client. The overview displays a preview image of all gallery entries. 
/// The gallery contains all annotations which are created in this call.
/// </summary>
public class AnchorGalleryOverviewManagerClient : AnchorGalleryOverviewManager
{
    /// <summary>
    /// get all gallery entries for preview gallery images
    /// </summary>
    public override void LoadGalleryPreviewItems()
    {
        base.LoadGalleryPreviewItems();
        var anchors = AnchorPointManager.Instance.GetAllAnchorPoints();
        int count = 0;
        foreach (var anchor in anchors)
        {
            count++;
            var item = Instantiate(itemPrefab, ContentContainer);
            item.AnchorId = anchor.Id;
            var annotation = AnnotationManager.Instance.getAnchor(anchor.Id);
            if (annotation)
            {
                var iAnnoation = annotation as IAnchorAnnotationBase;
                if (iAnnoation != null)
                {
                    item.PreviewImage = iAnnoation.GetSnapshot().SnapshotTexture;
                    item.Owner = iAnnoation.AnnotationOwner;
                }
            }
        }
    }

    /// <summary>
    /// delete the annotation with the given anchorId from the anchor point layer
    /// </summary>
    /// <param name="anchorId">id of the anchor which should be deleted</param>
    public override void DeleteItem(int anchorId)
    {
        base.DeleteItem(anchorId);
        AnnotationManager.Instance.RemovedAnchor(anchorId);
        LoadGalleryPreviewItems();
    }
}
