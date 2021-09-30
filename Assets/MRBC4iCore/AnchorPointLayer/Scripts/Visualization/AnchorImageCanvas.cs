using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(RectTransform))]
public class AnchorImageCanvas : AnchorImage
{
    protected Image anchorDisplayImage;
    protected RectTransform rectTransform;

    public override void SetComponents()
    {
        base.SetComponents();
        anchorDisplayImage = GetComponentInChildren<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    public override void SetTexture(Texture2D tex, bool permanentSave = true)
    {
        if (!componentsSet) SetComponents();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        anchorDisplayImage.sprite = sprite;
        anchorDisplayImage.enabled = StatusProperties.Values.ARActive;


        rectTransform.sizeDelta = new Vector2(tex.width, tex.height);

        if (GetComponent<BoxCollider2D>())
            GetComponent<BoxCollider2D>().size = rectTransform.rect.size;

        if (GetComponent<BoxCollider>())
            GetComponent<BoxCollider>().size = new Vector3(tex.width, tex.height, 0.01f);
    }

    public override Texture2D GetTexture()
    {
        return anchorDisplayImage.sprite.texture;
    }

    public override void SetPivot(Vector2 pivot)
    {
        if (!componentsSet) SetComponents();

        var pos = rectTransform.localPosition;
        rectTransform.pivot = pivot;
        rectTransform.localPosition = pos;

        if (GetComponent<BoxCollider>())
            GetComponent<BoxCollider>().center = (new Vector2(0.5f, 0.5f) - pivot) * GetComponent<BoxCollider>().size;
    }

    public override Vector2 GetPivot()
    {
        if (!componentsSet) SetComponents();

        return rectTransform.pivot;
    }

    public Vector2 GetSize()
    {
        if (!componentsSet) SetComponents();

        return rectTransform.localScale;
    }

    public void SetSize(Vector2 scale)
    {
        if (!componentsSet) SetComponents();

        rectTransform.localScale = scale;
    }
}
