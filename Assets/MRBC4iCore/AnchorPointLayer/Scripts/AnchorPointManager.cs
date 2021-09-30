using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


/// <summary>
/// This manager handles a set of anchor points.
/// It is possible to select, add, remove and move anchor points and iterate over them.
/// For adding anchor points with a screen position, add an appropriate ScreenPoseConverter
/// to the AnchorPointManager-gameobject.
/// For aligning anchor points with an AR plane, add an appropriate ARPlaneFinder to the
/// AnchorPointManager-gameobject.
/// </summary>
public class AnchorPointManager : AManager<AnchorPointManager>, IEnumerable<AnchorPoint>
{
    /// <summary>
    /// Event is fired after an anchor point has been added.
    /// </summary>
    public event Action<AnchorPoint> Added;
    /// <summary>
    /// Event is fired before an anchor point is deleted. Use this
    /// event if you need anything from the anchor point or its children.
    /// </summary>
    public event Action<AnchorPoint> Deleting;
    /// <summary>
    /// Event is fired after an anchor point has been deleted. 
    /// </summary>
    public event Action<int> Deleted;
    /// <summary>
    /// Event is fired before anchor points are loaded. Use this
    /// event if you need anything from the old anchor points or their children.
    /// </summary>
    public event Action<IEnumerable<AnchorPoint>> Loading;
    /// <summary>
    /// Event is fired after anchor points have been loaded.
    /// List contains only newly loaded anchor points.
    /// </summary>
    public event Action<IEnumerable<AnchorPoint>> Loaded;
    /// <summary>
    /// Event is fired after anchor points have been saved
    /// </summary>
    public event Action Saved;


    private List<AnchorPoint> anchorObjects = new List<AnchorPoint>();
    private AnchorPoint selectedAnchor = null;

    private ScreenPoseConverter screenPoseConverter;
    private ARPlaneFinder planeFinder;
    private AnchorCreator anchorCreator;

    private int nextIndex = 0;


    public enum Alignment
    {
        Camera,
        Plane
    }


    #region Unity Loop


    void Start()
    {
        screenPoseConverter = GetComponent<ScreenPoseConverter>();
        if (screenPoseConverter == null)
        {
            screenPoseConverter = new ScreenPoseConverterFactory().CreateFirst(gameObject);
            Debug.Log("Generated default screen-pose converter");
        }

        planeFinder = GetComponent<ARPlaneFinder>();
        if (planeFinder == null)
        {
            planeFinder = new ARPlaneFinderFactory().CreateFirst(gameObject);
            Debug.Log("Generated default plane finder");
        }

        anchorCreator = GetComponent<AnchorCreator>();
        if(anchorCreator == null)
        {
            anchorCreator = new AnchorCreatorFactory().CreateFirst(gameObject);
        }
    }


    #endregion

    #region Select
    /// <summary>
    /// Get or set the selected anchor by its id
    /// </summary>
    public int SelectedId
    {
        get
        {
            if (selectedAnchor == null)
                return -1;
            return selectedAnchor.Id;
        }

        set
        {
            SelectedAnchor = GetAnchorPoint(value);
        }
    }

    /// <summary>
    /// Get or set the selected anchor. The anchor needs
    /// to be part of the anchor point manager.
    /// </summary>
    public AnchorPoint SelectedAnchor
    {
        get
        {
            return selectedAnchor;
        }

        set
        {
            if (selectedAnchor != null)
            {
                if (selectedAnchor == value)
                    return;

                selectedAnchor.IsSelected = false;
            }
            if (anchorObjects.Contains(value))
            {
                selectedAnchor = value;
                selectedAnchor.IsSelected = true;
            }
            else
            {
                selectedAnchor = null;
            }

        }
    }
    #endregion

    #region Get
    /// <summary>
    /// Test if an anchor point with the given index exists
    /// </summary>
    public bool ContainsAnchorPoint(int id)
    {
        return anchorObjects.Any(a => a.Id == id);
    }

