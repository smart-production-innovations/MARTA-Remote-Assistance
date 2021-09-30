using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// manages all drawing activities for the annotation layer
/// </summary>
public class DrawingRemoteManager : DrawingManager<DrawingRemoteManager>
{
    public bool isFullScreen = true;
    //game object which defines the boundaries in which drawing is allowed
    public RectTransform drawingBounds;
    //boundaries of the final drawing area
    public RectTransform drawingArea;

    public GameObject smaleVideo;
    public RectTransform drawingOverlayRemoteVideoImage;

    private Vector2 drawingBoundsActualSize;

    #region unity loop

    protected override void Update()
    {
        base.Update();

        if (DrawingActive || BandwidthManager.Instance.SupportModeType == SupportModeType.LowBandwidthMode)
        {
            if (drawingBounds)
            {
                if (drawingBounds.rect.size != drawingBoundsActualSize)
                {
                    drawingBoundsActualSize = drawingBounds.rect.size;
                    matchSize();
                }
            }
        }

        if (BandwidthManager.Instance.SupportModeType != SupportModeType.LowBandwidthMode && drawingOverlayRemoteVideoImage && drawingOverlayRemoteVideoImage.gameObject.activeSelf)
        {
            drawingOverlayRemoteVideoImage.gameObject.SetActive(false);
        }
    }
    #endregion

