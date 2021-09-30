using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// manages the functionality for interacting with the gallery UI on the server. The gallery contains all annotations which are created in this call.
/// </summary>
public class AnchorGalleryDetailManagerServer : AnchorGalleryDetailManager
{
    public Texture2D defaultTexture;

    protected override void OnEnable()
    {
        base.OnEnable();

        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.GalleryOpen, ""));
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.GalleryClose, ""));
    }

    /// <summary>
    /// load next gallery entry
    /// </summary>
    public override void GoToNext()
    {
        base.GoToNext();
        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.CallForNextAnchorData, RemoteHelperImage.AnchorId.ToString()));
        AnchorGalleryServerHelpe.DisplayLoadDataView(-1, clearDrawing: true);
    }

    /// <summary>
    /// load previous gallery entry
    /// </summary>
    public override void GoToPrevious()
    {
        base.GoToPrevious();
        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.CallForPreviousAnchorData, RemoteHelperImage.AnchorId.ToString()));
    }

    /// <summary>
    /// load latest gallery entry
    /// </summary>
    public override void GoToLatest()
    {
        base.GoToLatest();
        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.CallForPreviousAnchorData, 0.ToString()));
    }

    /// <summary>
    /// load gallery entry on display index position
    /// </summary>
    /// <param name="anchorId">anchor id of the entry with should be loaded</param>
    public override void GetGalleryItem(int anchorId, int displayTime = -1)
    {
        base.GetGalleryItem(anchorId, displayTime);
        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.CallForAnchorData, anchorId.ToString()));
    }

    /// <summary>
    /// close detail view and go back to overview
    /// </summary>
    public override void GoToOverview()
    {
        DrawingRemoteManager.Instance.CancelDrawing();
        base.GoToOverview();
    }

    /// <summary>
    /// load UI content for active gallery item. On server side this functionality is managed when receive the display data from client (ReceiveData). 
    /// </summary>
    public override void showAnchorImage()
    {
        base.showAnchorImage();
        DrawingRemoteManager.Instance.SetSnapshotTexture(defaultTexture);
    }
}
