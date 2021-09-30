using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// helper methods for image calculation
/// </summary>
public static class ImageHelper
{
    /// <summary>
    /// show the screen drawing annotation in a static canvas for testing on pc
    /// </summary>
    /// <param name="image">static ui image</param>
    /// <param name="data">byte stream of the image</param>
    public static void SaveImageToUIImage(Image image, byte[] data)
    {
        Vector2 pivot = Vector2.zero;
        var tex = contentToTexture(data, out pivot);
        if (tex) SetImage(image, tex, pivot);
    }

    /// <summary>
    /// show the screen drawing annotation in a static canvas for testing on pc 
    /// </summary>
    /// <param name="image">static ui image</param>
    /// <param name="tex">texture</param>
    /// <param name="pivot">pivot point</param>
    private static void SetImage(Image image, Texture2D tex, Vector2 pivot)
    {
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));// new Vector2(.1f, .1f));
        image.sprite = sprite;
        image.GetComponent<RectTransform>().pivot = pivot;
    }

    /// <summary>
    /// convert the byte stream texture content to a Texture2D object
    /// </summary>
    /// <param name="data">byte stream texture content</param>
    /// <param name="pivot">pivot point</param>
    /// <returns>result texture2D</returns>
    public static Texture2D contentToTexture(byte[] data, out Vector2 pivot)
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        tex.LoadImage(data);
        pivot = new Vector2(0.5f, 0.5f);
        return tex;
    }

    /// <summary>
    /// convert byte array to texture
    /// </summary>
    /// <param name="data">byte array data</param>
    /// <returns>texture</returns>
    public static Texture2D convertToTexture(byte[] data)
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        tex.LoadImage(data);
        return tex;
    }

    /// <summary>
    /// convert texture to byte array
    /// </summary>
    /// <param name="tex">texture</param>
    /// <returns>byte array</returns>
    public static byte[] convertToByteArray(Texture2D tex)
    {
        if (tex.isReadable) 
            return tex.EncodeToPNG();

        return new byte[0];
    }

    /// <summary>
    /// crop the annotation image. Remove empty boarder space.
    /// </summary>
    /// <param name="texture">annotation drawing</param>
    /// <param name="pivot">shifted pivot point to original uncropped pivot position</param>
    /// <returns>cropped texture</returns>
    public static Texture2D cropTexture(Texture2D texture, out Vector2 pivot)
    {
        //target texture parameter
        Vector2 targetSize = new Vector2(texture.width, texture.height);
        Vector2 targetPivot = targetSize / 2f;

        //calculate empty space
        int right = getBound(texture, Bound.right);
        int left = getBound(texture, Bound.left);
        int top = getBound(texture, Bound.top);
        int bottom = getBound(texture, Bound.bottom);

        //image is completely empty
        if (left > right)
        {
            pivot = Vector2.zero;
            return null;
        }

        var newSize = new Vector2Int(right - left, top - bottom);

        //create cropped image
        Texture2D tex = new Texture2D(newSize.x, newSize.y, TextureFormat.ARGB32, false);
        tex.SetPixels(texture.GetPixels(left, bottom, newSize.x, newSize.y));
        tex.Apply();

        //create new pivot according to the same position as the target pivot position
        pivot = targetPivot - new Vector2((left + right) / 2f, (bottom + top) / 2f);
        pivot /= newSize;
        pivot += new Vector2(0.5f, 0.5f);

        return tex;
    }

    private enum Bound
    {
        right,
        left,
        top,
        bottom
    }

    /// <summary>
    /// calculate empty boarder space 
    /// </summary>
    /// <param name="texture">target texture</param>
    /// <param name="bound">calculation direction (right, left, top or bottom)</param>
    /// <returns>first pixel with data</returns>
    private static int getBound(Texture2D texture, Bound bound)
    {
        int width = 1, height = texture.height;
        int from = 0, to = 0;
        int direction = 1;

        switch (bound)
        {
            case Bound.right:
                from = texture.width - 1;
                to = 0;
                width = 1;
                height = texture.height;
                direction = -1;
                break;
            case Bound.left:
                from = 0;
                to = texture.width - 1;
                width = 1;
                height = texture.height;
                direction = 1;
                break;
            case Bound.top:
                from = texture.height - 1;
                to = 0;
                width = texture.width;
                height = 1;
                direction = -1;
                break;
            case Bound.bottom:
                from = 0;
                to = texture.height - 1;
                width = texture.width;
                height = 1;
                direction = 1;
                break;
            default:
                break;
        }

        int index = from;
        for (int i = from; i != to; i += direction)
        {
            int x = 0, y = 0;
            switch (bound)
            {
                case Bound.right:
                case Bound.left:
                    x = i;
                    break;
                case Bound.top:
                case Bound.bottom:
                    y = i;
                    break;
                default:
                    break;
            }
            var pixels = texture.GetPixels(x, y, width, height);
            foreach (var pixel in pixels)
            {
                if (pixel.a > 0)
                    return index;
            }

            index = i;
        }

        return to;
    }
}

