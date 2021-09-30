using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// manages the functionality for the gallery overview on the server. The overview displays a preview image of all gallery entries. 
/// The gallery contains all annotations which are created in this call.
/// </summary>
public class AnchorGalleryOverviewManagerServer : AnchorGalleryOverviewManager
{
    /// <summary>
    /// singleton pattern instance property of derived manger type
    /// </summary>
    public static AnchorGalleryOverviewManagerServer ServerInstance
    {
        get
        {
            return ((AnchorGalleryOverviewManagerServer)AnchorGalleryOverviewManagerServer.Instance);
        }
    }

    /// <summary>
    /// get all gallery entries for preview gallery images
    /// </summary>
    public override void LoadGalleryPreviewItems()
    {
        base.LoadGalleryPreviewItems();
        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.CallForGalleryItems, ""));
    }

    /// <summary>
    /// get all gallery entries for preview gallery images
    /// </summary>
    public virtual void LoadGalleryPreviewItems(List<int> anchorIds)
    {
        base.LoadGalleryPreviewItems();
        foreach (var anchorId in anchorIds)
        {
            var item = Instantiate(itemPrefab, ContentContainer);
            item.AnchorId = anchorId;

            var snapshot = AnchorGalleryServerHelpe.getSnapshotFromDictionary(anchorId);
            if (snapshot != null)
            {
                item.PreviewImage = (snapshot.PreviewSnapshotTexture as Texture2D);
                item.Orientation = snapshot.SnapshotOrientation;
                item.Owner = snapshot.Owner;
            }
            else
            {
                item.Owner = AnnotationOwner.Client;
                EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.CallForAnchorSnapshotData, anchorId.ToString()));
            }
        }
    }

    /// <summary>
    /// get all gallery entries for preview gallery images
    /// </summary>
    public virtual void LoadGalleryPreviewItems(int anchorId, Texture2D previewImage, int snapshotOrientation)
    {
        foreach (Transform child in ContentContainer)
        {
            var item = child.GetComponent<GalleryItem>();
            if (item.AnchorId == anchorId)
            {
                item.PreviewImage = previewImage;
                item.Orientation = snapshotOrientation;
                break;
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
        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.DeleteAnchor, anchorId.ToString() + "/" + CommandMsgType.CallForGalleryItems));
    }
}
