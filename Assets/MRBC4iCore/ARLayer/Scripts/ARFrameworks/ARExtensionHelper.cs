using UnityEngine;
using System.Collections;
using Unity.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Saves the data of the AR planes found by the AR algorithm. This data can be serialized for permanent storage in a data memory or for network transmission.
/// </summary>
[Serializable]
public class ARLayerPlane : SerializeTypeConverter
{
    private float[] center;
    private float[] position;
    private float[] rotation;
    private float[] scale;
    private float[][] boundary;
    private float[] cameraPosition;
    private float[] cameraRotaton;
    public float CameraNearPlane;
    public float CameraFarePlane;
    public float CameraFOV;

    /// <summary>
    /// center of the AR plane
    /// </summary>
    public Vector3 Center
    {
        get
        {
            return toVector3(center);
        }
        set
        {
            center = toArray(value);
        }
    }

    /// <summary>
    /// position of the AR plane
    /// </summary>
    public Vector3 Position
    {
        get
        {
            return toVector3(position);
        }
        set
        {
            position = toArray(value);
        }
    }

    /// <summary>
    /// Euler rotation of the AR plane
    /// </summary>
    public Vector3 EulerRotation
    {
        get
        {
            return toVector3(rotation);
        }
        set
        {
            rotation = toArray(value);
        }
    }

    /// <summary>
    /// Rotation of the AR plane
    /// </summary>
    public Quaternion Rotation
    {
        get
        {
            return Quaternion.Euler(toVector3(rotation));
        }
        set
        {
            rotation = toArray(value.eulerAngles);
        }
    }

    /// <summary>
    /// Size of the AR plane
    /// </summary>
    public Vector3 Scale
    {
        get
        {
            return toVector3(scale);
        }
        set
        {
            scale = toArray(value);
        }
    }

    /// <summary>
    /// position of the AR camera
    /// </summary>
    public Vector3 CameraPosition
    {
        get
        {
            return toVector3(cameraPosition);
        }
        set
        {
            cameraPosition = toArray(value);
        }
    }

    /// <summary>
    /// rotation of the AR camera
    /// </summary>
    public Vector3 CameraRotaton
    {
        get
        {
            return toVector3(cameraRotaton);
        }
        set
        {
            cameraRotaton = toArray(value);
        }
    }

    /// <summary>
    /// shape of the AR plane
    /// </summary>
    public Vector2[] Boundary
    {
        get
        {
            var list = new List<Vector2>();
            if (boundary != null)
            {
                
                foreach (var item in boundary)
                {
                    if (item.Length >= 2)
                        list.Add(new Vector2(item[0], item[1]));
                }
            }
            return list.ToArray();
        }
        set
        {
            if (value != null)
            {
                boundary = new float[value.Length][];
                for (int i = 0; i < value.Length; i++)
                {
                    boundary[i] = new float[] { value[i].x, value[i].y };
                }
            }
        }
    }

    /// <summary>
    /// mesh of the AR plane
    /// </summary>
    public virtual Mesh DisplayMesh
    {
        get
        {
            var mesh = new Mesh();
            ARPlaneMeshGenerators.GenerateMesh(mesh, new Pose(), Boundary);
            return mesh;
        }
    }

    /// <summary>
    /// constructor to create a new AR plane
    /// </summary>
    /// <param name="center">center of the AR plane</param>
    /// <param name="transform">position and rotation of the AR plane</param>
    /// <param name="boundary">shape of the AR plane</param>
    /// <param name="camera">position and rotation of the AR camera</param>
    public ARLayerPlane(Vector3 center, Transform transform, Vector2[] boundary, Camera camera)
    {
        this.Center = center;
        this.Position = transform.position;
        this.EulerRotation = transform.eulerAngles;
        this.Scale = transform.localScale;
        this.Boundary = boundary;

        this.CameraPosition = camera.transform.position;
        this.CameraRotaton = camera.transform.eulerAngles;
        this.CameraNearPlane = camera.nearClipPlane;
        this.CameraFarePlane = camera.farClipPlane;
        this.CameraFOV = camera.fieldOfView;
    }
}

/// <summary>
/// Prepares AR features for access by other scripts
/// </summary>
public class ARExtensionHelper : MonoBehaviour
{
    public virtual void Awake()
    {
    }

    /// <summary>
    /// List of all AR planes detected by the AR algorithm
    /// </summary>
    /// <returns></returns>
    public virtual List<ARLayerPlane> GetAllPlanes()
    {
        var list = new List<ARLayerPlane>();
        return list;
    }
}