/// <summary>
/// manages the annotation layer
/// </summary>
public class AnnotationManager : AnnotationManager<AnnotationManager>
{
}


/// <summary>
/// manages the annotation layer
/// </summary>
/// <typeparam name="T">Type of the final class to get right instance typecast</typeparam>
public class AnnotationManager<T> : AnchorPointSelection<T> where T : Component
{
    //prefab for instancing new annotation drawing
    public AnchorImage AnnotationCanvasPrefab;

    public RawImage[] NonARDisplay = new RawImage[] { };

    // camera pose relative to new anchor
    private Pose currentAnchorCameraPose;

    //private Sprite emptySprite = null;
    private Texture2D emptyTexture = null;

    #region unity loop
    private void Awake()
    {
        base.Awake();
        if (getNonARDisplay(AnnotationOwner.Server))
            emptyTexture = (Texture2D)getNonARDisplay(AnnotationOwner.Server).texture;
    }

    private RawImage getNonARDisplay(AnnotationOwner annotationOwner)
    {
        if (NonARDisplay.Length > (int)annotationOwner)
        {
            return NonARDisplay[(int)annotationOwner];
        }
        else if (NonARDisplay.Length > 0)
            return NonARDisplay[0];
        return null;
    }
    #endregion

    #region touch
    /// <summary>
    /// create new anchor point with an empty annotation
    /// </summary>
    /// <param name="position">screen position for the new anchor point</param>
    public virtual AnchorImage createEmptyAnnotation(Vector2 position, float drawingAreaScale = 1, AnnotationOwner annotationOwner = AnnotationOwner.Client)
    {
        //create new anchor point
        AnchorPoint anchor;
        Transform plane = null;
        if (StatusProperties.Values.ARActive)
            anchor = AnchorPointManager.Instance.AddAnchorPoint(position.x, position.y, out plane);
        else
        {
            anchor = AnchorPointManager.Instance.AddAnchorPoint(Vector3.zero, Quaternion.identity, setPoseDriver: false);
        }
        if (anchor)
        {
            ComputeCameraPose(anchor);
            var canvas  = createAnnotationCanvas(anchor, position, drawingAreaScale, plane, annotationOwner: annotationOwner);
            return canvas;
        }

        return null;
    }

    /// <summary>
    /// create new anchor point with an empty annotation
    /// </summary>
    /// <param name="position">room position for the new anchor point</param>
    public virtual AnchorImage createEmptyAnnotation(Vector3 position, Quaternion rotation, float drawingAreaScale = 1, AnnotationOwner annotationOwner = AnnotationOwner.Client)
    {
        //create new anchor point
        var anchor = AnchorPointManager.Instance.AddAnchorPoint(position, rotation);
        if (anchor)
        {
            ComputeCameraPose(anchor);
            return createAnnotationCanvas(anchor, new Vector2(Screen.width / 2, Screen.height / 2), drawingAreaScale, annotationOwner: annotationOwner);
        }
        return null;
    }

    /// <summary>
    /// calculate the actual AR camera position to save the camera world position with the screenshot
    /// </summary>
    /// <param name="anchor"></param>
    private void ComputeCameraPose(AnchorPoint anchor)
    {
        var cam = CameraHelper.ARCamera;
        currentAnchorCameraPose.position = cam.transform.position - anchor.transform.position;
        currentAnchorCameraPose.rotation = cam.transform.rotation * Quaternion.Inverse(cam.transform.rotation);
    }

