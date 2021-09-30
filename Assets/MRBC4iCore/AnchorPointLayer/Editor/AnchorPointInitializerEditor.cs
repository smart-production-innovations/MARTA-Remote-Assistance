using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if ARFoundation2 || ARFoundation3
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
#elif ARFoundation
using UnityEngine.XR.ARFoundation;
#elif Vuforia
using Vuforia;
#endif


[CustomEditor(typeof(AnchorPointInitializer))]
public class AnchorPointInitializerEditor : Editor
{
    SerializedProperty frameworkProp;
    SerializedProperty arFoundationBaseProp;
    SerializedProperty vuforiaBaseProp;
    SerializedProperty planeFinderTypeProp;
    SerializedProperty screenPoseConverterTypeProp;
    SerializedProperty enableInteractionProp;
    SerializedProperty contextMenuProp;
    SerializedProperty storageProp;
    SerializedProperty loaderObjectProp;
    SerializedProperty autoSaveProp;
    SerializedProperty autoLoadProp;

    void OnEnable()
    {
        frameworkProp = serializedObject.FindProperty("framework");
        arFoundationBaseProp = serializedObject.FindProperty("arFoundationBase");
        vuforiaBaseProp = serializedObject.FindProperty("vuforiaBase");
        planeFinderTypeProp = serializedObject.FindProperty("planeFinderType");
        screenPoseConverterTypeProp = serializedObject.FindProperty("screenPoseConverterType");
        enableInteractionProp = serializedObject.FindProperty("enableInteraction");
        contextMenuProp = serializedObject.FindProperty("contextMenu");
        storageProp = serializedObject.FindProperty("storage");
        loaderObjectProp = serializedObject.FindProperty("loaderObject");
        autoSaveProp = serializedObject.FindProperty("autoSave");
        autoLoadProp = serializedObject.FindProperty("autoLoad");
    }


    #region GUI

    public override void OnInspectorGUI()
    {
        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
        serializedObject.Update();

        
        OnFrameworkGUI();
        OnAnchorPointGUI();
        OnStorageGUI();
        OnInteractionGUI();

        if (GUILayout.Button("Instantiate Game Objects"))
        {
            CreateGameObjects();
        }

        // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties();
    }


