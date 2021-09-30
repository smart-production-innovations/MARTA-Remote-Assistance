using UnityEngine;

/// <summary>
/// This game object is used for initializing the anchor point framework
/// in the editor. It is not necessary at runtime.
/// </summary>
public class AnchorPointInitializer : MonoBehaviour
{
    public enum ARFramework
    {
        None,
        ARFoundation,
        ARFoundation2,
        ARCore,
        Vuforia,
        ARFoundation3
    }

    public enum StorageLocation
    {
        None,
        LocalFile
    }

    public enum SaveAction
    {
        Continuous,
        Manually
    }


    [SerializeField]
    private ARFramework framework;

    [SerializeField]
    private GameObject arFoundationBase;

    [SerializeField]
    private GameObject vuforiaBase;

    [SerializeField]
    private string planeFinderType;

    [SerializeField]
    private string screenPoseConverterType;

    [SerializeField]
    private bool enableInteraction;

    [SerializeField]
    private GameObject contextMenu;

    [SerializeField]
    private StorageLocation storage;

    /// <summary>
    /// This object defines the null point of the anchor point manager, e.g. an image target
    /// It is used by the auto-saver and loader scripts and the type varies based on the AR framework /// </summary>
    [SerializeField]
    private UnityEngine.Object loaderObject;

    [SerializeField]
    private bool autoSave;

    [SerializeField]
    private bool autoLoad;


}

