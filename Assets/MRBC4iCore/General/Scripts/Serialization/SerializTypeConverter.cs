using System;
using UnityEngine;

/// <summary>
/// Data must be serialized for network transmission or storage.
/// Base class that manages the conversion of non-serializable data types into a serializable format.
/// </summary>
[Serializable]
public class SerializeTypeConverter
{
    /// <summary>
    /// Conversion back to the vecto2 data type from the serializable conversion
    /// </summary>
    /// <param name="value">serializable version of the data</param>
    /// <returns>vector2 data type for working in program code</returns>
    protected Vector2 toVector2(float[] value)
    {
        if (value != null && value.Length >= 2)
            return new Vector2(value[0], value[1]);
        return Vector2.zero;
    }

    /// <summary>
    /// Conversion back to the vecto3 data type from the serializable conversion
    /// </summary>
    /// <param name="value">serializable version of the data</param>
    /// <returns>vector3 data type for working in program code</returns>
    protected Vector3 toVector3(float[] value)
    {
        if (value != null && value.Length >= 3)
            return new Vector3(value[0], value[1], value[2]);
        return Vector3.zero;
    }

    /// <summary>
    /// conversion of the vecto2 data type into a serializable format
    /// </summary>
    /// <param name="value">vector2 data type for working in program code</param>
    /// <returns>serializable version of the data</returns>
    protected float[] toArray(Vector2 value)
    {
        return new float[2] { value.x, value.y };
    }

    /// <summary>
    /// conversion of the vecto3 data type into a serializable format
    /// </summary>
    /// <param name="value">vector3 data type for working in program code</param>
    /// <returns>serializable version of the data</returns>
    protected float[] toArray(Vector3 value)
    {
        return new float[3] { value.x, value.y, value.z };
    }
}
