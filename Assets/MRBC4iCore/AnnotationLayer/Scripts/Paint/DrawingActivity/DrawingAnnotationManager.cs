using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum AnnotationDrawingMode
{
    New,
    Edit
}

/// <summary>
/// manages all drawing activities for the annotation layer
/// </summary>
//[RequireComponent(typeof(AnnotationManager))]
public class DrawingAnnotationManager : DrawingManager<DrawingAnnotationManager>
{
    private AnnotationManager annotationManager;

    #region unity loop
    protected override void Awake()
    {
        base.Awake();
        annotationManager = AnnotationManager.Instance;
    }
    #endregion

    #region touch
    /// <summary>
    /// Define the action happens on touch or mouse down
    /// </summary>
    public override bool InputPositionUpEvents(Vector2 screenPosition)
    {
        var valid = base.InputPositionDownEvents(screenPosition);

        //ensure that in one unity loop an annotation is not saved and a new one is created at the same time.
        if (ReadyForNextDrawing && valid)
        {
            if (annotationManager && !annotationManager.LongPress)
            {
                createEmptyAndDisplayAnnotation(screenPosition, annotationOwner: AnnotationOwner.Client);
            }
        }
        return valid;
    }

    public void createEmptyAndDisplayAnnotation(Vector2 screenPosition, AnnotationOwner annotationOwner = AnnotationOwner.Client)
    {
        var drawImage = SearchHelper.FindSceneObjectOfType<DrawImage>();
        AnchorImage anchor;
        if (drawImage && drawImage.GetComponent<RectTransform>())
        {
            var overlayWidth = drawImage.GetComponent<RectTransform>().rect.width;
            var scaleFactor = (Screen.width / (overlayWidth / 2));

            //if short touch -> create new anchor point with an empty annotation
            anchor = createEmptyAnnotation(screenPosition, scaleFactor, annotationOwner: annotationOwner);
        }
        else
        {
            //if short touch -> create new anchor point with an empty annotation
            anchor = createEmptyAnnotation(screenPosition, annotationOwner: annotationOwner);
        }

        if (anchor)
        {
            ActiveAnchorId = anchor.Anchor.Id;
            anchor.OnDisplayInitEmptyContent();
        }
        else ActiveAnchorId = -1;
    }

    /// <summary>
    /// create new anchor point with an empty annotation
    /// </summary>
    /// <param name="position">screen position for the new anchor point</param>
    public AnchorImage createEmptyAnnotation(Vector2 position, float drawingAreaScale = 1, AnnotationOwner annotationOwner = AnnotationOwner.Client)
    {
        if (!annotationManager) return null;

        //create new empty annotation
        var anchor = annotationManager.createEmptyAnnotation(position, drawingAreaScale, annotationOwner: annotationOwner);
        if (anchor != null)
        {
            DisplayEmptyDrawingArea();
        }

        return anchor;
    }
    #endregion

    #region result
    /// <summary>
    /// show the screen drawing annotation in the canvas of the anchor annotation game object
    /// </summary>
    /// <param name="image">anchor annotation game object</param>
    public void SaveImageToAnchor(AnchorImage image)
    {
        if (annotationManager) annotationManager.SaveImageToAnchor(image, GetImageData());
    }

    /// <summary>
    /// show the screen drawing annotation in the canvas of the last anchor annotation game object
    /// </summary>
    public void SaveImageToLastAnchor()
    {
        if (annotationManager) annotationManager.SaveImageToLastAnchor(GetImageData());
        DrawingActive = false;
    }

    /// <summary>
    /// show the screen drawing annotation in the canvas of the selected anchor annotation game object
    /// </summary>
    public void SaveImageToSelectedAnchor()
    {
        if (annotationManager) annotationManager.SaveImageToSelectedAnchor(GetImageData()); 
        DrawingActive = false;
    }

    /// <summary>
    /// cache the screen drawing annotation while the drawing activity remains active
    /// </summary>
    public override void ContinuousSaveImage(bool permanentSave = false)
    {
        base.ContinuousSaveImage(permanentSave);

        if (annotationManager)
        {
            var anchor = AnchorPointManager.Instance.GetAnchorPoint(ActiveAnchorId);

            if (anchor)
            {
                var annotation = anchor.GetComponentInChildren<AnchorImage>();
                if (annotation)
                    if (annotationManager.SaveImageToAnchor(annotation, GetImageData(), permanentSave) && permanentSave)
                        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.NewGalleryItem, annotation.Anchor.Id.ToString()));
            }
        }
    }


    /// <summary>
    /// save the screen drawing annotation
    /// </summary>
    /// <param name="image">static ui image</param>
    public override void DeleteImage()
    {
        base.DeleteImage();
        if (annotationManager) annotationManager.RemovedAnchor(ActiveAnchorId);
    }

    /// <summary>
    /// deletes the selected anchor
    /// </summary>
    public void RemoveSelectedAnchor()
    {
        if (annotationManager) annotationManager.RemoveSelectedAnchor();
        DrawingActive = false;
    }

    /// <summary>
    /// cancel the drawing activities for the active annotation drawing. The last changes are discarded and the anchor point delete if it is empty.
    /// </summary>
    public override void CancelDrawing(bool goToGalleryOverviewIfGalleryIsOpen = false)
    {
        base.CancelDrawing(goToGalleryOverviewIfGalleryIsOpen);
        if (annotationManager) annotationManager.CancelDrawing(ActiveAnchorId);
    }

    /// <summary>
    /// show the screen drawing annotation in a static canvas for testing on pc
    /// </summary>
    /// <param name="image">static ui image</param>
    public void SaveImageToUIImage(Image image)
    {
        ImageHelper.SaveImageToUIImage(image, GetImageData());
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
        if (annotationManager) annotationManager.LoadImageFromAnchor(image);
    }
    #endregion
}
