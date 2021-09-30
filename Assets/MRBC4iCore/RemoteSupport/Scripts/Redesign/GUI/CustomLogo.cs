using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public static class IsImageExtension
{
    static List<string> jpg;
    static List<string> bmp;
    static List<string> gif;
    static List<string> png;

    public enum ImageType
    {
        JPG,
        BMP,
        GIF,
        PNG,
        NONE
    }

    const string JPG = "FF";
    const string BMP = "42";
    const string GIF = "47";
    const string PNG = "89";

    static IsImageExtension()
    {
        jpg = new List<string> { "FF", "D8" };
        bmp = new List<string> { "42", "4D" };
        gif = new List<string> { "47", "49", "46" };
        png = new List<string> { "89", "50", "4E", "47", "0D", "0A", "1A", "0A" };
    }

    public static bool IsImage(this string file, out ImageType type)
    {
        type = ImageType.NONE;
        if (string.IsNullOrWhiteSpace(file)) return false;
        if (!File.Exists(file)) return false;
        using (var stream = File.OpenRead(file))
            return stream.IsImage(out type);
    }

    public static bool IsImage(this Stream stream, out ImageType type)
    {
        type = ImageType.NONE;
        stream.Seek(0, SeekOrigin.Begin);
        string bit = stream.ReadByte().ToString("X2");
        switch (bit)
        {
            case JPG:
                if (stream.IsImage(jpg))
                {
                    type = ImageType.JPG;
                    return true;
                }
                break;
            case BMP:
                if (stream.IsImage(bmp))
                {
                    type = ImageType.BMP;
                    return true;
                }
                break;
            case GIF:
                if (stream.IsImage(gif))
                {
                    type = ImageType.GIF;
                    return true;
                }
                break;
            case PNG:
                if (stream.IsImage(png))
                {
                    type = ImageType.PNG;
                    return true;
                }
                break;
            default:
                break;
        }
        return false;
    }

    public static bool IsImage(this Stream stream, List<string> comparer)
    {
        stream.Seek(0, SeekOrigin.Begin);
        foreach (string c in comparer)
        {
            string bit = stream.ReadByte().ToString("X2");
            if (0 != string.Compare(bit, c))
                return false;
        }
        return true;
    }
}

public class CustomLogo : MonoBehaviour
{
    public Image logoInstance;

    #region parameter
    public static string getFileDirectory()
    {
        return FileHelper.GetDirectoryPath("Logo");
    }

    public static string getFilePath(string filename)
    {
        return Path.Combine(getFileDirectory(), filename);
    }

    public static Vector2 SetSpriteAlignment(SpriteAlignment align)
    {
        Vector2 pivot = Vector2.zero;
        switch (align)
        {
            case SpriteAlignment.Center:
                pivot = new Vector2(0.5f, 0.5f);
                break;
            case SpriteAlignment.TopLeft:
                pivot = new Vector2(0f, 1f);
                break;
            case SpriteAlignment.TopCenter:
                pivot = new Vector2(0.5f, 1f);
                break;
            case SpriteAlignment.TopRight:
                pivot = new Vector2(1f, 1f);
                break;
            case SpriteAlignment.LeftCenter:
                pivot = new Vector2(0f, 0.5f);
                break;
            case SpriteAlignment.RightCenter:
                pivot = new Vector2(1f, 0.5f);
                break;
            case SpriteAlignment.BottomLeft:
                pivot = new Vector2(0f, 0f);
                break;
            case SpriteAlignment.BottomCenter:
                pivot = new Vector2(0.5f, 0f);
                break;
            case SpriteAlignment.BottomRight:
                pivot = new Vector2(1f, 0f);
                break;
            case SpriteAlignment.Custom:
                pivot = new Vector2(0.5f, 0.5f);
                break;
            default:
                break;
        }
        return pivot;
    }
    #endregion

    private void Awake()
    {
        StartCoroutine(loadCustomLogo());
    }

    #region load
    public IEnumerator loadCustomLogo()
    {
        if (Directory.Exists(getFileDirectory()))
        {
            var files = Directory.GetFiles(getFileDirectory());
            var customLogoExists = false;
            foreach (var logoFile in files)
            {
                try
                {
                    IsImageExtension.ImageType type;
                    if (logoFile.IsImage(out type))
                    {
                        var sprite = loadAsset(logoFile, 8, SpriteAlignment.Center);
                        var logo = GameObject.Instantiate(logoInstance, transform);
                        logo.sprite = sprite;
                        logo.GetComponent<ImageAspect>().AspectRation = (float)sprite.texture.width / sprite.texture.height;
                        logo.enabled = true;
                        customLogoExists = true;
                    }
                }
                catch (Exception e)
                {

                }
            }

            if (!customLogoExists)
                gameObject.SetActive(false);
        }
        yield return null;
    }

    public static Sprite loadAsset(string filename, int pixelsPerUnit, SpriteAlignment align)
    {
        string filePath = "file://" + getFilePath(filename);
        WWW www = new WWW(filePath);

        while (!www.isDone)
        {
        }

        if (!string.IsNullOrEmpty(www.error))
        {

            Debug.Log(www.error + ": " + filePath);
        }
        else
        {            
            return initTexture(www.texture, filename, pixelsPerUnit, align);
        }

        return null;
    }

    public static Sprite initTexture(Texture2D texture, string filename, int pixelsPerUnit, SpriteAlignment align)
    {
        var sprite = Sprite.Create(texture as Texture2D, new Rect(0, 0, texture.width, texture.height), SetSpriteAlignment(align), pixelsPerUnit);
        sprite.name = filename;
        return sprite;
    }
    #endregion
}