    /// <summary>
    /// Get anchor point by its id
    /// </summary>
    public AnchorPoint GetAnchorPoint(int id, int relativIndex = 0)
    {
        if (relativIndex == 0)
        {
            var anchor = anchorObjects.FirstOrDefault(a => a.Id == id);
            return anchor;
        }
        else
        {
            var displayIndex = GetAnchorPointIndex(id);
            if (displayIndex >= 0)
            {
                displayIndex += relativIndex;
                if (displayIndex >= AnchorPointManager.Instance.GeAnchorCount()) displayIndex = 0;
                if (displayIndex < 0) displayIndex = AnchorPointManager.Instance.GeAnchorCount() - 1;
                return GetAnchorPointOfIndex(displayIndex);
            }
            return null;
        }
    }

    /// <summary>
    /// Get the most recently added anchor point
    /// </summary>
    /// <returns></returns>
    public AnchorPoint GetLastAnchorPoint()
    {
        if (anchorObjects.Count > 0)
            return anchorObjects[anchorObjects.Count - 1];
        return null;
    }

    /// <summary>
    /// Get all anchor points
    /// </summary>
    public IEnumerable<AnchorPoint> GetAllAnchorPoints()
    {
        return anchorObjects;
    }


    /// <summary>
    /// Get anchor count
    /// </summary>
    public int GeAnchorCount()
    {
        return anchorObjects.Count;
    }


    /// <summary>
    /// Get anchor point by its index
    /// </summary>
    public AnchorPoint GetAnchorPointOfIndex(int index)
    {
        if (index >= 0 && index < anchorObjects.Count)
            return anchorObjects[index];

        return null;
    }

    /// <summary>
    /// Get index of an anchor point
    /// </summary>
    /// <param name="id">id of the anchor point</param>
    /// <returns>index of the anchor point</returns>
    public int GetAnchorPointIndex(int id)
    {
        var anchor = anchorObjects.Select((item, index) => new { Index = index, Value = item }).FirstOrDefault(a => a.Value.Id == id);
        if (anchor != null)
        {
            var displayIndex = anchor.Index;
            return displayIndex;
        }
        return -1;
    }
    #endregion

    #region Add / Remove

    /// <summary>
    /// Add a new anchor point at the given 3D pose.
    /// A new gameobject with an AnchorPoint-component is created as a child 
    /// of the AnchorPointManager.
    /// </summary>
    public AnchorPoint AddAnchorPoint(Pose pose)
    {
        return AddAnchorPoint(pose.position, pose.rotation);
    }

    /// <summary>
    /// Add a new anchor point at the given 3D pose and a pose driver
    /// A new gameobject with an AnchorPoint-component is created as a child 
    /// of the AnchorPointManager.
    /// </summary>
    public AnchorPoint AddAnchorPoint(Vector3 position, Quaternion rotation, Transform poseDriver = null, bool setPoseDriver = true)
    {
        var id = GetNextId();

        var newAnchor = CreateGameObject();
        newAnchor.transform.position = position;
        newAnchor.transform.rotation = rotation;
        newAnchor.Id = id;
        newAnchor.Type = AnchorPoint.AnchorType.Standard;

        if (setPoseDriver)
        {
            if (poseDriver == null)
            {
                AddReferencePointForAnchor(newAnchor, position, rotation);
            }
            else
                newAnchor.PoseDriver = poseDriver;
        }

        anchorObjects.Add(newAnchor);
        SelectedAnchor = newAnchor;

        Added?.Invoke(newAnchor);

        return newAnchor;
    }

    public Transform CreateReferencePoint(Vector3 position, Quaternion rotation)
    {
        return anchorCreator.CreatePoseDriver(new Pose(position, rotation));
    }

    public void AddReferencePointForAnchor(AnchorPoint anchor, Vector3 position, Quaternion rotation)
    {
        anchor.PoseDriver = CreateReferencePoint(position, rotation);
    }

    public void ChangeReferencePointForAnchor(AnchorPoint anchor, Vector3 position, Quaternion rotation)
    {
        anchor.PoseDriver = anchorCreator.ReplacePoseDriver(new Pose(position, rotation), anchor.PoseDriver);
    }