    /// <summary>
    /// create an annotation object an add it to the anchor point
    /// </summary>
    public AnchorImage createAnnotation(AnchorPoint anchor, AnnotationOwner annotationOwner = AnnotationOwner.Client)
    {
        return createAnnotationCanvas(anchor, new Vector2(Screen.width / 2, Screen.height / 2), annotationOwner: annotationOwner);
    }

    /// <summary>
    /// create an annotation object an add it to the anchor point
    /// </summary>
    /// <param name="anchor">anchor point</param>
    /// <param name="position">screen click position</param>
    /// <param name="drawingAreaScale">scale factor at the time of creation. This information is needed for reload annotation on other devices or with different windows size.</param>
    private AnchorImage createAnnotationCanvas(AnchorPoint anchor, Vector2 position, float drawingAreaScale = 1, Transform plane = null, AnnotationOwner annotationOwner = AnnotationOwner.Client)
    {
        if (anchor != null)
        {
            //add an empty annotation to the anchor point object
            var canvas = Instantiate(AnnotationCanvasPrefab, anchor.transform);
            canvas.Instantiate();
            canvas.transform.localPosition = Vector3.zero;

            //Transfer the 2D drawing into 3D space by using one of the projection types.

            //Projection type 1 (deprecated): Anchor the drawing to a feature point. The annotation is scaled according to the distance between camera and feature point.
            if (canvas is AnchorAnnotationDistanceScale)
            {
                var annoation = (AnchorAnnotationDistanceScale)canvas;
                annoation.DrawingAreaScale = drawingAreaScale;
                annoation.ScreenClickPosition = position;

                var anchorPos = anchor.transform.position;

                var distance = CameraHelper.getDistance(CameraHelper.ARCamera.transform, anchorPos);

                annoation.DistanceToSnapshot = distance;
            }

            //Projection type 2 (recommended): Projection of the drawing onto a plane in 3d space. This plane can be a reconstructed AR plane or a plane parallel to the camera.
            if (canvas is AnchorAnnotationProjection3D)
            {
                var annotation = (AnchorAnnotationProjection3D)canvas;
                annotation.AnnotationOwner = annotationOwner;
                annotation.SetProjector(CameraHelper.ARCamera.transform.position, CameraHelper.ARCamera.transform.eulerAngles);

                annotation.SetPoint(anchor.transform.position, anchor.transform.eulerAngles);
                if (plane != null)
                {
                    annotation.SetPlane(plane.position, plane.eulerAngles);
                }

                if (getNonARDisplay(annotation.AnnotationOwner))
                    annotation.NonARDisplay = getNonARDisplay(annotation.AnnotationOwner);
            }

            return canvas;
        }
        return null;
    }
    #endregion

    #region result
    /// <summary>
    /// show the screen drawing annotation in the canvas of the anchor annotation game object
    /// </summary>
    /// <param name="image">anchor annotation game object</param>
    /// <param name="data">byte stream of the image</param>
    public bool SaveImageToAnchor(AnchorImage image, byte[] data, bool permanentSave = true)
    {
        Vector2 pivot = Vector2.zero;
        var tex = ImageHelper.contentToTexture(data, out pivot);
        if (tex)
        {
            image.SetTexture(tex, permanentSave);
            image.SetPivot(pivot);

            AnchorPointManager.Instance.SelectedAnchor = null;
            return true;
        }
        else
        {
            //remove anchor if image is completely empty
            AnchorPointManager.Instance.RemoveAnchorPoint(image.Anchor);
            return false;
        }
    }

    /// <summary>
    /// show the screen drawing annotation in the canvas of the last anchor annotation game object
    /// </summary>
    public virtual AnchorImage SaveImageToAnchor(int anchorId, byte[] data, float scaleFactor, bool permanentSave = true)
    {
        var anchor = AnchorPointManager.Instance.GetAnchorPoint(anchorId);

        if (anchor)
        {
            var annotation = anchor.GetComponentInChildren<AnchorImage>();
            if (annotation)
            {
                if (annotation is AnchorAnnotationDistanceScale)
                    ((AnchorAnnotationDistanceScale)annotation).DrawingAreaScale = scaleFactor;
                if (SaveImageToAnchor(annotation, data, permanentSave) && permanentSave)
                {
                    CommunicationManager.Instance.ReceiveNewGalleryItem(annotation.Anchor.Id.ToString());
                }
            }
            return annotation;
        }

        return null;
    }

