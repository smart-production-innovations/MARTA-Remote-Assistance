using UnityEngine;
using System.Collections;
using System.IO;

public class FileHelper
{
    public static string GetDirectoryPath(string filename)
    {
        // load email text template
        var path = Path.Combine(Application.persistentDataPath, filename);
        if (!Directory.Exists(path))
        {
#if UNITY_ANDROID
            Directory.CreateDirectory(path);
#else
                path = Path.Combine(Application.streamingAssetsPath, filename);
#endif
        }
        return path;
    }


    public static string GetFilePath (string filename)
    {
        // load email text template
        var path = Path.Combine(Application.persistentDataPath, filename);
        if (!File.Exists(path))
        {
#if UNITY_ANDROID
            string androidPath = "jar:file://" + Application.dataPath + "!/assets/" + filename;
            WWW wwwfile = new WWW(androidPath);
            while (!wwwfile.isDone) { }
            var bytes = wwwfile.bytes;
            if (bytes.Length == 0)
            {
                var tempPath = Path.Combine(Application.streamingAssetsPath, filename);
                bytes = File.ReadAllBytes(tempPath);
            }
            File.WriteAllBytes(path, bytes);
#else
            path = Path.Combine(Application.streamingAssetsPath, filename);
#endif
        }
        return path;
    }

    public static string ReadTextFile (string filename)
    {
        var path = GetFilePath(filename);
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }
        return null;
    }
}
