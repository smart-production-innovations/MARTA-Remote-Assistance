using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages switching between different detected AR planes for the drawing activity
/// </summary>
public class ARPlaneDisplayManager : AManager<ARPlaneDisplayManager>
{
    public GameObject planContrainerTransform;
    public GameObject planContrainer;
    public ImageProjector projectorSnapshot;
    public ImageProjector projectorDrawing;
    public MeshFilter planPrefab;
    public Transform bgPlane;
    public Camera outputTextureCamera;
    public Transform drawingPlane;

    private Texture bgTexture;
    private bool createLayerPreviews = false;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        if (createLayerPreviews)
        {
            StopAllCoroutines();
            StartCoroutine("createTextureLayersAsync");
            createLayerPreviews = false;
        }
    }

    /// <summary>
    /// sets all received layers from client as individual drawing layer
    /// </summary>
    /// <param name="planes"></param>
    public void SetPlanes(List<ARLayerPlane> planes)
    {
        projectorSnapshot.ClearProjectionDynamicReceivers();
        if (planContrainer)
        {
            planContrainer.transform.DeleteAllChildren();

            foreach (var item in planes)
            {
                var plane = Instantiate(planPrefab, item.Position, item.Rotation, planContrainer.transform);
                plane.transform.localPosition = item.Position;
                plane.transform.localRotation = item.Rotation;
                plane.transform.localScale = item.Scale;
                var mesh = plane.GetComponentInChildren<MeshFilter>();
                mesh.mesh = item.DisplayMesh;
                projectorSnapshot.AddProjectionDynamicReceivers(plane.GetComponentInChildren<MeshRenderer>());

                projectorSnapshot.camera.nearClipPlane = item.CameraNearPlane;
                projectorSnapshot.camera.farClipPlane = item.CameraFarePlane;
                projectorSnapshot.camera.fieldOfView = item.CameraFOV;// / 3 * 2;
                bgPlane.localPosition = new Vector3(0,0, Mathf.Min(50, item.CameraFarePlane-0.01f));
            }
        }
        createLayerPreviews = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// set the position and rotation of the projection plane
    /// </summary>
    /// <param name="PlanePosition">plane position</param>
    /// <param name="PlaneRotation">plane rotation</param>
    public void setDrawingProjectionLocation(Vector3 PlanePosition, Vector3 PlaneRotation)
    {
        drawingPlane.localEulerAngles = PlaneRotation;
        drawingPlane.localPosition = PlanePosition;
    }

    /// <summary>
    /// set the last received video image as background texture to draw on
    /// </summary>
    /// <param name="screenshot"></param>
    public void SetTexture(Texture screenshot, flipDirection flip = flipDirection.horizontal)
    {
        if (projectorSnapshot)
        {
            screenshot = FlipTexture((Texture2D)screenshot, flip);
            projectorSnapshot.ProjectionTexture = screenshot;
            bgTexture = screenshot;
        }
        createLayerPreviews = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// In which direction should the texture be inverted?
    /// </summary>
    public enum flipDirection
    {
        horizontal,
        vertical,
        both,
        none
    }

    /// <summary>
    /// mirroring a texture in x or / and y direction
    /// </summary>
    /// <param name="original">initial texture to be mirrored</param>
    /// <param name="flip">mirror axes</param>
    /// <returns></returns>
    public static Texture2D FlipTexture(Texture2D original, flipDirection flip = flipDirection.horizontal)
    {
        if (original && original.isReadable)
        {
            Texture2D flipped = new Texture2D(original.width, original.height);

            int width = original.width;
            int height = original.height;


            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row < height; row++)
                {
                    int x = col;
                    int y = row;
                    switch (flip)
                    {
                        case flipDirection.horizontal:
                            x = (width - 1) - col;
                            break;
                        case flipDirection.vertical:
                            y = (height - 1) - row;
                            break;
                        case flipDirection.both:
                            x = (width - 1) - col;
                            y = (height - 1) - row;
                            break;
                        default:
                            break;
                    }
                    flipped.SetPixel(x, y, original.GetPixel(col, row));
                }
            }

            flipped.Apply();

            return flipped;
        }
        return original;
    }

    /// <summary>
    /// Set the rotation of the projection on the AR planes according to the client device
    /// </summary>
    /// <param name="eulerAngle">angle of rotation</param>
    public void SetCameraRotation(Vector3 eulerAngle)
    {
        projectorSnapshot.transform.parent.localEulerAngles = eulerAngle;
        createLayerPreviews = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Get the rotation of the projection
    /// </summary>
    /// <returns></returns>
    public Vector3 getCameraRotation()
    {
        return projectorSnapshot.transform.parent.localEulerAngles;
    }

    /// <summary>
    /// Get the position of the snapshot
    /// </summary>
    /// <returns></returns>
    public Vector3 getCameraPosition()
    {
        return projectorSnapshot.transform.parent.localPosition;
    }

    /// <summary>
    /// Set the rotation of the projection on the AR planes according to the client device
    /// </summary>
    /// <param name="eulerAngle"></param>
    public void SetCameraPosition(Vector3 pos)
    {
        projectorSnapshot.transform.parent.localPosition = pos;
        createLayerPreviews = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// set the texture parameter of the projection shader
    /// </summary>
    private void setProjectionTexture()
    {
        var meshRender = planContrainer.GetComponentInChildren<MeshRenderer>();
        if (meshRender != null)
            meshRender.material.SetTexture("_ShadowMap", bgTexture);
    }

    /// <summary>
    /// set the projection offset of the projection shader
    /// </summary>
    private void setProjectionOffset()
    {
        var meshRender = planContrainer.GetComponentInChildren<MeshRenderer>();
        if (meshRender != null)
        {
            meshRender.material.SetVector("_ProjectorOffset", projectorSnapshot.transform.position);
        }
    }

    /// <summary>
    /// set the projection offset of the projection shader
    /// </summary>
    private void setProjectionRotation()
    {
        var meshRender = planContrainer.GetComponentInChildren<MeshRenderer>();
        if (meshRender != null)
        {
            meshRender.material.SetVector("_ProjectorRotation", projectorSnapshot.transform.eulerAngles);
        }
    }

    /// <summary>
    /// generate the preview images for manual project plane selection
    /// </summary>
    public void createTextureLayers()
    {
        if (outputTextureCamera)
        {
            LayerPlaneContainer.Instance.clear();
            // add the default option of anchoring in a feature point with projection parallel to the camera
            LayerPlaneContainer.Instance.add((Texture2D)FlipTexture((Texture2D)bgTexture, flipDirection.both), drawingPlane.localPosition, drawingPlane.localEulerAngles, true);

            // hide all AR planes to activate them individually for the thumbnails
            foreach (Transform plane in planContrainer.transform)
            {
                plane.gameObject.SetActive(false);
            }
            var oldActiveTexture = RenderTexture.active;
            var mRtBuffer = outputTextureCamera.targetTexture;
            RenderTexture.active = mRtBuffer;
            foreach (Transform plane in planContrainer.transform)
            {
                // active the AR plane to be visualized by the thumbnail
                plane.gameObject.SetActive(true);

                // create a preview image for each AR plane found in the camera's field of view
                outputTextureCamera.Render();
                var mTexture = new Texture2D(mRtBuffer.width, mRtBuffer.height, TextureFormat.ARGB32, false);
                mTexture.ReadPixels(new Rect(0, 0, mRtBuffer.width, mRtBuffer.height), 0, 0, false);
                mTexture.Apply();
                LayerPlaneContainer.Instance.add(mTexture, plane.localPosition, plane.localEulerAngles);
                plane.gameObject.SetActive(false);
            }
            RenderTexture.active = oldActiveTexture;
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// generate the preview images for manual project level selection
    /// </summary>
    /// <returns></returns>
    IEnumerator createTextureLayersAsync()
    {
        // wait until UI interactions are handled
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        createTextureLayers();
    }

    /// <summary>
    /// set the annotation texture
    /// </summary>
    /// <param name="drawing">annotation texture</param>
    public void SetDrawingTexture(Texture2D drawing)
    {
        projectorDrawing.ProjectionTexture = drawing;
    }
}
