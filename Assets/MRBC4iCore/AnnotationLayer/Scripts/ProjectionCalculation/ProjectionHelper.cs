using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper methods for projection calculation
/// Deprecated: Used from AnchorAnnotationProjection which is very slow
/// </summary>
public class ProjectionHelper : AManager<ProjectionHelper>
{
    public ImageProjector projector;
    public Camera projectionCamera;
    public Transform projectionPlane;
    public Camera outputCamera;

    private Image outputImage;

    /// <summary>
    /// Project the 2d annotation from the projector position onto the selected projection plane.
    /// Save the projection to a texture. 
    /// </summary>
    /// <param name="targetPosition">position of the selected AR plane</param>
    /// <param name="targetRotation">rotation of the selected AR plane</param>
    /// <param name="projectorPosition">position of the projector (camera position from which the snapshot was taken)</param>
    /// <param name="projectorRotation">rotation of the projector (camera position from which the snapshot was taken)</param>
    /// <param name="projectionTexture">annotation</param>
    /// <param name="outputImage">game object which shows the projection result</param>
    /// <param name="anchor">connected anchor point</param>
    public void CreateProjection(Vector3 targetPosition, Vector3 targetRotation, Vector3 projectorPosition, Vector3 projectorRotation, Texture2D projectionTexture, ref Image outputImage, AnchorPoint anchor)
    {
        EventNameManager.SendEventShowARHelper("CalcProjection");
        gameObject.SetActive(true);
        projectionCamera.fieldOfView = CameraHelper.ARCamera.fieldOfView;
        projectionCamera.nearClipPlane = CameraHelper.ARCamera.nearClipPlane;
        projectionCamera.farClipPlane = CameraHelper.ARCamera.farClipPlane;
        projector.transform.localPosition = projectorPosition;
        projector.transform.localEulerAngles = projectorRotation;
        projectionPlane.transform.localPosition = targetPosition;
        projectionPlane.transform.localEulerAngles = targetRotation;
        projector.SetProjectionTexture(projectionTexture);

        this.outputImage = outputImage;

        outputImage.transform.parent.position = targetPosition;
        outputImage.transform.parent.eulerAngles = targetRotation;

        if (anchor.PoseDriver != null)
        {
            AnchorPointManager.Instance.ChangeReferencePointForAnchor(anchor, targetPosition, Quaternion.Euler(targetRotation));
        }

        if (anchor.GetComponent<Image>() == null)
        {
            anchor.gameObject.AddComponent<Image>();
        }
    }

    /// <summary>
    /// save projection to a texture
    /// </summary>
    /// <returns></returns>
    protected Texture2D renderProjection()
    {
        if (outputCamera)
        {
            var oldActiveTexture = RenderTexture.active;
            var mRtBuffer = outputCamera.targetTexture;
            RenderTexture.active = mRtBuffer;

            outputCamera.Render();
            var mTexture = new Texture2D(mRtBuffer.width, mRtBuffer.height, TextureFormat.ARGB32, false);
            mTexture.ReadPixels(new Rect(0, 0, mRtBuffer.width, mRtBuffer.height), 0, 0, false);
            mTexture.Apply();

            RenderTexture.active = oldActiveTexture;
            return mTexture;
        }
        return null;
    }

    /// <summary>
    /// display projection in an UI element
    /// </summary>
    /// <returns></returns>
    IEnumerator createTexture2D()
    {
        // wait until UI interactions are handled
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        var tex = renderProjection();
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        outputImage.sprite = sprite;
        EventNameManager.SendEventHideARHelper();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// display projection sprite in an UI element
    /// </summary>
    /// <param name="projetedSprite"></param>
    public void ProjectionRendered(Sprite projetedSprite)
    {
        outputImage.sprite = projetedSprite;
        EventNameManager.SendEventHideARHelper();
        gameObject.SetActive(false);
    }
}