    private void OnFrameworkGUI()
    {
        EditorGUILayout.LabelField("AR Framework", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("Changing the AR Framework takes a few seconds. Please wait until the" +
            "inspector is reloaded before you continue.", MessageType.Info);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(frameworkProp, new GUIContent("AR Framework"));
        if (EditorGUI.EndChangeCheck())
        {
            var frameworkName = frameworkProp.enumNames[frameworkProp.enumValueIndex];
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, frameworkName);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, frameworkName);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WSA, frameworkName);
        }

        EditorGUILayout.ObjectField(arFoundationBaseProp, typeof(GameObject), new GUIContent("AR Foundation Base"));
        EditorGUILayout.ObjectField(vuforiaBaseProp, typeof(GameObject), new GUIContent("Vuforia Base"));



    }

    private SerializedProperty GetARFrameworkObject()
    {
        switch ((AnchorPointInitializer.ARFramework)frameworkProp.enumValueIndex)
        {
            case AnchorPointInitializer.ARFramework.ARFoundation:
            case AnchorPointInitializer.ARFramework.ARFoundation2:
            case AnchorPointInitializer.ARFramework.ARFoundation3:
                return arFoundationBaseProp;
            case AnchorPointInitializer.ARFramework.Vuforia:
                return vuforiaBaseProp;
        }
        return null;
    }

    private void OnAnchorPointGUI()
    {
        EditorGUILayout.LabelField("Anchor Point Layer", EditorStyles.boldLabel);

        SelectComponentGUI(planeFinderTypeProp, new ARPlaneFinderFactory(), "AR Plane Finder");
        SelectComponentGUI(screenPoseConverterTypeProp, new ScreenPoseConverterFactory(), "Screen Pose Converter");

    }

    private static void SelectComponentGUI(SerializedProperty stringProperty, ComponentList factory, string label)
    {
        var planeFinderTypes = factory.GetAllTypes().ToList();
        var index = planeFinderTypes.IndexOf(stringProperty.stringValue);
        if (index < 0) index = 0;
        index = EditorGUILayout.Popup(label, index, planeFinderTypes.ToArray());
        stringProperty.stringValue = planeFinderTypes[index];
    }

    private void OnInteractionGUI()
    {
        EditorGUILayout.LabelField("Interaction", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(enableInteractionProp);

        if(enableInteractionProp.boolValue)
        {
            EditorGUILayout.PropertyField(contextMenuProp);
        }
    }

    private void OnStorageGUI()
    {
        EditorGUILayout.LabelField("Anchor Point Storage", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(storageProp);
        if (storageProp.enumValueIndex != (int)AnchorPointInitializer.StorageLocation.None)
        {
            EditorGUILayout.PropertyField(autoSaveProp);
            EditorGUILayout.PropertyField(autoLoadProp);

            if (autoLoadProp.boolValue)
            {
#if Vuforia
                // select trackable
                EditorGUILayout.ObjectField(loaderObjectProp, typeof(TrackableBehaviour));
#elif ARFoundation2 || ARFoundation3
                // select image library
                EditorGUILayout.ObjectField(loaderObjectProp, typeof(XRReferenceImageLibrary));
#endif
            }
        }
    }

    #endregion


    #region Game Object Creation

    private void CreateGameObjects()
    {
        var arFramework = CreateInstance(GetARFrameworkObject());
        var baseObject = CreateAnchorPointManager();
        new ARPlaneFinderFactory().CreateComponent(baseObject, planeFinderTypeProp.stringValue);
        new ScreenPoseConverterFactory().CreateComponent(baseObject, screenPoseConverterTypeProp.stringValue);
        CreateAnchorPointStorage(baseObject, arFramework);
        CreateInteraction(baseObject);
    }

    private GameObject CreateAnchorPointManager()
    {
        var anchorPointManager = FindObjectOfType<AnchorPointManager>();
        if (anchorPointManager != null)
        {
            return anchorPointManager.gameObject;
        }

        GameObject baseObject = new GameObject("Anchor Point Manager", typeof(AnchorPointManager));

#if ARFoundation || ARFoundation2 || ARFoundation3
        var arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
        baseObject.transform.SetParent(arSessionOrigin.transform);
#endif
        return baseObject;
    }

    private void CreateInteraction(GameObject baseObject)
    {
        if(enableInteractionProp.boolValue)
        {
            var interaction = GetOrAddComponent<AnchorPointInteraction>(baseObject);

            if (interaction != null)
            {

                var contextMenu = CreateInstance(contextMenuProp);
                interaction.ContextMenu = (GameObject)contextMenu;
            }
        }
    }

    private void CreateAnchorPointStorage(GameObject baseObject, GameObject arFramework)
    {
        if(storageProp.enumValueIndex == (int)AnchorPointInitializer.StorageLocation.LocalFile)
        {            
            AnchorPointSaver autoSaver = null;
            if(autoSaveProp.boolValue)
            {
                autoSaver = GetOrAddComponent<AnchorPointSaver>(baseObject);
                autoSaver.AutoSave = true;
            }


            if (autoLoadProp.boolValue)
            {
                if (loaderObjectProp.objectReferenceValue != null)
                {
#if Vuforia
                    var nullPointObject = ((MonoBehaviour)loaderObjectProp.objectReferenceValue).gameObject;
                    GetOrAddComponent<TrackableLoader>(nullPointObject);
                
#elif ARFoundation2 || ARFoundation3
                    var arSession = arFramework.GetComponentInChildren<ARSessionOrigin>();
                    if(arSession != null)
                    {
                        var loader = GetOrAddComponent<TrackedImageLoader>(baseObject);
                        var imageManager = GetOrAddComponent<ARTrackedImageManager>(arSession.gameObject);
                        loader.trackedImageManager = imageManager;

                        imageManager.enabled = true;
                        imageManager.maxNumberOfMovingImages = 1;
                        var library = loaderObjectProp.objectReferenceValue as XRReferenceImageLibrary;
                        try
                        {

                            imageManager.referenceLibrary = library;
                        }
                        catch
                        {
                            // always throws exception, but works
                        }
                    }
#endif
                }
            }
        }
    }


    private T GetOrAddComponent<T>(GameObject baseObject) where T : Component
    {
        T component = baseObject.GetComponent<T>();
        if (component == null)
        {
            component = baseObject.AddComponent<T>();
        }
        return component;
    }

    private static GameObject CreateInstance(SerializedProperty property)
    {
        if (property == null)
            return null;

        var baseObject = (GameObject)property.objectReferenceValue;
        if (baseObject == null)
            return null;
        if (!IsPrefab(baseObject))
            return baseObject;
        
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(baseObject);
        property.objectReferenceValue = instance;
        return instance;

    }

    private static bool IsPrefab(GameObject gameObject)
    {
        return gameObject.scene.rootCount == 0;
    }

#endregion
}

