using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

/// <summary>
/// static helper class for serialize and deserialize data
/// </summary>
public static class SerialiezeHelper
{
    /// <summary>
    /// convert a object to a byte array 
    /// </summary>
    /// <param name="obj">object which should be converted</param>
    /// <returns>converted byte array</returns>
    public static byte[] SerializeToByteArray(this object obj)
    {
        if (obj == null)
        {
            return null;
        }
        var bf = new BinaryFormatter();
        using (var ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    /// <summary>
    /// convert a byte array to a object
    /// </summary>
    /// <typeparam name="T">result type of the converted object</typeparam>
    /// <param name="byteArray">byte array which should be converted</param>
    /// <returns>converted object</returns>
    public static T Deserialize<T>(this byte[] byteArray) where T : class
    {
        if (byteArray == null)
        {
            return null;
        }
        using (var memStream = new MemoryStream())
        {
            var binForm = new BinaryFormatter();
            memStream.Write(byteArray, 0, byteArray.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var obj = (T)binForm.Deserialize(memStream);
            return obj;
        }
    }

    /// <summary>
    /// returns a defined area of an array.
    /// </summary>
    /// <typeparam name="T">array item type</typeparam>
    /// <param name="data">source data</param>
    /// <param name="index">start index in source array</param>
    /// <param name="length">amount of data in destination array</param>
    /// <returns></returns>
    public static T[] SubArray<T>(this T[] data, int index, int length)
    {
        T[] result = new T[length];
        Array.Copy(data, index, result, 0, length);
        return result;
    }
}

