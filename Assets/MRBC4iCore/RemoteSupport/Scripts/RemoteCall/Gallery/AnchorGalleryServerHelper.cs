using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Caching of the gallery data to bridge the transmission time of the snapshots.
/// </summary>
public class SnapshotDictinaryEntry
{
    public int AnchorId { get; set; }
    public Texture PreviewSnapshotTexture { get; set; }
    public Texture SnapshotTexture { get; set; }
    /// <summary>
    /// on server created snapshots are flipped in y direction
    /// </summary>
    public int SnapshotOrientation { get; set; }
    public AnnotationOwner Owner { get; set; }

    /// <summary>
    /// instantiate a new cache entry
    /// </summary>
    /// <param name="anchorId">anchor point id</param>
    /// <param name="previewTexture">snapshot preview texture</param>
    /// <param name="texture">snapshot texture</param>
    /// <param name="orientation">snapshot orientation</param>
    /// <param name="owner">creator of the anchor point</param>
    public SnapshotDictinaryEntry(int anchorId, Texture previewTexture, Texture texture, int orientation, AnnotationOwner owner)
    {
        AnchorId = anchorId;
        PreviewSnapshotTexture = previewTexture;
        SnapshotTexture = texture;
        SnapshotOrientation = orientation;
        Owner = owner;
    }
}

/// <summary>
/// static helper class for gallery on server side
/// </summary>
public static class AnchorGalleryServerHelpe
{
    /// <summary>
    /// Caching of the gallery data to bridge the transmission time of the snapshots.
    /// </summary>
    private static Dictionary<int, SnapshotDictinaryEntry> anchorSnapshot = new Dictionary<int, SnapshotDictinaryEntry>();

    /// <summary>
    /// get temporary cashed values to bridge the transmission time of the snapshots
    /// </summary>
    /// <param name="anchorID">id of the anchor that should be loaded</param>
    /// <returns>cashed data</returns>
    public static SnapshotDictinaryEntry getSnapshotFromDictionary(int anchorID)
    {
        if (anchorSnapshot.ContainsKey(anchorID))
            return anchorSnapshot[anchorID];
        return null;
    }

    /// <summary>
    /// check if there is a cashed anchor entry with a snapshot on the server
    /// </summary>
    /// <param name="anchorID">anchor id</param>
    /// <returns></returns>
    public static bool NeedSnapshotData(int anchorID)
    {
        if (anchorSnapshot.ContainsKey(anchorID))
            return (anchorSnapshot[anchorID].SnapshotTexture == null);
        return true;
    }

    /// <summary>
    /// add or update a cash entry for a anchor
    /// </summary>
    /// <param name="anchorID">anchor id</param>
    /// <param name="snapshot">snapshot texture</param>
    /// <param name="orientation">Image orientation. Images received from the client over the data channel are flipped differently than those received over the video channel.</param>
    public static void saveSnapshotToDictionary(int anchorID, Texture snapshot, int orientation = 1, AnnotationOwner owner = AnnotationOwner.Client)
    {
        if (anchorSnapshot.ContainsKey(anchorID))
        {
            anchorSnapshot[anchorID].SnapshotTexture = snapshot;
            anchorSnapshot[anchorID].SnapshotOrientation = 1;
        }
        else anchorSnapshot.Add(anchorID, new SnapshotDictinaryEntry(anchorID, snapshot, snapshot, orientation, owner));
    }

    /// <summary>
    /// add or update a cash entry for a anchor
    /// </summary>
    /// <param name="anchorID">anchor id</param>
    /// <param name="snapshot">preview snapshot texture</param>
    /// <param name="orientation">Image orientation. Images received from the client over the data channel are flipped differently than those received over the video channel.</param>
    public static void savePreviewSnapshotToDictionary(int anchorID, Texture snapshot, int orientation = 1, AnnotationOwner owner = AnnotationOwner.Client)
    {
        if (anchorSnapshot.ContainsKey(anchorID))
        {
            anchorSnapshot[anchorID].PreviewSnapshotTexture = snapshot;
            anchorSnapshot[anchorID].SnapshotOrientation = 1;
        }
        else anchorSnapshot.Add(anchorID, new SnapshotDictinaryEntry(anchorID, snapshot, null, orientation, owner));
    }

    /// <summary>
    /// add a new cash entry for a anchor which is new created on server side. In this case the server knows the screen shot immediate in full screen resolution.
    /// </summary>
    /// <param name="anchorID">new anchor id</param>
    /// <param name="snapshot">server side created snapshot which is received from the client over the video channel.</param>
    public static void addNewSnapshotToDictionary(int anchorID, Texture snapshot)
    {
        if (!anchorSnapshot.ContainsKey(anchorID) || anchorSnapshot[anchorID] == null)
            anchorSnapshot.Add(anchorID, new SnapshotDictinaryEntry(anchorID, snapshot, snapshot, -1, AnnotationOwner.Server));
    }

    /// <summary>
    /// display received data
    /// </summary>
    public static void DisplayReceivedData(int anchorId, Vector2 pivot, Vector2 clickPoint, Texture2D annotation = null, Texture snapshot = null, int snapshotOrientation = 1)
    {
        if (AnchorGalleryOverviewManager.Instance.gameObject.activeInHierarchy)
        {
            AnchorGalleryOverviewManagerServer.ServerInstance.LoadGalleryPreviewItems(anchorId, snapshot as Texture2D, snapshotOrientation);
        }
        else
        {
            RemoteHelperImage.AnchorId = anchorId;
            DrawingRemoteManager.Instance.DisplayDrawingArea(pivot, clickPoint, annotation, snapshot, snapshotOrientation);
        }
    }

    /// <summary>
    /// As long as the snapshot for the gallery entry has not been transferred completely over the network to the expert, a default illustration is displayed instead of the snapshot.
    /// </summary>
    /// <param name="anchorId">anchor id for the gallery entry</param>
    /// <param name="clearDrawing">reset annotation</param>
    public static void DisplayLoadDataView(int anchorId, bool clearDrawing = false)
    {
        RemoteHelperImage.AnchorId = anchorId;
        DrawingRemoteManager.Instance.DisplayLoadDataView(clearDrawing);
    }

    /// <summary>
    /// initialize the gallery helper for a new session
    /// </summary>
    public static void initGallerHelper()
    {
        // delete the buffered data from the last session
        anchorSnapshot.Clear();
        // leave the gallery if the last session was closed in the gallery view
        if (CallSettings.HasInstance)
            CallSettings.Instance.LeaveGallery(true);
    }
}