    /// <summary>
    /// show the screen drawing annotation in the canvas of the last anchor annotation game object
    /// </summary>
    public virtual AnchorImage SaveImageToLastAnchor(byte[] data, bool permanentSave = true)
    {
        var anchor = AnchorPointManager.Instance.GetLastAnchorPoint().GetComponentInChildren<AnchorImage>();

        if (anchor)
            SaveImageToAnchor(anchor, data, permanentSave);

        return anchor;
    }

    /// <summary>
    /// show the screen drawing annotation in the canvas of the selected anchor annotation game object
    /// </summary>
    public virtual AnchorImage SaveImageToSelectedAnchor(byte[] data)
    {
        var anchor = getActiveAnchor();
        if (anchor != null)
            SaveImageToAnchor(anchor, data);
        else
            SaveImageToLastAnchor(data);

        return anchor;
    }

    /// <summary>
    /// get the selected anchor annotation
    /// </summary>
    /// <returns>selected anchor annotation</returns>
    protected AnchorImage getActiveAnchor()
    {
        if (AnchorPointManager.Instance.SelectedAnchor)
            return AnchorPointManager.Instance.SelectedAnchor.GetComponentInChildren<AnchorImage>();
        return null;
    }

    /// <summary>
    /// get the anchor annotation
    /// </summary>
    /// <returns>selected anchor annotation</returns>
    public AnchorImage getAnchor(int anchorId, int relativIndex = 0)
    {
        var anchor = AnchorPointManager.Instance.GetAnchorPoint(anchorId, relativIndex);
        if (anchor)
            return anchor.GetComponentInChildren<AnchorImage>();
        return null;
    }

    /// <summary>
    /// get the anchor annotation
    /// </summary>
    /// <returns>selected anchor annotation</returns>
    public AnchorImage getFirstAnchor()
    {
        var anchor = AnchorPointManager.Instance.GetAnchorPointOfIndex(0);
        if (anchor)
            return anchor.GetComponentInChildren<AnchorImage>();
        return null;
    }

    /// <summary>
    /// deletes the selected anchor
    /// </summary>
    public virtual void RemoveSelectedAnchor()
    {
        AnchorPointManager.Instance.RemoveAnchorPoint(AnchorPointManager.Instance.SelectedAnchor);
    }

    /// <summary>
    /// deletes the selected anchor
    /// </summary>
    public virtual void RemovedAnchor(int anchorId)
    {
        var anchor = AnchorPointManager.Instance.GetAnchorPoint(anchorId);
        if (anchor)
            AnchorPointManager.Instance.RemoveAnchorPoint(anchor);
    }

    /// <summary>
    /// cancel the drawing activities for the active annotation drawing. The last changes are discarded and the anchor point delete if it is empty.
    /// </summary>
    public virtual void CancelDrawing(int anchorId = -1)
    {
        AnchorPoint anchor = null;
        IAnchorAnnotationBase annotation = null;
        if (anchorId >= 0) anchor = AnchorPointManager.Instance.GetAnchorPoint(anchorId);

        if (anchor == null) anchor = AnchorPointManager.Instance.SelectedAnchor;
        if (anchor == null) anchor = AnchorPointManager.Instance.GetLastAnchorPoint();

        if (anchor) annotation = anchor.GetComponentInChildren<AnchorImage>() as IAnchorAnnotationBase;


        if (annotation != null && annotation.IsEmpty)
        {
            AnchorPointManager.Instance.RemoveAnchorPoint(anchor);

            if (getNonARDisplay(annotation.AnnotationOwner))
                getNonARDisplay(annotation.AnnotationOwner).texture = emptyTexture; //.sprite = emptySprite;
        }
        else
        {
            AnchorPointManager.Instance.SelectedAnchor = null;
        }
    }
    #endregion

    #region load
    /// <summary>
    /// load image date from annotation anchor to drawing area to edit the annotation again
    /// </summary>
    /// <param name="image">annotation anchor game object</param>
    public virtual void LoadImageFromAnchor(AnchorImage image)
    {
        var tex = image.GetTexture();
        var pivot = image.GetPivot();
        DrawFreeHand.Instance.loadDrawing(tex, pivot);
    }
    #endregion
}
