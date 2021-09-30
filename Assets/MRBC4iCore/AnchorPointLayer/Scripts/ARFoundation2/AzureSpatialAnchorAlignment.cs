using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

#if AzureSpatialAnchors
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
#endif
#if ARFoundation2
using UnityEngine.XR.ARFoundation;
#endif

/// <summary>
/// save and load anchors based on an Azure spatial anchor
/// </summary>
public class AzureSpatialAnchorAlignment : MonoBehaviour
{
    public enum Mode
    {
        /// <summary>
        /// automatically create spatial anchor as soon as AR environment is active
        /// </summary>
        AutoSave,
        /// <summary>
        /// create spatial anchor at the first anchor in AR environment
        /// </summary>
        SaveOnFirstAnchor,
        /// <summary>
        /// load anchor points based on previously created spatial anchor
        /// </summary>
        Load,
        None,
    }

    private enum State
    {
        Initialize,
        Initialized,
        WaitForAnchor,
        AutoGenerateAnchor,
        Save,
        SavingToCloud,
        Load,
        Finished,
        Error
    }

    public Mode AnchorMode;

    public GameObject AnchorPrefab;

    public UnityEngine.UI.Text InfoTextBox;

#if ARFoundation2 && AzureSpatialAnchors
    public SpatialAnchorManager CloudManager;

    #region Private Members

    private CloudSpatialAnchorWatcher currentWatcher;
    private ReferenceDetector referenceDetector;

    private GameObject currentAnchorObject;

    private State cs = State.Initialize;
    private State currentState
    {
        get { return cs; }
        set { cs = value; Debug.Log("azure: new state: " + value); }
    }


    #endregion

    #region Unity

    private void Start()
    {
        referenceDetector = new ReferenceDetector();
        referenceDetector.Setup();

        if (CloudManager == null)
        {
            CloudManager = FindObjectOfType<SpatialAnchorManager>();
            if (CloudManager == null)
            {
                currentState = State.Error;
                return;
            }
        }

        AddCallbacks();

        currentState = State.Initialize;

        if (ARSession.state == ARSessionState.SessionTracking)
            Initialize();
        else
            ARSession.stateChanged += ARSession_stateChanged;
    }

    void Update()
    {

        switch(currentState)
        {
            case State.AutoGenerateAnchor:
                if(TryGenerateAnchor())
                {
                    currentState = State.Save;
                }
                break;
            case State.Save:
                currentState = State.SavingToCloud;
                SaveAnchor();
                break;
        }

    }


    private async void OnDestroy()
    {
        await StopSession();
        RemoveCallbacks();
        RemoveWatcher();
    }


    #endregion

    #region Private


    private void AddCallbacks()
    {
        CloudManager.Error += SpatialAnchorManager_Error;
        CloudManager.LogDebug += SpatialAnchorManager_LogDebug;
        CloudManager.AnchorLocated += SpatialAnchorManager_AnchorLocated;
    }

    private void RemoveCallbacks()
    {

        CloudManager.Error -= SpatialAnchorManager_Error;
        CloudManager.LogDebug -= SpatialAnchorManager_LogDebug;
        CloudManager.AnchorLocated -= SpatialAnchorManager_AnchorLocated;
    }

    private async Task Initialize()
    {
        if (currentState != State.Initialize)
            return;

        await CreateAndStartSession();

        currentState = State.Initialized;

        if (AnchorMode == Mode.None)
            return;

        var supportProjectMgr = SupportProjectManager.Instance;
        var anchorNames = supportProjectMgr.PreloadLastAnchors().Where(a => a.Type == AnchorPoint.AnchorType.AzureSpatialAnchor).Select(a => a.Name).ToArray();
        if(anchorNames.Length > 0 && AnchorMode != Mode.Load)
        {
            // do not create new spatial anchor, if already one exists in the project
            return;
        }


        switch (AnchorMode)
        {
            case Mode.Load:
                if (anchorNames.Length > 0)
                {
                    SetupWatcher(anchorNames);
                }
                currentState = State.Load;
                break;
            case Mode.AutoSave:
                currentState = State.AutoGenerateAnchor;
                break;
            case Mode.SaveOnFirstAnchor:
                AnchorPointManager.Instance.Added += AnchorPointAdded;
                currentState = State.WaitForAnchor;
                break;

        }
    }


    private async Task CreateAndStartSession()
    {
        // sanity test configuration
        if (!string.IsNullOrWhiteSpace(CloudManager.SpatialAnchorsAccountId) && !string.IsNullOrWhiteSpace(CloudManager.SpatialAnchorsAccountKey))
        {
            if (CloudManager.Session == null)
            {
                await CloudManager.CreateSessionAsync();
            }

            CloudManager.Session.SessionUpdated += SpatialAnchorManager_SessionUpdated;

            await CloudManager.StartSessionAsync();
        }
    }

    private async Task StopSession()
    {
        CloudManager.Session.SessionUpdated -= SpatialAnchorManager_SessionUpdated;
        CloudManager.StopSession();
        await CloudManager.ResetSessionAsync();
    }