    /// <summary>
    /// Add a new anchor point at the screen position if it can be converted to a 3D pose.
    /// The screen-pose-converter, which needs to be part of the game object, is used 
    /// for transforming the  2d screen position to a 3D pose.
    /// </summary>
    /// <returns>The return value might be null if no 3D pose can be computed</returns>
    public AnchorPoint AddAnchorPoint(float screenPosX, float screenPosY)
    {
        Transform plane;
        return AddAnchorPoint(screenPosX, screenPosY, out plane);
    }

    /// <summary>
    /// Add a new anchor point at the screen position if it can be converted to a 3D pose.
    /// The screen-pose-converter, which needs to be part of the game object, is used 
    /// for transforming the  2d screen position to a 3D pose.
    /// </summary>
    /// <returns>The return value might be null if no 3D pose can be computed</returns>
    public AnchorPoint AddAnchorPoint(float screenPosX, float screenPosY, out Transform plane)
    {
        Pose pose;
        if (TryGetPose(screenPosX, screenPosY, out pose, out plane))
        {
            return AddAnchorPoint(pose);
        }
        return null;
    }

    /// <summary>
    /// Try to convert a screen position into a 3D pose
    /// </summary>
    /// <param name="screenPosX">Screen position in pixels</param>
    /// <param name="screenPosY">Screen position in pixels</param>
    /// <param name="pose">Return value, 3D pose</param>
    /// <returns>Returns true, if position can be coverted.</returns>
    public bool TryGetPose(float screenPosX, float screenPosY, out Pose pose)
    {
        return screenPoseConverter.TryGetPose(screenPosX, screenPosY, out pose, out Transform plane);
    }

    /// <summary>
    /// Try to convert a screen position into a 3D pose
    /// </summary>
    public bool TryGetPose(float screenPosX, float screenPosY, out Pose pose, out Transform plane)
    {
        return screenPoseConverter.TryGetPose(screenPosX, screenPosY, out pose, out plane);
    }

    /// <summary>
    /// Try to convert a screen position into a 3D pose
    /// </summary>
    public bool TryGetPoseAndDistance(float screenPosX, float screenPosY, out Pose pose, out Transform plane, out float distance)
    {
        return screenPoseConverter.TryGetPoseAndDistance(screenPosX, screenPosY, out pose, out plane, out distance);
    }

    /// <summary>
    /// get next free anchor id
    /// </summary>
    /// <returns></returns>
    private int GetNextId()
    {
        while(ContainsAnchorPoint(nextIndex))
        {
            nextIndex++;
        }
        var newId = nextIndex;
        nextIndex++;
        return newId;
    }

    /// <summary>
    /// create a empty anchor point gameobject
    /// </summary>
    /// <returns>new anchor point gameobject</returns>
    private AnchorPoint CreateGameObject()
    {
        var anchor = new GameObject("Anchor").AddComponent<AnchorPoint>();
        anchor.transform.parent = this.transform;
        return anchor;
    }

    /// <summary>
    /// Delete the anchor point with the given id. The game object and all its children
    /// are deleted as well.
    /// </summary>
    public void RemoveAnchorPoint(int id)
    {
        RemoveAnchorPoint(GetAnchorPoint(id));
    }


    /// <summary>
    /// Delete the given anchor point. The game object and all its children
    /// are deleted as well.
    /// </summary>
    public void RemoveAnchorPoint(AnchorPoint anchorObject)
    {
        if (anchorObject != null)
        {
            int id = anchorObject.Id;

            Deleting?.Invoke(anchorObject);

            if (SelectedAnchor == anchorObject)
                SelectedAnchor = null;


            anchorObjects.Remove(anchorObject);
            GameObject.Destroy(anchorObject.gameObject);

            Deleted?.Invoke(id);
        }
    }

