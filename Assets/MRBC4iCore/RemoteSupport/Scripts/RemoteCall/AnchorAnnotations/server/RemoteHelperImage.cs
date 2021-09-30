using Byn.Awrtc.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// send expert drawing to caller device
/// </summary>
public class RemoteHelperImage : MonoBehaviour
{
    public int anchorId;
    //annotation texture 
    private Texture2D mTexture;
    private byte[] mByteBuffer = null;

    /// <summary>
    /// Define the active anchorId on the device
    /// </summary>
    public static int AnchorId
    {
        get
        {
            var helper = SearchHelper.FindSceneObjectOfType<RemoteHelperImage>();
            if (helper)
                return helper.anchorId;
            return 0;
        }
        set
        {
            var helper = SearchHelper.FindSceneObjectsOfTypeAll<RemoteHelperImage>();
            foreach (var item in helper)
            {
                item.anchorId = value;
            }

            if (DrawingRemoteManager.HasInstance)
                DrawingRemoteManager.Instance.ActiveAnchorId = value;
        }
    }

    void Awake()
    {
	    int width = ResolutionManager.Instance.pWidth;
        int height = ResolutionManager.Instance.pHeight;
        if (width > 0 && height > 0)
        {
            CreateTexture(width, height);
        }
        ResolutionManager.OnPResolutionChanged += CreateTexture;
        ActionEventManager.Subscribe(EventName.SendImageToClient, sendImageToClient);
    }

    /// <summary>
    /// adapt the resolution of the annotation texture according to the resolution and orientation of the mobile remote device
    /// </summary>
    /// <param name="width">width of the annotation texture</param>
    /// <param name="height">height of the annotation texture</param>
    private void CreateTexture(int width, int height)
    {
        mTexture = new Texture2D(StatusProperties.Values.DrawingResizeFactor * width, StatusProperties.Values.DrawingResizeFactor * height, TextureFormat.ARGB32, false);
    }

    void OnDestroy()
    {
        ActionEventManager.Unsubscribe(EventName.SendImageToClient, sendImageToClient);
		
		ResolutionManager.OnPResolutionChanged -= CreateTexture;
    }

    /// <summary>
    /// send expert drawing to caller device
    /// </summary>
    public void sendImageToClient()
    {
        sendImageToClient(true);
    }

    /// <summary>
    /// send expert drawing to caller device
    /// </summary>
    /// <param name="permanentSave">false: continues save is active. Immediately send each image change to the client and mark the change as temporary. This is important when calling the cancel task.</param>
    public void sendImageToClient(bool permanentSave)
    {
        var temp = RenderTexture.active;

        RenderTexture.active = DrawingRemoteManager.Instance.TemporaryRenderTexture;
        mTexture.ReadPixels(new Rect(0, 0, DrawingRemoteManager.Instance.TemporaryRenderTexture.width, DrawingRemoteManager.Instance.TemporaryRenderTexture.height), 0, 0, false);
        mTexture.Apply();

        if (ARPlaneDisplayManager.HasInstance)
            ARPlaneDisplayManager.Instance.SetDrawingTexture(mTexture);

        RenderTexture.active = temp;

        mByteBuffer = mTexture.EncodeToPNG();

        var data = new AnnotationImageData(anchorId, mByteBuffer, 
            DrawingRemoteManager.Instance.DrawingOverlayScaleFactor, 
            permanentSave);

        // The annotation data is transferred from the expert to the worker on site.
        CommunicationManager.Instance.SendData(data);
    }

    /// <summary>
    /// send delete anchor point with annotation to caller device
    /// </summary>
    public void deleteImageFromClient()
    {
        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.DeleteAnchor, anchorId.ToString()));
    }


    /// <summary>
    /// send cancel annotation to caller device
    /// </summary>
    public void cancelImageActionOnClient()
    {
        EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.CancelAnchor, anchorId.ToString()));
    }
}
