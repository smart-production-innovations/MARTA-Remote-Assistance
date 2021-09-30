using UnityEngine;
using System.Collections;

/// <summary>
/// manages the actual implemented AR framework
/// </summary>
public class ARFrameworkManager : AManager<ARFrameworkManager>
{
    private ARExtensionHelper arLayer;
    void Start()
    {
        arLayer = ARLayers;
    }

    /// <summary>
    /// get the AR features for the implemented AR framework
    /// </summary>
    public ARExtensionHelper ARLayers
    {
        get
        {
            if (!arLayer)
            {
                arLayer = GetComponent<ARExtensionHelper>();
                if (arLayer == null)
                {
                    arLayer = new ARExtendionFactory().CreateFirst(gameObject);
                    Debug.Log("Generated default AR extension helper");
                }
            }
            return arLayer;
        }
    }
}