    private void SetupWatcher(string[] anchorsToFindId)
    {
        var anchorLocateCriteria = new AnchorLocateCriteria();
        anchorLocateCriteria.Identifiers = anchorsToFindId;

        currentWatcher = CloudManager.Session.CreateWatcher(anchorLocateCriteria);
    }

    private void RemoveWatcher()
    {
        if (currentWatcher != null)
        {
            currentWatcher.Stop();
            currentWatcher = null;
        }
    }

    private void AnchorPointAdded(AnchorPoint newAnchor)
    {
        if(currentState == State.WaitForAnchor)
        {
            currentAnchorObject = newAnchor.gameObject;
            currentAnchorObject.AddComponent<CloudNativeAnchor>();
            
            GameObject.Instantiate(AnchorPrefab, newAnchor.transform);

            AnchorPointManager.Instance.Added -= AnchorPointAdded;

            currentState = State.Save;
        }

    }

    private bool TryGenerateAnchor()
    {
        var anchorPointMgr = AnchorPointManager.Instance;
        bool capturingPossible = anchorPointMgr.TryGetPose(Screen.width * 0.5f, Screen.height * 0.5f, out Pose pose);
        if (capturingPossible)
        {
            currentAnchorObject = GameObject.Instantiate(AnchorPrefab, pose.position, pose.rotation);

            currentAnchorObject.AddComponent<CloudNativeAnchor>();
            return true;
        }

        return false;
    }


    private async void SaveAnchor()
    {
        // Get the cloud-native anchor behavior
        CloudNativeAnchor cna = currentAnchorObject.GetComponent<CloudNativeAnchor>();

        // If the cloud portion of the anchor hasn't been created yet, create it
        if (cna.CloudAnchor == null) { cna.NativeToCloud(); }

        // Get the cloud portion of the anchor
        CloudSpatialAnchor cloudAnchor = cna.CloudAnchor;
        
        while (!CloudManager.IsReadyForCreate)
        {
            await Task.Delay(330);
            float createProgress = CloudManager.SessionStatus.RecommendedForCreateProgress;
            if(InfoTextBox != null)
            {
                InfoTextBox.text = $"Move your device to capture more environment data: {createProgress:0%}";
            }
        }

        if (InfoTextBox != null)
        {
            InfoTextBox.text = "";
        }

        bool success = false;

        Debug.Log("Azure: Saving spatial anchor to cloud ...");

        try
        {
            // Actually save
            await CloudManager.CreateAnchorAsync(cloudAnchor);

            // Success?
            success = cloudAnchor != null;

            if (success && currentState != State.Error)
            {
                var name = cloudAnchor.Identifier;
                referenceDetector.CreateReference(name, AnchorPoint.AnchorType.AzureSpatialAnchor, cloudAnchor.GetPose(), currentAnchorObject.transform);
                currentState = State.Finished;
            }
            else
            {
                Debug.Log("Azure: Failed to save anchor");
                currentState = State.Error;
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Azure: Failed to save anchor (" + ex.Message + ")");
            currentState = State.Error;
        }
    }





    #endregion

    #region Azure Callbacks

    private async void ARSession_stateChanged(ARSessionStateChangedEventArgs obj)
    {
        if (ARSession.state == ARSessionState.SessionTracking)
        {
            // callback can be removed, because session needs to be initialized only once
            ARSession.stateChanged -= ARSession_stateChanged;
            await Initialize();
        }
    }

    private void SpatialAnchorManager_SessionUpdated(object sender, SessionUpdatedEventArgs args)
    {
        //var status = args.Status;
        //Debug.Log("azure: session update " + status.UserFeedback + " " + status.RecommendedForCreateProgress);
    }


    private void SpatialAnchorManager_Error(object sender, SessionErrorEventArgs args)
    {
        currentState = State.Error;
        Debug.Log(args.ErrorMessage);
    }

    private void SpatialAnchorManager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
    {
        if (args.Status == LocateAnchorStatus.Located)
        {
            Debug.LogFormat("Azure Anchor located: {0}", args.Identifier);

            var cloudAnchor = args.Anchor;
            var anchorName = args.Identifier;

            UnityDispatcher.InvokeOnAppThread(() =>
            {
                var anchorPose = cloudAnchor.GetPose();

                GameObject newGameObject = GameObject.Instantiate(AnchorPrefab, anchorPose.position, anchorPose.rotation);
                var cloudNativeAnchor = newGameObject.AddComponent<CloudNativeAnchor>();

                // apply cloud anchor to the native anchor
                cloudNativeAnchor.CloudToNative(cloudAnchor);


                referenceDetector.LoadOnReferenceDetected(anchorName, AnchorPoint.AnchorType.AzureSpatialAnchor, anchorPose);


                currentState = State.Finished;
            });
        }
    }


    private void SpatialAnchorManager_LogDebug(object sender, OnLogDebugEventArgs args)
    {
        Debug.Log(args.Message);
    }


    #endregion
#else
    private void Start()
    {
    }
#endif
    }