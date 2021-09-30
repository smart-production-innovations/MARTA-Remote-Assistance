using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Converts the projection into a texture
/// Deprecated: Used from AnchorAnnotationProjection which is very slow
/// </summary>
[RequireComponent(typeof(Camera))]
public class ProjectionResult : MonoBehaviour
{
    public ProjectionHelper projectionHelper;
    public Sprite projectedSprite;
    private bool coroutineRunning = false;

    private int renderDelay = 0;
    private int renderDelayDefault = 2;

    private void OnEnable()
    {
        renderDelay = renderDelayDefault;
    }

    /// <summary>
    /// Tear down RenderTexture.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (renderDelay == 0)
        {
            SaveBitmap(source);
        }
        Graphics.Blit(source, destination);
        renderDelay--;
    }


    /// <summary>
    /// save frame to a texture
    /// </summary>
    /// <param name="source"></param>
    void SaveBitmap(RenderTexture source)
    {
        if (!coroutineRunning)
        {
            StopAllCoroutines();
            StartCoroutine(CoroutineSendFrame(source));
        }
    }

    /// <summary>
    /// save frame to a texture
    /// </summary>
    /// <param name="source">target frame</param>
    /// <returns></returns>
    public IEnumerator CoroutineSendFrame(RenderTexture source)
    {
        try
        {
            coroutineRunning = true;

            var mTexture = new Texture2D(source.width, source.height, TextureFormat.ARGB32, false);
            mTexture.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
            mTexture.Apply();
            projectedSprite = Sprite.Create(mTexture, new Rect(0, 0, mTexture.width, mTexture.height), new Vector2(0.5f, 0.5f));
            projectionHelper.ProjectionRendered(projectedSprite);
        }
        catch (Exception e)
        {

        }

        coroutineRunning = false;

        yield return null;
    }

}