    /// <summary>
    /// Delete the most recently added anchor point. The game object and
    /// all its children are deleted as well.
    /// </summary>
    public void RemoveLastAnchorPoint()
    {
        if (anchorObjects.Count > 0)
            RemoveAnchorPoint(anchorObjects[anchorObjects.Count - 1]);
    }

    /// <summary>
    /// delete all anchor points
    /// </summary>
    public void RemoveAllAnchorPoints()
    {
        while (anchorObjects.Count > 0)
            RemoveLastAnchorPoint();
    }

    #endregion


    #region Move / Align

    /// <summary>
    /// Move the anchor point with the given id to the new screen position.
    /// This function needs a screen-pose-converter in the game object to convert
    /// the 2d position to a 3D pose. If no 3D pose can be computed, nothing is changed.
    /// </summary>
    /// <param name="screenPosX">New horizontal screen position</param>
    /// <param name="screenPosY">New vertical screen position</param>
    public void MoveAnchorPoint(int id, float screenPosX, float screenPosY)
    {
        MoveAnchorPoint(GetAnchorPoint(id), screenPosX, screenPosY);
    }

    /// <summary>
    /// Move the anchor point with the given id to the new 3D pose
    /// </summary>
    public void MoveAnchorPoint(int id, Pose newPose)
    {
        MoveAnchorPoint(GetAnchorPoint(id), newPose);
    }

    /// <summary>
    /// Move the given anchor point to the new screen position.
    /// This function needs a screen-pose-converter in the game object to convert
    /// the 2d position to a 3D pose. If no 3D pose can be computed, nothing is changed.
    /// </summary>
    /// <param name="screenPosX">New horizontal screen position</param>
    /// <param name="screenPosY">New vertical screen position</param>
    public void MoveAnchorPoint(AnchorPoint anchorObject, float screenPosX, float screenPosY)
    {
        Pose pose;
        Transform plane;
        if (TryGetPose(screenPosX, screenPosY, out pose, out plane))
        {
            MoveAnchorPoint(anchorObject, pose);
        }
    }

    /// <summary>
    /// Move the given anchor point to the new 3D pose
    /// </summary>
    public void MoveAnchorPoint(AnchorPoint anchorObject, Pose newPose)
    {
        if (anchorObject != null)
        {
            anchorObject.transform.position = newPose.position;
            anchorObject.transform.rotation = newPose.rotation;
        }
    }

    /// <summary>
    /// Move the most recently added anchor point to the new screen position.
    /// This function needs a screen-pose-converter in the game object to convert
    /// the 2d position to a 3D pose. If no 3D pose can be computed, nothing is changed.
    /// </summary>
    /// <param name="screenPosX">New horizontal screen position</param>
    /// <param name="screenPosY">New vertical screen position</param>
    public void MoveLastAnchorPoint(float screenPosX, float screenPosY)
    {
        if (anchorObjects.Count > 0)
            MoveAnchorPoint(anchorObjects[anchorObjects.Count - 1], screenPosX, screenPosY);
    }

    /// <summary>
    /// Move the most recently added anchor point to the new 3D pose
    /// </summary>
    public void MoveLastAnchorPoint(Pose newPose)
    {
        if (anchorObjects.Count > 0)
            MoveAnchorPoint(anchorObjects[anchorObjects.Count - 1], newPose);
    }


    /// <summary>
    /// Align anchor point with given id to the main camera or an AR plane.
    /// The AR-plane is automatically detected by the ARPlaneFinder in the game object.
    /// If alignment is set to plane and no AR-plane is available, nothing is changed. 
    /// </summary>
    public void AlignAnchor(int index, Alignment alignment)
    {
        AlignAnchor(GetAnchorPoint(index), alignment);
    }

    /// <summary>
    /// Align given anchor point to the main camera or an AR plane.
    /// The AR-plane is automatically detected by the ARPlaneFinder in the game object.
    /// If alignment is set to plane and no AR-plane is available, nothing is changed. 
    /// </summary>
    public void AlignAnchor(AnchorPoint anchorObject, Alignment alignment)
    {
        if (anchorObject == null)
            return;

        switch (alignment)
        {
            case Alignment.Camera:
                anchorObject.transform.rotation = CameraHelper.ARCamera.transform.rotation;
                break;
            case Alignment.Plane:
                Pose planePose;
                if (planeFinder.TryGetPlanePose(out planePose))
                {
                    AlignAnchorToPlane(anchorObject, planePose);
                }
                break;

        }
    }


