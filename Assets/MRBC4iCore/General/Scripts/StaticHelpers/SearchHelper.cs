using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// static class that helps to find scene objects of a specific type
/// </summary>
public static class SearchHelper
{
    /// <summary>
    /// find a single gameobject of the given type T in the scene
    /// </summary>
    /// <typeparam name="T">type of the gameobject</typeparam>
    /// <returns>gameobject</returns>
    public static T FindSceneObjectOfType<T>() where T : Component
    {
        return FindSceneObjectOfType<T>(typeof(T));
    }


    /// <summary>
    /// find a single gameobject of the given type or inherited types in the scene
    /// </summary>
    /// <typeparam name="T">return type of the gamebject</typeparam>
    /// <param name="type">type or basic type of the gameobject</param>
    /// <returns>gameobject</returns>
    private static T FindSceneObjectOfType<T>(Type type) where T : Component
    {
        var obj = (T)FindSceneObjectsOfTypeAll(type).FirstOrDefault();
        if (obj) return obj;

        foreach (Type subtype in Assembly.GetAssembly(type).GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(type)))
        {
            obj = FindSceneObjectOfType<T>(subtype);
            if (obj) return obj;
        }

        return null;
    }

    /// <summary>
    /// find a single gameobject which implements a specific interface in the scene
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T FindSceneObjectWithInterfaceOfType<T>()
    {
        return FindSceneObjectsOfTypeAll<MonoBehaviour>().OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// find all gameobjects of the given type T in the scene
    /// </summary>
    /// <typeparam name="T">type of the gameobject</typeparam>
    /// <returns>list of gameobjects</returns>
    public static T[] FindSceneObjectsOfTypeAll<T>(bool includeInactive = true) where T : Component
    {
        return FindSceneObjectsOfTypeAll(typeof(T), includeInactive).Select(x => (T)x).ToArray();
    }

    /// <summary>
    /// find all gameobjects of the given type in the scene
    /// </summary>
    /// <param name="type">type of the gameobject</param>
    /// <returns>list of gameobjects</returns>
    private static UnityEngine.Object[] FindSceneObjectsOfTypeAll(Type type, bool includeInactive = true)
    {
        List<UnityEngine.Object> results = new List<UnityEngine.Object>();
        SceneManager.GetActiveScene().GetRootGameObjects().ToList().ForEach(g => results.AddRange(g.GetComponentsInChildren(type, includeInactive)));
        return results.ToArray();
    }
}
