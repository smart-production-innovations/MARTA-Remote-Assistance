using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Byn.Awrtc;
using UnityEngine;

/// <summary>
/// Define parameters which should be send to the client when calling SetImageToAnchor
/// </summary>
[Serializable]
public class AnnotationImageData
{
    public int AnchorId;
    public byte[] imageData;
    public float scaleFactor;
    public bool permanentSave;

    public AnnotationImageData(int id, byte[] data, float scaleFactor, bool permanentSave)
    {
        AnchorId = id;
        imageData = data;
        this.scaleFactor = scaleFactor;
        this.permanentSave = permanentSave;
    }
}

/// <summary>
/// Define the possible parameters which could be send over the data channel from the client to the expert
/// </summary>
[Serializable]
public class AnchorImageData : SerializeTypeConverter
{
    public int AnchorId;
    private float[] pivot;
    private float[] clickPoint;
    public byte[] imageData;
    public byte[] snapshot;
    public byte[] previewSnapshot;
    public byte[] previewImage;
    public List<ARLayerPlane> planList;
    private float[] cameraRotation;
    private float[] cameraPosition;
    private float[] anchorPosition;
    private float[] anchorRotation;

    /// <summary>
    /// parse the serializable float array values to a vector type
    /// </summary>
    public Vector2 Pivot
    {
        get
        {
            return toVector2(pivot);
        }
        set
        {
            pivot = toArray(value);
        }
    }

    /// <summary>
    /// checks if there is a click point set
    /// </summary>
    public bool HasClickPoint
    {
        get
        {
            if (clickPoint == null || clickPoint.Length < 2)
                return false;
            return true;
        }
    }

    /// <summary>
    /// parse the serializable float array values to a vector type
    /// </summary>
    public Vector2 ClickPoint
    {
        get
        {
            return toVector2(clickPoint);
        }
        set
        {
            clickPoint = toArray(value);
        }
    }

    /// <summary>
    /// parse the serializable float array values to a vector type
    /// </summary>
    public Vector3 CameraRotation
    {
        get
        {
            return toVector3(cameraRotation);
        }
        set
        {
            cameraRotation = toArray(value);
        }
    }

    /// <summary>
    /// parse the serializable float array values to a vector type
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
    /// parse the serializable float array values to a vector type
    /// </summary>
    public Vector3 AnchorPosition
    {
        get
        {
            return toVector3(anchorPosition);
        }
        set
        {
            anchorPosition = toArray(value);
        }
    }

    /// <summary>
    /// parse the serializable float array values to a vector type
    /// </summary>
    public Vector3 AnchorRotation
    {
        get
        {
            return toVector3(anchorRotation);
        }
        set
        {
            anchorRotation = toArray(value);
        }
    }

    /// <summary>
    /// Initialize the data required for sending over the network 
    /// </summary>
    /// <param name="id">anchor id</param>
    /// <param name="pivot">pivot point of the annotation</param>
    /// <param name="clickPoint">relative screen click point in the live video</param>
    /// <param name="imageData">annotation texture data</param>
    /// <param name="previewSnapshot">reduced snapshot texture data</param>
    /// <param name="snapshot">snapshot texture data</param>
    /// <param name="planeList">List of all detected AR planes within the camera's field of view</param>
    /// <param name="cameraTransform">position and rotation of the mobile device</param>
    /// <param name="anchorTransform">position and rotation of the anchor point</param>
    /// <param name="previewImage">reduced annotation texture data</param>
    public AnchorImageData(int id, Vector2 pivot, Vector2 clickPoint, byte[] imageData = null, byte[] previewSnapshot = null, byte[] snapshot = null, List<ARLayerPlane> planeList = null, TransformParameter cameraTransform = null, Transform anchorTransform = null, byte[] previewImage = null)
        : this(id, imageData, previewSnapshot, snapshot, planeList, cameraTransform, anchorTransform, previewImage)
    {
        Pivot = pivot;
        ClickPoint = clickPoint;
    }

    /// <summary>
    /// Initialize the data required for sending over the network 
    /// </summary>
    /// <param name="id">anchor id</param>
    /// <param name="imageData">annotation texture data</param>
    /// <param name="previewSnapshot">reduced snapshot texture data</param>
    /// <param name="snapshot">snapshot texture data</param>
    /// <param name="planeList">List of all detected AR planes within the camera's field of view</param>
    /// <param name="cameraTransform">position and rotation of the mobile device</param>
    /// <param name="anchorTransform">position and rotation of the anchor point</param>
    /// <param name="previewImage">reduced annotation texture data</param>
    public AnchorImageData(int id, byte[] imageData = null, byte[] previewSnapshot = null, byte[] snapshot = null, List<ARLayerPlane> planeList = null, TransformParameter cameraTransform = null, Transform anchorTransform = null, byte[] previewImage = null)
    {
        AnchorId = id;
        this.imageData = imageData;
        this.snapshot = snapshot;
        this.previewSnapshot = previewSnapshot;
        this.previewImage = previewImage;
        this.planList = planeList;

        if (cameraTransform != null)
        {
            this.CameraRotation = cameraTransform.Rotation;
            this.CameraPosition = cameraTransform.Position;
        }
        else
        {
            this.CameraRotation = this.CameraPosition = Vector3.zero;
        }

        if (anchorTransform)
        {
            this.AnchorPosition = anchorTransform.position;
            this.AnchorRotation = anchorTransform.eulerAngles;
        }
        else
        {
            this.AnchorPosition = this.AnchorRotation = Vector3.zero;
        }
    }
}



