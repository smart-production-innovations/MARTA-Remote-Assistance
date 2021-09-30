using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class AnchorImage3D : AnchorImage
{
    private MeshRenderer anchorDisplayImage;
    private Vector2 pivot = new Vector2(0.5f, 0.5f);



    public override void SetTexture(Texture2D tex, bool permanentSave = true)
    {
        if (anchorDisplayImage == null)
            anchorDisplayImage = GetComponent<MeshRenderer>();

        anchorDisplayImage.material.mainTexture = tex;
    }

    public override Texture2D GetTexture()
    {
        return (Texture2D)anchorDisplayImage.sharedMaterial.mainTexture;
    }

    public override void SetPivot(Vector2 pivot)
    {
        if (GetComponent<RectTransform>())
        {
            GetComponent<RectTransform>().pivot = pivot;
        }
        this.pivot = pivot;
    }


    public override Vector2 GetPivot()
    {
        if (GetComponent<RectTransform>())
        {
            return GetComponent<RectTransform>().pivot;
        }
        return pivot;
    }
}

