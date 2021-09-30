using UnityEngine;
using System.Collections;

/// <summary>
/// Extends the render texture functionality
/// </summary>
public static class ExtensionMethod
{
    /// <summary>
    /// convert a RenderTexture into a Texture2D
    /// </summary>
    /// <param name="rTex">source RenderTexture</param>
    /// <param name="tex">target Texture2D</param>
    /// <returns>result Texture2D</returns>
    public static Texture2D toTexture2D(this RenderTexture rTex, ref Texture2D tex)
    {
        // synchronize the size of the source and the target texture
        if (tex == null)
        {
            tex = new Texture2D(rTex.width, rTex.height, TextureFormat.ARGB32, false);
        }
        else
        {
            tex.width = rTex.width;
            tex.height = rTex.height;
        }

        // render the RenderTexture into the Texture2d
        var oldActiveTexture = RenderTexture.active;
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        // reset the camera/active render texture in case it is still used for other purposes
        RenderTexture.active = oldActiveTexture;

        return tex;
    }
}
