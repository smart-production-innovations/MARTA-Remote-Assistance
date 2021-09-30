using UnityEngine;

public abstract class AnchorImage : AnchorContent
{
    public Texture2D Content;

    protected override void instantiate()
    {
        if (Content != null)
        {
            SetTexture(Content);
        }

        base.instantiate();
    }



    public void SetTexture(byte[] tex)
    {

    }

    public abstract void SetTexture(Texture2D tex, bool permanentSave = true);
    public abstract Texture2D GetTexture();

    public abstract void SetPivot(Vector2 pivot);
    public abstract Vector2 GetPivot();
}