    #region touch
    /// <summary>
    /// Define the action happens on touch or mouse down
    /// </summary>
    protected override bool InputPositionDownEvents(Vector2 screenPosition)
    {
        var valid = base.InputPositionDownEvents(screenPosition);

        //ensure that in one unity loop an annotation is not saved and a new one is created at the same time.
        if (ReadyForNextDrawing && valid)
        {
            if (drawingBounds)
            {
                Vector2 mousePosInImage;

                if (!RectTransformUtility.RectangleContainsScreenPoint(drawingBounds, screenPosition))
                    return false;

                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(drawingBounds, screenPosition, null, out mousePosInImage))
                    return false;

                OverlayAnchorPosition = mousePosInImage - DrawingBoundsSize / 2;
            }
            else
            {
                OverlayAnchorPosition = screenPosition - DrawingBoundsSize / 2;
            }
        }
        return valid;
    }

    /// <summary>
    /// synchronize size and position of the video panel with the size and position of the drawing panel
    /// </summary>
    public override void matchSize()
    {
        if (isFullScreen || !drawingBounds) return;

        var saveOverlayPos = OverlayAnchorPosition;

        Vector3[] corners = new Vector3[4];
        drawingBounds.GetWorldCorners(corners);

        drawingArea.position = corners[0];
        drawingArea.sizeDelta = DrawingBoundsSize;

        OverlayAnchorPosition = saveOverlayPos;

        base.matchSize();

        drawingOverlayRemoteVideoImage.localScale = drawingOverlay.localScale;
        drawingOverlayRemoteVideoImage.sizeDelta = drawingOverlay.sizeDelta;
        drawingOverlayRemoteVideoImage.anchoredPosition = drawingOverlay.anchoredPosition;
    }


    /// <summary>
    /// activates a empty drawing canvas
    /// </summary>
    public override void DisplayEmptyDrawingArea()
    {
        base.DisplayEmptyDrawingArea();
        DisplayEmptyDrawingAreaExtension();
    }

    /// <summary>
    /// activates a empty drawing canvas
    /// </summary>
    public override void DisplayEmptyDrawingArea(Vector2 clickPoint)
    {
        base.DisplayEmptyDrawingArea(clickPoint);
        DisplayEmptyDrawingAreaExtension();
    }

    /// <summary>
    /// additional code for activating a empty drawing canvas
    /// </summary>
    private void DisplayEmptyDrawingAreaExtension()
    {
        if (drawingBounds && drawingBounds.GetComponent<RawImage>() && (BandwidthManager.Instance.SupportModeType != SupportModeType.LowBandwidthMode))
            SetSnapshotTexture(makeSnapshot(drawingBounds.GetComponent<RawImage>().texture));
    }

    /// <summary>
    /// activates a drawing canvas for editing a existing annotation
    /// </summary>
    public override void DisplayDrawingArea(Vector2 pivot, Vector2 clickPoint, Texture2D annotation = null, Texture snapshot = null, int snapshotOrientation = 1)
    {
        base.DisplayDrawingArea(pivot, clickPoint, annotation, snapshot, snapshotOrientation);

        if (snapshot == null && drawingBounds && drawingBounds.GetComponent<RawImage>())
            SetSnapshotTexture(makeSnapshot(drawingBounds.GetComponent<RawImage>().texture));
    }

    /// <summary>
    /// As long as the snapshot for the gallery entry has not been transferred completely over the network to the expert, a default illustration is displayed instead of the snapshot.
    /// </summary>
    /// <param name="clearDrawing">reset annotation</param>
    public void DisplayLoadDataView(bool clearDrawing = false)
    {
        if (clearDrawing)
            DrawFreeHand.Instance.ResetCanvas();
        var snapshot = ((AnchorGalleryDetailManagerServer)AnchorGalleryDetailManagerServer.Instance).defaultTexture;
        SetSnapshotTextureForDataSend(snapshot);
        DrawingActive = true;
    }

    /// <summary>
    /// makes a snapshot of the active video frame
    /// </summary>
    /// <param name="sourceTex">active video frame texture</param>
    /// <returns>copy of the video frame texture</returns>
    public static Texture2D makeSnapshot(Texture sourceTex)
    {
        var destTex = new Texture2D(sourceTex.width, sourceTex.height, TextureFormat.ARGB32, false);
        if (sourceTex is Texture2D)
        {
            var sourceTexture = (Texture2D)sourceTex;

            // Get the pixel block and reverse the array to rotate the image.
            var pix = sourceTexture.GetPixels32();

            // Copy the reversed image data to a new texture.
            destTex.SetPixels32(pix);
            destTex.Apply();
        }
        else
        {
            Graphics.CopyTexture(sourceTex, destTex);
            destTex.Apply();
        }

        return destTex;
    }

    #endregion

    #region properties
    /// <summary>
    /// state of the drawing manger. DrawingActive = true -> user is currently in drawing mode of an annotation. DrawingActive = false -> manager is waiting for are new annotation click.
    /// </summary>
    public override bool DrawingActive
    {
        set
        {
            base.DrawingActive = value;
            if (drawingBounds) drawingBounds.gameObject.SetActive(!value);
            if (smaleVideo) smaleVideo.gameObject.SetActive(value && (BandwidthManager.Instance.SupportModeType == SupportModeType.Videostream));
            if (value && drawingOverlayRemoteVideoImage) drawingOverlayRemoteVideoImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// drawing area (artboard) size
    /// </summary>
    public override Vector2 DrawingAreaSize
    {
        get
        {
            if (drawingArea)
                return drawingArea.rect.size;

            return base.DrawingAreaSize;
        }
    }

    /// <summary>
    /// Size of the border of the artboard (drawing area) by the underlying screenshot
    /// </summary>
    public override Vector2 DrawingBoundsSize
    {
        get
        {
            if (drawingBounds)
                return drawingBounds.rect.size;

            return base.DrawingBoundsSize;
        }
    }

    /// <summary>
    /// Get the texture of the screen snapshot of the actual drawing activity
    /// </summary>
    public override Texture SnapshotTexture
    {
        get
        {
            var txt = base.SnapshotTexture;
            if (txt)
                return txt;
            else if (drawingBounds && drawingBounds.GetComponent<RawImage>())
                return drawingBounds.GetComponent<RawImage>().texture;

            return null;
        }
    }
    #endregion

    #region result
    /// <summary>
    /// cancel the drawing activities for the active annotation drawing. The last changes are discarded and the anchor point delete if it is empty.
    /// </summary>
    public override void CancelDrawing(bool goToGalleryOverviewIfGalleryIsOpen = false)
    {
        base.CancelDrawing(goToGalleryOverviewIfGalleryIsOpen);

        if (drawingBounds && drawingBounds.GetComponent<RemoteHelperImage>())
            drawingBounds.GetComponent<RemoteHelperImage>().cancelImageActionOnClient();
    }


    /// <summary>
    /// save the screen drawing annotation
    /// </summary>
    /// <param name="image">static ui image</param>
    public override void SaveImage()
    {
        base.SaveImage();

        if (drawingOverlayRemoteVideoImage)
        {
            drawingOverlayRemoteVideoImage.gameObject.SetActive((BandwidthManager.Instance.SupportModeType == SupportModeType.LowBandwidthMode));
            var drawingOverlayRemoteVideoImageRaw = drawingOverlayRemoteVideoImage.GetComponent<RawImage>();
            if (drawingOverlayRemoteVideoImageRaw)
                drawingOverlayRemoteVideoImageRaw.texture = TemporaryRenderTexture;
        }
    }


    /// <summary>
    /// cache the screen drawing annotation while the drawing activity remains active
    /// </summary>
    public override void ContinuousSaveImage(bool permanentSave = false)
    {
        base.ContinuousSaveImage(permanentSave);

        if (drawingBounds && drawingBounds.GetComponent<RemoteHelperImage>())
            drawingBounds.GetComponent<RemoteHelperImage>().sendImageToClient(permanentSave);
    }

    /// <summary>
    /// save the screen drawing annotation
    /// </summary>
    /// <param name="image">static ui image</param>
    public override void DeleteImage()
    {
        base.DeleteImage();

        if (drawingBounds && drawingBounds.GetComponent<RemoteHelperImage>())
            drawingBounds.GetComponent<RemoteHelperImage>().deleteImageFromClient();

        DrawingActive = false;
    }

    #endregion

    #region load
    /// <summary>
    /// load image date from annotation anchor to drawing area to edit the annotation again
    /// </summary>
    /// <param name="image">annotation anchor game object</param>
    public override void LoadImageFromAnchor(AnchorImage image)
    {
        base.LoadImageFromAnchor(image);
    }
    #endregion
}
