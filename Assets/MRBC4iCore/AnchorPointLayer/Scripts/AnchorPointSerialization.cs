using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable]
internal class SerializableAnchorPoint
{
    public int Id;
    public string Name;
    public Pose Pose;
    public AnchorPoint.AnchorType Type;


    public SerializableAnchorPoint(AnchorPoint src)
    {
        Id = src.Id;
        Name = src.AnchorName;
        Pose = new Pose(src.transform.localPosition, src.transform.localRotation);
        Type = src.Type;
    }

    public void ApplyData(AnchorPoint dst)
    {
        dst.Id = Id;
        dst.AnchorName = Name;
        dst.transform.localPosition = Pose.position;
        dst.transform.localRotation = Pose.rotation;
        dst.Type = Type;
    }
}

[Serializable]
internal class SerializableAnchorPointList
{
    public Pose NullPoint;
    public SerializableAnchorPoint[] Points;
}

/// <summary>
/// Serialization of anchor points to json
/// </summary>
internal class AnchorPointSerialization
{

    /// <summary>
    /// Serialize a nullpoint and a set of anchor points to a json-string
    /// </summary>
    public static string Serialize(Pose nullpoint, IEnumerable<AnchorPoint> anchorObjects)
    {
        var serializableList = new SerializableAnchorPointList()
        {
            NullPoint = nullpoint,
            Points = anchorObjects.Select(ap => new SerializableAnchorPoint(ap)).ToArray()
        };

        return JsonUtility.ToJson(serializableList);
    }

    /// <summary>
    /// Deserialize a json-string to a nullpoint-pose and a set of anchor points.
    /// You can use the method SerializableAnchorPoint.ApplyData to apply the data
    /// to AnchorPoints.
    /// </summary>
    public static IEnumerable<SerializableAnchorPoint> Deserialize(string json, out Pose nullpoint)
    {
        var serializableList = JsonUtility.FromJson<SerializableAnchorPointList>(json);
        nullpoint = serializableList.NullPoint;
        return serializableList.Points;

    }

}




