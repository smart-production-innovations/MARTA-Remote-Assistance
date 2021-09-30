using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// Single instance basic class for managing core features. Implements the singleton pattern for calling the manager from any core feater script.
/// </summary>
/// <typeparam name="T">Type of the final class to get right instance typecast</typeparam>
[System.Serializable]
public abstract class AManager<T> : MonoBehaviour where T : Component
{
    [SerializeField]
    private bool persistent = false;

    // use this instance
    private static T _instance;
    protected bool _destroyed = false;

    /// <summary>
    /// singleton pattern instance property
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = SearchHelper.FindSceneObjectOfType<T>();
                if (_instance == null)
                {
                    GameObject managerGameObject = new GameObject();
                    managerGameObject.transform.position = Vector3.zero;
                    managerGameObject.transform.localRotation = Quaternion.identity;
                    managerGameObject.transform.localScale = Vector3.one;
                    _instance = managerGameObject.AddComponent<T>() as T;
                    // hide gameObject in Hierarchy and don't save it to scene
                    managerGameObject.hideFlags = HideFlags.HideAndDontSave;
                    managerGameObject.name = "ManagerInstance" + typeof(T);
                    //throw new Exception("Why Manager is created? " + _instance);
                }
            }
            return _instance;
        }
    }

    private static bool hasInstanceChecked = false;
    private static bool hasInstance = true;
    /// <summary>
    /// check if there is a gameobject of the manager type in the scene hierarchy
    /// </summary>
    public static bool HasInstance
    {
        get
        {
            if (!hasInstanceChecked)
            {
                hasInstance = (_instance != null || SearchHelper.FindSceneObjectOfType<T>());
                hasInstanceChecked = true;
            }

            return hasInstance;
        }
    }

    /// <summary>
    /// check if a active manager instance exists
    /// </summary>
    public static bool InstanceExists
    {
        get { return _instance != null; }
    }

    protected virtual void Awake()
    {
        if (persistent)
        {
            GameObject.DontDestroyOnLoad(this.gameObject);
        }

        //check if only one instance of the manager exists
        if (_instance == null)
        {
            _instance = this as T;
        }
        else
        {
            if (_instance != this)
            {
                this._destroyed = true;
                Debug.Log(string.Format("Other instance of {0} detected (will be destroyed): destroy {1}, use {2}", typeof(T), this, _instance));
                GameObject.Destroy(this.gameObject);
                return;
            }
        }
    }
}