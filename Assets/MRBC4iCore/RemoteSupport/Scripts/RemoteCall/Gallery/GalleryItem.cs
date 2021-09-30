using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Functionality of an gallery preview item
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class GalleryItem : MonoBehaviour, IPointerClickHandler
{
    public Image uiImage;
    public Image uiBorder;

    /// <summary>
    /// gallery preview UI element
    /// </summary>
    public Image UIImage
    {
        get
        {
            if (!uiImage)
                uiImage = GetComponentInChildren<Image>();
            return uiImage;
        }
    }

    private RectTransform rectTransform;
    /// <summary>
    /// size of the gallery preview item
    /// </summary>
    public RectTransform RectTransform
    {
        get
        {
            if (!rectTransform)
                rectTransform = GetComponent<RectTransform>();
            return rectTransform;
        }
    }

    /// <summary>
    /// border of the UI element visualizing the creator of the gallery entry
    /// </summary>
    public Image UIBorder
    {
        get
        {
            if (!uiBorder)
            {
                var imgs = GetComponentsInChildren<Image>();
                if (imgs.Length > 1)
                    uiBorder = imgs[1];
                else if (imgs.Length == 1)
                    uiBorder = imgs[0];
            }
            return uiBorder;
        }
    }

    public int AnchorId { get; set; }

    /// <summary>
    /// Who created the annotation
    /// </summary>
    public AnnotationOwner Owner
    {
        get
        {
            if (UIBorder)
            {
                if (UIBorder.color == Color.cyan)
                    return AnnotationOwner.Client;
            }
            return AnnotationOwner.Server;
        }
        set
        {
            if (UIBorder)
            {
                if (value == AnnotationOwner.Client)
                    UIBorder.color = Color.cyan;
                else UIBorder.color = Color.red;
            }
        }
    }


    /// <summary>
    /// on server created screeshots are flipped in y direction
    /// </summary>
    public int Orientation
    {
        get
        {
            if (UIImage)
                return (int)UIImage.transform.localScale.y;
            return 1;
        }
        set
        {
            if (UIImage)
                UIImage.transform.localScale = new Vector3(1, value, 1);
        }
    }


    /// <summary>
    /// Preview Image
    /// </summary>
    public Texture2D PreviewImage
    {
        get
        {
            if (UIImage)
                return UIImage.sprite.texture;
            return null;
        }
        set
        {
            if (value && UIImage)
            {
                Sprite sprite = Sprite.Create(value, new Rect(0, 0, value.width, value.height), new Vector2(0.5f, 0.5f));
                UIImage.sprite = sprite;
                StartCoroutine("matchSizeAfterDisplayed");
            }
        }
    }


    /// <summary>
    /// Index of the item in the anchor point layer
    /// </summary>
    public int DisplayIndex
    {
        get
        {
            return transform.GetSiblingIndex();
        }
    }

    /// <summary>
    /// manage click on gallery item 
    /// </summary>
    /// <param name="eventData">click event data</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        AnchorGalleryDetailManager.Instance.gameObject.SetActive(true);
        AnchorGalleryDetailManager.Instance.GetGalleryItem(AnchorId);
        AnchorGalleryOverviewManager.Instance.gameObject.SetActive(false);
    }

    /// <summary>
    /// Delete this annotation
    /// </summary>
    public void DeleteItem()
    {
        AnchorGalleryOverviewManager.Instance.DeleteItem(AnchorId);
    }

    /// <summary>
    /// synchronize the Preview Image texture size proportions with the size of the preview display images
    /// </summary>
    private void matchSize()
    {
        var displayHeigt = UIImage.rectTransform.rect.height;
        var displayWidth = PreviewImage.width * (displayHeigt / (float)PreviewImage.height);
        if (displayWidth < 1)
            displayWidth = RectTransform.rect.width;

        var sizeDelta = new Vector2(displayWidth, 0);
        UIImage.rectTransform.sizeDelta = sizeDelta;
    }

    /// <summary>
    /// synchronize the Preview Image texture size proportions with the size of the preview display images
    /// </summary>
    /// <returns></returns>
    IEnumerator matchSizeAfterDisplayed()
    {
        yield return new WaitForFixedUpdate();
        matchSize();
    }
}
