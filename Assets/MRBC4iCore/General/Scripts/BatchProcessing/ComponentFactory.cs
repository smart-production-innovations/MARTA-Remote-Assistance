using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public interface ComponentList
{
    IEnumerable<string> GetAllTypes();
}

/// <summary>
/// It is possible to register multiple subclasses of a base MonoBehaviour
/// with a unique name and later create the specific component based on the name.
/// This can e.g. be used to select a specific implementation of all possibilites 
/// in the editor and create the component at runtime.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ComponentFactory<T> : ComponentList where T : MonoBehaviour
{
    private Dictionary<string, Type> registeredTypes = new Dictionary<string, Type>();
    
    /// <summary>
    /// Add an implantation of the component factory.
    /// </summary>
    /// <typeparam name="T2">Class witch implements the component.</typeparam>
    /// <param name="name">unique name of the implementation.</param>
    public void AddType<T2>(string name) where T2 : T
    {
        var type = typeof(T2);
        registeredTypes.Add(name, type);
    }

    /// <summary>
    /// get a list of all implementations
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetAllTypes()
    {
        return registeredTypes.Keys;
    }

    /// <summary>
    /// create a component from the registered types
    /// </summary>
    /// <param name="gameObject">add component to this game object</param>
    /// <param name="typeName">unique type name of the registered type</param>
    /// <param name="onlyCreateIfNotExist">create this component always or only if this component type does not exist on the game object</param>
    /// <returns></returns>
    public T CreateComponent(GameObject gameObject, string typeName, bool onlyCreateIfNotExist = true)
    {
        if (!registeredTypes.ContainsKey(typeName))
        {
            Debug.LogError("Could not create plane finder " + typeName);
            return null;
        }

        var type = registeredTypes[typeName];
        if (type != null)
        {
            if(onlyCreateIfNotExist)
            {
                var existing = gameObject.GetComponent(type) as T;
                if (existing != null)
                    return existing;
            }
            return (T)gameObject.AddComponent(type);
        }
        // nop for null-types
        return null;

    }

    /// <summary>
    /// create a component for the first registered component type
    /// </summary>
    /// <param name="gameObject">add component to this game object</param>
    /// <param name="onlyCreateIfNotExist">create this component always or only if this component type does not exist on the game object</param>
    /// <returns></returns>
    public T CreateFirst(GameObject gameObject, bool onlyCreateIfNotExist = true)
    {
        return CreateComponent(gameObject, registeredTypes.Keys.First(), onlyCreateIfNotExist);
    }
}

