using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Single instance basic class for managing core features that listen to mouse click or touch events and implements as specific interface TI. Implements the singleton pattern for calling the manager from any core feater script.
/// </summary>
/// <typeparam name="T">Type of the final class to get right instance typecast</typeparam>
/// <typeparam name="TI">Type of the implemented interface to get right instance typecast</typeparam>
public abstract class AInterfaceClickManager<T, TI> : AClickManager<T>
    where T : Component, TI
{
    private static TI interfaceInstance;
    /// <summary>
    /// singleton pattern interface instance property
    /// </summary>
    public static TI InterfaceInstance
    {
        get
        {
            if (interfaceInstance == null)
            {
                interfaceInstance = SearchHelper.FindSceneObjectWithInterfaceOfType<TI>();

                if (interfaceInstance == null)
                    interfaceInstance = Instance;
            }
            return interfaceInstance;
        }
    }

    private static bool hasInterfaceInstanceChecked = false;
    private static bool hasInterfaceInstance = true;
    /// <summary>
    /// check if there is a game object of the manager type in the scene hierarchy
    /// </summary>
    public static bool HasInterfaceInstance
    {
        get
        {
            if (!hasInterfaceInstanceChecked)
            {
                hasInterfaceInstance = (interfaceInstance != null || SearchHelper.FindSceneObjectWithInterfaceOfType<TI>() != null);
                hasInterfaceInstanceChecked = true;
            }

            return hasInterfaceInstance;
        }
    }


    protected override void Awake()
    {
        base.Awake();

        //check if only one instance of the manager exists
        if (interfaceInstance == null)
        {
            interfaceInstance = this as T;
        }
        else
        {
            TI thisInterface = this as T;
            if (!interfaceInstance.Equals(thisInterface))
            {
                this._destroyed = true;
                Debug.LogWarning(string.Format("Other instance of {0} detected (will be destroyed): destroy {1}, use {2}", typeof(TI), this, interfaceInstance));
                GameObject.Destroy(this.gameObject);
                return;
            }
        }
    }
}
