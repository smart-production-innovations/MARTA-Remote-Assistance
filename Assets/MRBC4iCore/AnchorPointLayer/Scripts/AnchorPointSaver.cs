using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// This component automatically saves the anchor point set whenever
/// something changes.
/// </summary>
[RequireComponent(typeof(AnchorPointManager))]
public class AnchorPointSaver : MonoBehaviour
{
    public bool AutoSave = true;

    /// <summary>
    /// File name in default location, can't be changed at runtime.
    /// </summary>
    public string FileName;

    /// <summary>
    /// File path 
    /// </summary>
    public string FilePath { get; set; }

    private AnchorPointManager anchorManager;

    // internal state to avoid saving during loading
    private bool disableAutoSave;

    #region Unity

    private void Awake()
    {
        SetDefaultFilePath(FileName);
    }

    void Start()
    {
        anchorManager = GetComponent<AnchorPointManager>();

        anchorManager.Added += OnAnchorAdded;
        anchorManager.Deleted += OnAnchorDeleted;
        anchorManager.Loading += OnAnchorsLoading;
        anchorManager.Loaded += OnAnchorsLoaded;

    }

    void OnDestroy()
    {
        anchorManager.Added -= OnAnchorAdded;
        anchorManager.Deleted -= OnAnchorDeleted;
        anchorManager.Loading -= OnAnchorsLoading;
        anchorManager.Loaded -= OnAnchorsLoaded;
        
    }

    #endregion



    #region Public

    /// <summary>
    /// Event is fired after file has been saved.
    /// Parameter contains path of saved file
    /// </summary>
    public event Action<string> Saved;

    public void SetDefaultFilePath(string filename)
    {
        FilePath = Path.Combine(Application.persistentDataPath, filename);
    }

    #endregion



    #region Anchor Point Events

    private void OnAnchorsLoading(IEnumerable<AnchorPoint> anchors)
    {
        disableAutoSave = true;
    }

    private void OnAnchorsLoaded(IEnumerable<AnchorPoint> anchors)
    {
        disableAutoSave = false;
        if(AutoSave)
            Save();
    }

    private void OnAnchorAdded(AnchorPoint newAnchor)
    {
        if (AutoSave)
            Save();
    }

    private void OnAnchorDeleted(int anchorId)
    {
        if (AutoSave)
            Save();
    }

    #endregion



    #region Private
    public void Save()
    {
        if (this.enabled && !disableAutoSave && anchorManager != null)
        {
            anchorManager.SaveAnchorPoints(FilePath);
            Saved?.Invoke(FilePath);
        }
    }

    #endregion
}

