using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Modification of the "Image Projector Shader For Unity3D" project
//https://forum.unity.com/threads/solved-image-projection-shader.254196/
//http://www.xdpixel.com/downloads/unity/ImageProjector.zip

public class ImageProjector : MonoBehaviour
{
    public Texture ProjectionTexture = null;
    public Renderer[] ProjectionStaticReceivers = null;
    protected List<Renderer> ProjectionDynamicReceivers = new List<Renderer>();
    public float Angle = 0.0f;
    public bool flipProjection = false;

    public Camera camera;
    public float aspect = 1;
    private VideoSize videoSize;

    Vector4 Vec3ToVec4(Vector3 vec3, float w)
    {
        return new Vector4(vec3.x, vec3.y, vec3.z, w);
    }

    void Start()
    {
        videoSize = SearchHelper.FindSceneObjectOfType<VideoSize>();
        if (!camera)
            camera = GetComponent<Camera>();

        if (ProjectionStaticReceivers == null || ProjectionStaticReceivers.Length == 0)
            ProjectionStaticReceivers = FindShader("Custom/DiffuseWithShadow");
        setAspect();
    }

    private void setAspect()
    {
        if (videoSize)
            aspect = (float)videoSize.SourceImageWidth / videoSize.SourceImageHeight;
        else if (ProjectionTexture)
            aspect = (float)ProjectionTexture.width / ProjectionTexture.height;
        else if (camera.targetTexture)
            aspect = (float)camera.targetTexture.width / camera.targetTexture.height;
        else
            aspect = (float)Screen.width / Screen.height;
    }

    public void SetProjectionTexture(Texture2D tex)
    {
        ProjectionTexture = tex;
        setAspect();
    }

    public void ClearProjectionDynamicReceivers()
    {
        ProjectionDynamicReceivers.Clear();
    }

    public void AddProjectionDynamicReceivers(Renderer renderer)
    {
        ProjectionDynamicReceivers.Add(renderer);
    }

    private Renderer[] FindShader(string shaderName)
    {
        List<Renderer> projectionReceivers = new List<Renderer>();

        Renderer[] rendererList = SearchHelper.FindSceneObjectsOfTypeAll<Renderer>();
        foreach (Renderer rend in rendererList)
        {
            foreach (Material mat in rend.sharedMaterials)
            {
                if (mat != null && mat.shader != null && mat.shader.name != null && mat.shader.name == shaderName)
                {
                    projectionReceivers.Add(rend);
                }
            }
        }
        return projectionReceivers.ToArray();
    }

    void Update()
    {
        Matrix4x4 matProj = Matrix4x4.Perspective(camera.fieldOfView, aspect, camera.nearClipPlane, camera.farClipPlane);

        Matrix4x4 matView = Matrix4x4.identity;
        matView = Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one);

        float x = Vector3.Dot(transform.right, -transform.position);
        float y = Vector3.Dot(transform.up, -transform.position);
        float z = Vector3.Dot(transform.forward, -transform.position);

        matView.SetRow(3, new Vector4(x, y, z, 2));

        Matrix4x4 LightViewProjMatrix = matView * matProj;

        syncProjectionSettings(ProjectionStaticReceivers, LightViewProjMatrix);
        syncProjectionSettings(ProjectionDynamicReceivers.ToArray(), LightViewProjMatrix);
    }

    protected void syncProjectionSettings(Renderer[] receivers, Matrix4x4 LightViewProjMatrix)
    {
        if (receivers == null || receivers.Length <= 0)
        {
            return;
        }

        var texture = ProjectionTexture;
        if (flipProjection && ProjectionTexture is Texture2D)
            texture = ARPlaneDisplayManager.FlipTexture((Texture2D)ProjectionTexture, ARPlaneDisplayManager.flipDirection.both);

        foreach (var imageReceiver in receivers)
        {
            imageReceiver.sharedMaterial.SetTexture("_ShadowMap", texture);
            imageReceiver.sharedMaterial.SetMatrix("_LightViewProj", LightViewProjMatrix);
            imageReceiver.sharedMaterial.SetFloat("_Angle", Angle);
            imageReceiver.sharedMaterial.SetFloat("_near", camera.nearClipPlane);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position, transform.position + (transform.forward * 100.0f));
    }
}
