using UnityEngine;
using System.Collections;

/// <summary>
/// create, manage and store screenshot data at the time an annotation is created
/// </summary>
public class Snapshot : MonoBehaviour
{
    private Texture2D snapshotTexture;
    private RenderTexture mRtBuffer = null;
    public bool linkSnapshotToAnchor = true;

    void Awake()
    {
        Instantiate();
    }

    void OnDestroy()
    {
        Destroy(mRtBuffer);
        Destroy(snapshotTexture);
    }

    /// <summary>
    /// Texture of the screenshot
    /// </summary>
    public Texture2D SnapshotTexture
    {
        get
        {
            return snapshotTexture;
        }

        internal set
        {
            snapshotTexture = value;
        }
    }

    private bool isInstantiated = false;
    public void Instantiate()
    {
        if (!isInstantiated)
        {
            // do not recreate snapshot texture when anchor point is loaded from file
            if (snapshotTexture == null)
            {
                makeSnapshot();

                if (linkSnapshotToAnchor)
                {
                    var image = GetComponent<AnchorImage>();
                    if (image != null)
                    {
                        image.SetTexture(snapshotTexture);
                    }
                }
            }
        }
    }


    //private void makeSnapshot()
    //{
    //    var cam = CameraHelper.CaptureCamera;

    //    if (snapshotTexture == null)
    //    {
    //        mRtBuffer = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.ARGB32);
    //        mRtBuffer.wrapMode = TextureWrapMode.Repeat;
    //        snapshotTexture = new Texture2D(cam.pixelWidth, cam.pixelHeight, TextureFormat.ARGB32, false);
    //    }
    //    else
    //    {
    //        mRtBuffer.width = snapshotTexture.width = cam.pixelWidth;
    //        mRtBuffer.height = snapshotTexture.height = cam.pixelHeight;
    //    }

    //    var oldTargetTexture = cam.targetTexture;
    //    var oldActiveTexture = RenderTexture.active;

    //    //Set the buffer as target and render the view of the camera into it
    //    cam.targetTexture = mRtBuffer;
    //    cam.Render();

    //    RenderTexture.active = mRtBuffer;
    //    snapshotTexture.ReadPixels(new Rect(0, 0, mRtBuffer.width, mRtBuffer.height), 0, 0, false);
    //    snapshotTexture.Apply();

    //    //reset the camera/active render texture  in case it is still used for other purposes
    //    cam.targetTexture = oldTargetTexture;
    //    RenderTexture.active = oldActiveTexture;
    //}

    /// <summary>
    /// create a snapshot
    /// </summary>
    private void makeSnapshot()
    {
        var cam = CameraHelper.WebCam;
        var camResolution = cam;// CameraHelper.CaptureCamera;

        /*
        var oldActiveTexture = RenderTexture.active;

        if (cam.targetTexture != null)
        {
            if (snapshotTexture == null)
            {
                snapshotTexture = new Texture2D(camResolution.targetTexture.width, camResolution.targetTexture.height, TextureFormat.ARGB32, false);
            }
            else
            {
                snapshotTexture.width = camResolution.targetTexture.width;
                snapshotTexture.height = camResolution.targetTexture.height;
            }
            RenderTexture.active = cam.targetTexture;
        }
        else
        {
            if (snapshotTexture == null)
            {
                mRtBuffer = new RenderTexture(camResolution.pixelWidth, camResolution.pixelHeight, 0, RenderTextureFormat.ARGB32);
                mRtBuffer.wrapMode = TextureWrapMode.Repeat;
                snapshotTexture = new Texture2D(camResolution.pixelWidth, camResolution.pixelHeight, TextureFormat.ARGB32, false);
            }
            else
            {
                mRtBuffer.width = snapshotTexture.width = camResolution.pixelWidth;
                mRtBuffer.height = snapshotTexture.height = camResolution.pixelHeight;
            }

            var oldTargetTexture = cam.targetTexture;

            //Set the buffer as target and render the view of the camera into it
            cam.targetTexture = mRtBuffer;
            cam.Render();
            cam.targetTexture = oldTargetTexture;

            RenderTexture.active = mRtBuffer;
        }

        snapshotTexture.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0, false);
        snapshotTexture.Apply();

        //reset the camera/active render texture  in case it is still used for other purposes
        RenderTexture.active = oldActiveTexture;
        */

        if (cam.targetTexture == null)
        {
            if (mRtBuffer == null)
            {
                mRtBuffer = new RenderTexture(camResolution.pixelWidth, camResolution.pixelHeight, 0, RenderTextureFormat.ARGB32);
                mRtBuffer.wrapMode = TextureWrapMode.Repeat;
            }
            else
            {
                mRtBuffer.width = camResolution.pixelWidth;
                mRtBuffer.height = camResolution.pixelHeight;
            }

            //Set the buffer as target and render the view of the camera into it
            cam.targetTexture = mRtBuffer;
            cam.Render();
            cam.targetTexture = null;
            mRtBuffer.toTexture2D(ref snapshotTexture);
        }
        else
        {
            cam.targetTexture.toTexture2D(ref snapshotTexture);
        }
    }

}