    /// <summary>
    /// Align the most recently added anchor point to the main camera or an AR plane.
    /// The AR-plane is automatically detected by the ARPlaneFinder in the game object.
    /// If alignment is set to plane and no AR-plane is available, nothing is changed. 
    /// </summary>
    public void AlignLastAnchor(Alignment alignment)
    {
        if (anchorObjects.Count > 0)
            AlignAnchor(anchorObjects[anchorObjects.Count - 1], alignment);
    }


    /// <summary>
    /// Align anchor point with given id to an arbitrary plane pose.
    /// </summary>
    public void AlignAnchorToPlane(int index, Pose planePose)
    {
        AlignAnchorToPlane(GetAnchorPoint(index), planePose);
    }

    /// <summary>
    /// Align the given anchor point to an arbitrary plane pose.
    /// </summary>
    public void AlignAnchorToPlane(AnchorPoint anchorObject, Pose planePose)
    {
        if (anchorObject != null)
        {
            var lookat = planePose.position - planePose.up * 100;
            anchorObject.transform.LookAt(lookat, CameraHelper.MainCamera.transform.up);
        }
    }

    /// <summary>
    /// Align the most recently added anchor point to an arbitrary plane pose.
    /// </summary>
    public void AlignLastAnchorToPlane(Pose planePose)
    {
        if (anchorObjects.Count > 0)
            AlignAnchorToPlane(anchorObjects[anchorObjects.Count - 1], planePose);
    }
    #endregion


    #region NullPoint / Load / Save
    /// <summary>
    /// get origin position of the anchor point system
    /// </summary>
    /// <returns></returns>
    public Pose GetNullPoint()
    {
        return new Pose(this.transform.localPosition, this.transform.localRotation);
    }

    /// <summary>
    /// Define a new nullpoint for all anchor points. The nullpoint defines the origin of the 
    /// AR space. Each anchor point is defined in relation to the nullpoint.
    /// </summary>
    /// <param name="newNullPoint">Pose of the null point. This can be e.g. a marker or another 3D
    /// trackable, which is used for defining the origin of the AR space</param>
    /// <param name="keepGlobalPosition">If true, the global positions of all anchor points stays
    /// the same. If false, all anchor points are moved from the old nullpoint to the new nullpoint.</param>
    public void SetNullPoint(Pose newNullPoint, bool keepGlobalPosition)
    {
        // Store global positions of all anchor points
        var poseLookup = new Dictionary<int, Pose>();
        if (keepGlobalPosition)
        {
            foreach (var anchor in anchorObjects)
            {
                poseLookup.Add(anchor.Id, new Pose(anchor.transform.position, anchor.transform.rotation));
            }
        }

        // set new null point
        this.transform.localPosition = newNullPoint.position;
        this.transform.localRotation = newNullPoint.rotation;

        // reset global positions
        if (keepGlobalPosition)
        {
            foreach (var anchor in anchorObjects)
            {
                var pose = poseLookup[anchor.Id];
                anchor.transform.position = pose.position;
                anchor.transform.rotation = pose.rotation;
            }
        }
    }


    public void TransformAllAnchors(Pose relativeTransform)
    {
        foreach(var anchor in anchorObjects)
        {
            var anchorPose = new Pose(anchor.transform.position, anchor.transform.rotation);
            anchorPose = anchorPose.GetTransformedBy(relativeTransform);
            anchor.transform.position = anchorPose.position;
            anchor.transform.rotation = anchorPose.rotation;
        }
    }



    /// <summary>
    /// Save all anchor points and the null point to a file.
    /// Please notice, that no content of the anchor points, i.e. child nodes, are
    /// stored in the file. You have to manually save these data and reassign it after 
    /// loading anchor points.
    /// </summary>
    /// <param name="filePath">Path to a json file</param>
    public void SaveAnchorPoints(string filePath)
    {
        var nullPoint = GetNullPoint();
        var jsonText = AnchorPointSerialization.Serialize(nullPoint, anchorObjects);
        File.WriteAllText(filePath, jsonText);
        Debug.Log("Saved anchor points to " + filePath);
        Saved?.Invoke();
    }


    /// <summary>
    /// Load data from file, but do not add loaded anchor points
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    internal static IEnumerable<SerializableAnchorPoint> PreLoadAnchorPoints(string filePath)
    {
        if(!File.Exists(filePath))
        {
            return Enumerable.Empty<SerializableAnchorPoint>();
        }

        var jsonText = File.ReadAllText(filePath);
        Pose nullPoint;
        var anchors = AnchorPointSerialization.Deserialize(jsonText, out nullPoint);
        return anchors;
    }

    public bool LoadAnchorPoints(string filePath, bool additive = false)
    {
        return LoadAnchorPoints(filePath, -1, Pose.identity, additive);
    }


    public bool LoadAnchorPoints(string filePath, int referenceAnchorId, Pose currentPoseOfReferenceAnchor, bool additive = false)
    {
        if (!File.Exists(filePath))
        {
            Debug.Log("Can't load anchor points because file does not exist. " + filePath);
            return false;
        }

        Debug.Log("Load anchor points from " + filePath);

        Loading?.Invoke(anchorObjects);

        var newAnchorObjects = new List<AnchorPoint>();

        if (!additive)
        {
            // delete existing data
            RemoveAllAnchorPoints();
        }

        // load all anchor points from json-file
        var jsonText = File.ReadAllText(filePath);
        Pose nullPoint;
        var newAnchors = AnchorPointSerialization.Deserialize(jsonText, out nullPoint);


        // find transformation between old coordinate system and current coordinate system
        // based on reference anchor
        Pose referenceTrafo = Pose.identity;
        if(referenceAnchorId >= 0)
        {
            foreach (var apData in newAnchors)
            {
                if (apData.Id == referenceAnchorId)
                {
                    var newPose = currentPoseOfReferenceAnchor;
                    var oldPose = apData.Pose;
                    var oldPoseInverse = oldPose.Inverse();
                    referenceTrafo = oldPoseInverse.GetTransformedBy(newPose);
                    break;
                }
            }
        }

        // create game objects for anchors and transform them to new coordinate system
        // if necessary, assign new anchor id
        foreach (var apData in newAnchors)
        {
            var newGameObject = CreateGameObject();
            apData.ApplyData(newGameObject);

            // transform based on relative transformation between old and new pose of reference anchor
            var pose = new Pose(newGameObject.transform.localPosition, newGameObject.transform.localRotation);
            var transformedPose = pose.GetTransformedBy(referenceTrafo);
            newGameObject.transform.localRotation = transformedPose.rotation;
            newGameObject.transform.localPosition = transformedPose.position;


            // assign new index if already in use
            // keep original id, in case an application has wired something to the anchor id
            newGameObject.OriginalId = newGameObject.Id;
            if (ContainsAnchorPoint(newGameObject.Id))
            {
                Debug.Log("Assign new index " + nextIndex + " instead of " + newGameObject.Id);
                newGameObject.Id = GetNextId();
            }

            anchorObjects.Add(newGameObject);
            newAnchorObjects.Add(newGameObject);
        }

        Loaded?.Invoke(newAnchorObjects);
        return true;
    }

    #endregion

    #region IEnumerable
    /// <summary>
    /// gets the enumerator of the anchor point list
    /// </summary>
    public IEnumerator<AnchorPoint> GetEnumerator()
    {
        return ((IEnumerable<AnchorPoint>)anchorObjects).GetEnumerator();
    }

    /// <summary>
    /// gets the enumerator of the anchor point list
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<AnchorPoint>)anchorObjects).GetEnumerator();
    }

    #endregion



}
