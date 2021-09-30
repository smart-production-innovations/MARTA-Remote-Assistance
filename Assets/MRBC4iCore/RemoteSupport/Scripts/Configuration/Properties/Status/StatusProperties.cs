using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using System.IO;

/// <summary>
/// Different software versions can be supported by the remote application. 
/// The application of the worker on site reports its calculation mode to the expert regarding the projection conversion from the 2d annotation into the 3D space.
/// </summary>
public enum CalculationMode
{
    ClickPointPlane, //deprecated projection method
    Projection // recommended projection method
}

/// <summary>
/// List of the configurable range of functions. All these functions can be dynamically activated or deactivated at runtime.
/// </summary>
public enum PropertyFlag
{
    ARActive, //Is the AR or non-AR mode active?
    ExpertHasCursorAnnotationActive, //Is the expert allowed to visualize his mouse position for the worker on site in the live video?
    ExpertHasDrawingAnnotationActive, //Is the expert allowed to create 2d annotations?
    ClientCanAnnotate, //Is the worker on site allowed to create 2d annotations?
    HasBandwidthOptions, //Is the selection between single image and live video transmission active?
    HasLiveDrawingTool, //Is the expert allowed to use the temporary live drawing function?
    ExpertHasProjectionLayerOption, //Can the expert manually adjust the AR project plane?
    ShowARPlanes, //Should the detected AR plane be displayed in the live video?
    ShowARFeaturePoints, //Should the detected AR point clouds be displayed in the live video?
    DefaultExpertMicrophoneOn, //Is the expert microphone on by default?
    DefaultClientMicrophoneOn, //Is the on-size worker microphone on by default?
    DefaultExpertSpeakerOn, //Is the expert speaker on by default?
    DefaultClientSpeakerOn, //Is the on-site worker speaker on by default?
    DrawingHasColorTool, //Does the drawing toolbar have the color selection tool?
    DrawingHasPenWidthTool, //Can the pen widths be adjusted via the drawing toolbar?
    DrawingHasPenTransparencyTool, //Can the color transparency be adjusted via the drawing toolbar?
    DrawingHasPenTool, //Does the drawing toolbar have the freehand drawing tool?
    DrawingHasEraserTool, //Does the drawing toolbar have the eraser tool?
    DrawingHasShapeTool, //Does the drawing toolbar have the shape tool?
    DrawingHasImageTool, //Does the drawing toolbar have the pictogram tool?
    DrawingHasArrowTool, //Does the drawing toolbar have the arrow tool?
    DrawingHasTextTool, //Does the drawing toolbar have the text tool?
    DrawingHasLayerTool, //Does the drawing toolbar have the drawing layers tool?
    DrawingHaClearTool, //Does the drawing toolbar have the cleanup tool?
    HasChatFeature, //Can the chat function be used?
    Properties, //May the expert adjust the configuration?
    Debug, //Can users view Debug.Log messages? Activation is recommended for development only.
    DefaultValueLiveDrawing, //Is live drawing active by default?
    DefaultValueLowBandwidthModeOn, //Is the single image or live video stream mode active by default?
    VideoTransmissionStarted,
    GenerateUniqueKey
}


/// <summary>
/// The current active range of functions can be queried by any script at any time via a static instance of the StatusProperties.
/// </summary>
public class StatusProperties
{
    /// <summary>
    /// Filename of the configuration file which saves the default configuration of the status properties.
    /// </summary>
    private static string File
    {
        get
        {
            return FileHelper.GetFilePath("config.xml");
        }
    }

    public static event Action OnPropertiesLoaded;

    private static StatusProperties values = null;
    /// <summary>
    /// singleton pattern for the status properties
    /// </summary>
    public static StatusProperties Values
    {
        get
        {
            if (values == null)
                values = new StatusProperties();
            return values;
        }
    }

    /// <summary>
    /// list of configuration values
    /// </summary>
    private Dictionary<PropertyFlag, bool> configValues;

    private string ConfigPreset = "Default";

    private int sessionID = -1;
    /// <summary>
    /// After ending a session, the expert can immediately start the next session by entering a new connection key without closing the application. 
    /// In order to clearly separate the individual sessions, each session is given its own ID.
    /// </summary>
    public int SessionID
    {
        get { return sessionID; }
    }

    #region load and save data
    /// <summary>
    /// Initialization of the StatusProperties class.
    /// </summary>
    public StatusProperties()
    {
        setDefaultProps();
    }

    /// <summary>
    /// load default configuration
    /// </summary>
    private void setDefaultProps()
    {
        configValues = new Dictionary<PropertyFlag, bool>();

        // initiate all possible functions as active
        foreach (var flag in (PropertyFlag[])Enum.GetValues(typeof(PropertyFlag)))
        {
            configValues.Add(flag, true);
        }

        // deactivate the default state of some functions
        //configValues[PropertyFlag.ARActive] = false;
        configValues[PropertyFlag.DefaultExpertMicrophoneOn] = false;
        configValues[PropertyFlag.DefaultClientMicrophoneOn] = false;
        configValues[PropertyFlag.DefaultExpertSpeakerOn] = false;
        configValues[PropertyFlag.DefaultClientSpeakerOn] = false;

        configValues[PropertyFlag.DrawingHasPenWidthTool] = false;
        configValues[PropertyFlag.DrawingHasPenTransparencyTool] = false;
        configValues[PropertyFlag.DrawingHasEraserTool] = false;
        configValues[PropertyFlag.DrawingHasShapeTool] = false;
        configValues[PropertyFlag.DrawingHasImageTool] = false;
        configValues[PropertyFlag.DrawingHasLayerTool] = false;
        configValues[PropertyFlag.Debug] = false;
        configValues[PropertyFlag.DefaultValueLiveDrawing] = false;
        configValues[PropertyFlag.DefaultValueLowBandwidthModeOn] = false;
        configValues[PropertyFlag.VideoTransmissionStarted] = false;
    }

    private string[] presets = null;
    // would give a list of names of all defined preset options
    public string[] Presets
    {
        get
        {
            if (presets == null)
            {
                if (System.IO.File.Exists(StatusProperties.File))
                {
                    // load the list of names from the configuration file
                    var xml = new XmlDocument();
                    xml.Load(StatusProperties.File);
                    var parent = xml.DocumentElement;
                    var presetEnum = parent.GetEnumerator();
                    presets = new string[parent.ChildNodes.Count];
                    for (int i = 0; i < parent.ChildNodes.Count; i++)
                    {
                        presets[i] = parent.ChildNodes[i].Name;
                    }
                }
                else
                    presets = new string[0];

                // if there is only one configuration option it is loaded automatically
                if (presets.Length == 1)
                    ConfigPreset = presets[0];
            }
            return presets;
        }
    }

    public bool IsLoaded { get; set; } = false;

    /// <summary>
    /// load the given configuration
    /// </summary>
    /// <param name="name">preset name to load</param>
    /// <param name="informDevices">synchronizes the newly loaded configuration with all connected remote devices</param>
    public void loadPreset(string name = null, bool informDevices = false)
    {
        if (name != null)
            ConfigPreset = name;

        loadData(informDevices);
        if (OnPropertiesLoaded != null) OnPropertiesLoaded();
    }

    /// <summary>
    /// load the given configuration
    /// </summary>
    /// <param name="informDevices">synchronizes the newly loaded configuration with all connected remote devices</param>
    private void loadData(bool informDevices = false)
    {
        if (System.IO.File.Exists(StatusProperties.File))
        {
            var xml = new XmlDocument();
            xml.Load(StatusProperties.File);
            loadData(xml, informDevices);
        }
    }

    /// <summary>
    /// load the given configuration
    /// </summary>
    /// <param name="xml">Configuration in XML format</param>
    /// <param name="informDevices">synchronizes the newly loaded configuration with all connected remote devices</param>
    private void loadData(XmlDocument xml, bool informDevices = false)
    {
        // convert the XML configuration into the configValue dictionary
        var element = xml.DocumentElement;
        if (element != null)
        {
            var presetItem = element.SelectSingleNode(ConfigPreset);
            if (presetItem != null)
            {
                var elemEnum = presetItem.GetEnumerator();
                while (elemEnum.MoveNext())
                {
                    var xmlItem = (XmlElement)elemEnum.Current;
                    var flagName = xmlItem.Name;
                    PropertyFlag flag;
                    if (Enum.TryParse<PropertyFlag>(flagName, out flag))
                    {
                        bool flagValue;
                        if (bool.TryParse(xmlItem.InnerText, out flagValue))
                            configValues[flag] = flagValue;
                    }
                }
            }
        }

        // synchronizes the newly loaded configuration with all connected remote devices
        if (informDevices)
            SendAllPropertiesToDevices();

        // Increase the session ID to ensure a unique session identifier for the new configuration
        sessionID++;
        IsLoaded = true;
    }

    /// <summary>
    /// store changes of the default configuration / selected preset
    /// </summary>
    public void saveData()
    {
        var xml = new XmlDocument();
        if (System.IO.File.Exists(StatusProperties.File))
        {
            xml.Load(StatusProperties.File);
        }

        XmlElement parent;
        // if there are no configuration entries yet, the corresponding XML structure is created
        if (!xml.HasChildNodes)
        {
            parent = xml.CreateElement("config");
            xml.AppendChild(parent);
        }
        else
        {
            parent = xml.DocumentElement;
        }

        // if there are no configuration entries yet, the corresponding XML structure is created
        var preset = parent.SelectSingleNode(ConfigPreset);
        if (preset == null)
        {
            preset = xml.CreateElement(ConfigPreset);
            parent.AppendChild(preset);
        }

        // convert all dictionary entry to the XML format
        foreach (var item in configValues)
        {
            // find corresponding XML entry
            var node = preset.SelectSingleNode(item.Key.ToString());
            if (node == null)
            {
                // if there  is no corresponding entry yet, create a entry for the property
                node = xml.CreateElement(item.Key.ToString());
                preset.AppendChild(node);
            }
            node.InnerText = item.Value.ToString();
        }

        xml.Save(StatusProperties.File);
    }
    #endregion

    /// <summary>
    /// change the given property state
    /// </summary>
    /// <param name="flag">property type</param>
    /// <param name="value">property value</param>
    /// <param name="informDevices">synchronizes the newly configuration with all connected remote devices</param>
    public void SetKey(PropertyFlag flag, bool value, bool informDevices = true)
    {
        // save new value in the dictionary
        configValues[flag] = value;
        // apply the new value to all UI elements associated with StatusProperties
        ToolProperties.SetAllItemsActive(true);
        // synchronizes the newly configuration with all connected remote devices
        if (informDevices)
            EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.StatusProperty, flag.ToString()+";"+value.ToString()));
    }

    /// <summary>
    /// get the actual state of a property entry
    /// </summary>
    /// <param name="flag">property type</param>
    /// <returns>property value</returns>
    public bool GetKey(PropertyFlag flag)
    {
        return configValues[flag];
    }

    /// <summary>
    /// synchronizes the whole configuration with all connected remote devices
    /// </summary>
    public void SendAllPropertiesToDevices()
    {
        foreach (var flag in (PropertyFlag[])Enum.GetValues(typeof(PropertyFlag)))
        {
            EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.StatusProperty, flag.ToString() + ";" + GetKey(flag).ToString()));
        }
        ARActive = ARActive;
    }

    /// <summary>
    /// is this the expert application?
    /// </summary>
    public bool isServer
    {
        get
        {
            if (CallSettings.HasInstance)
                return CallSettings.Instance.isServer;
            return false;
        }
    }

    private CalculationMode calculationMode = CalculationMode.ClickPointPlane;
    /// <summary>
    /// Different software versions can be supported by the remote application. 
    /// The application of the worker on site reports its calculation mode to the expert regarding the projection conversion from the 2d annotation into the 3D space.
    /// </summary>
    public CalculationMode CalculationMode
    {
        get
        {
            if (AnnotationManager.HasInstance)
            {
                // determine the projection method used in the scene
                if (AnnotationManager.Instance.AnnotationCanvasPrefab is AnchorAnnotationProjection3D)
                    return CalculationMode.Projection;

                return CalculationMode.ClickPointPlane;
            }
            return calculationMode;
        }
        set
        {
            calculationMode = value;
            // apply the new value to all UI elements associated with StatusProperties
            ToolProperties.SetAllItemsActive(true);
        }
    }

    /// <summary>
    /// The different projection calculation methods require a different annotation scaling factor
    /// </summary>
    public int DrawingResizeFactor
    {
        get
        {
            return (CalculationMode == CalculationMode.Projection ? 1 : 2);
        }
    }

    private bool isSmartGlassCommunication = false;
    /// <summary>
    /// Smart glasses do not support the full range of functions and require the simplification of the GUI compared to tablets or smartphones.
    /// Should the limited range of functions be loaded that is supported for communication with smart glasses?
    /// </summary>
    public bool IsSmartGlassCommunication
    {
        get
        {
            if (!isServer)
                return NonArMode.isSmartGlass;
            return isSmartGlassCommunication;
        }
        set { isSmartGlassCommunication = value; }
    }

    /// <summary>
    /// Is the connected mobile device a smart glasses?
    /// </summary>
    public bool IsSmartGlass
    {
        get
        {
            if (!isServer)
                return NonArMode.isSmartGlass;
            return false;
        }
    }

    private bool supportsARMode = true;
    /// <summary>
    /// Does the connected mobile device support AR calculation?
    /// </summary>
    public bool SupportsARMode
    {
        get
        {

            if (isServer)
                return supportsARMode;

#if !ARFoundation && !ARFoundation2 && !ARFoundation3 && !ARCore && !Vuforia
            // If no AR calculation algorithm is defined, AR is not supported.
            return false;
#else
            return supportsARMode;
#endif
        }
        set
        {
            if (!isServer)
            {
#if !ARFoundation && !ARFoundation2 && !ARFoundation3 && !ARCore && !Vuforia
                // If no AR calculation algorithm is defined, AR is not supported.
                supportsARMode = false;
#else
                supportsARMode = value;
#endif
            }
            else supportsARMode = value;

            if (ARModeManager.HasInstance && !supportsARMode)
                ARModeManager.Instance.SetARMode(false);
            // apply the new value to all UI elements associated with StatusProperties
            ToolProperties.SetAllItemsActive(true);
        }
    }

    /// <summary>
    /// Is the AR or non-AR mode active?
    /// </summary>
    public bool ARActive
    {
        get
        {
            //If the AR calculation is not supported the non-AR mode is always active.
            if (!SupportsARMode)
                return false;
            return configValues[PropertyFlag.ARActive];
        }
        set
        {
            //If the AR calculation is not supported the non-AR mode is always active.
            if (!SupportsARMode)
                SetKey(PropertyFlag.ARActive, false);
            else
                SetKey(PropertyFlag.ARActive, value);

            if (ARModeManager.HasInstance)
                ARModeManager.Instance.SetARMode(ARActive);

            GalleryTool.SetAllItemsActive();

            // synchronizes the newly configuration with all connected remote devices
            EventNameManager.SendEventCommandMsg(new CommandMsg(CommandMsgType.ARMode, ARActive.ToString()));
        }
    }

    /// <summary>
    /// May the expert adjust the configuration?
    /// </summary>
    public bool CanEditProperties
    {
        get
        {
            if (!isServer)
                return false;
            return configValues[PropertyFlag.Properties];
        }
        set { SetKey(PropertyFlag.Properties, value); }
    }

    /// <summary>
    /// Can users view Debug.Log messages? Activation is recommended for development only.
    /// </summary>
    public bool DebugTools
    {
        get { return configValues[PropertyFlag.Debug]; }
        set { SetKey(PropertyFlag.Debug, value); }
    }

    /// <summary>
    /// Is the expert allowed to visualize his mouse position for the worker on site in the live video?
    /// </summary>
    public bool ExpertHasCursorAnnotationActive
    {
        get { return configValues[PropertyFlag.ExpertHasCursorAnnotationActive]; }
        set { SetKey(PropertyFlag.ExpertHasCursorAnnotationActive, value); }
    }

    /// <summary>
    /// Is the expert allowed to create 2d annotations?
    /// </summary>
    public bool ExpertHasDrawingAnnotationActive
    {
        get
        {
            if (DrawingSettings.Instance.AnnotationType == AnnotationType.Pointer)
                return false;
            if (LowBandwidthActive)
                return true;
            return configValues[PropertyFlag.ExpertHasDrawingAnnotationActive];
        }
        set { SetKey(PropertyFlag.ExpertHasDrawingAnnotationActive, value); }
    }

    /// <summary>
    /// Is the worker on site allowed to create 2d annotations?
    /// </summary>
    public bool ClientCanAnnotate
    {
        get
        {
            if (IsSmartGlassCommunication)
                return false;
            if (DrawingSettings.Instance.AnnotationType == AnnotationType.Pointer)
                return false;
            if (!ExpertHasDrawingAnnotationActive)
                return false;
            return configValues[PropertyFlag.ClientCanAnnotate];
        }
        set { SetKey(PropertyFlag.ClientCanAnnotate, value); }
    }

    /// <summary>
    /// Is the selection between single image and live video transmission active?
    /// </summary>
    public bool HasBandwidthOptions
    {
        get { return configValues[PropertyFlag.HasBandwidthOptions]; }
        set { SetKey(PropertyFlag.HasBandwidthOptions, value); }
    }

    /// <summary>
    /// Is the single image or live video transmission active?
    /// </summary>
    public bool LowBandwidthActive
    {
        get
        {
            if (BandwidthManager.HasInstance)
            {
                return (BandwidthManager.Instance.SupportModeType == SupportModeType.LowBandwidthMode);
            }
            return false;
        }
    }

    /// <summary>
    /// Is the expert allowed to use the temporary live drawing function?
    /// </summary>
    public bool HasLiveDrawingTool
    {
        get
        {
            if (!ARActive)
                return false;
            return configValues[PropertyFlag.HasLiveDrawingTool];
        }
        set { SetKey(PropertyFlag.HasLiveDrawingTool, value); }
    }

    /// <summary>
    /// Can the expert manually adjust the AR project plane?
    /// </summary>
    public bool ExpertHasProjectionLayerOption
    {
        get
        {
            if (!ARActive)
                return false;
            if (CalculationMode != CalculationMode.Projection)
                return false;
            return configValues[PropertyFlag.ExpertHasProjectionLayerOption];
        }
        set { SetKey(PropertyFlag.ExpertHasProjectionLayerOption, value); }
    }

    /// <summary>
    /// Is it possible to adjust the projection plane manually?
    /// </summary>
    public bool HasProjectionLayerTool
    {
        get
        {
            if (!isServer)
                return false;
            return ExpertHasProjectionLayerOption;
        }
    }

    /// <summary>
    /// Should the detected AR plane be displayed in the live video?
    /// </summary>
    public bool ShowARPlanes
    {
        get { return configValues[PropertyFlag.ShowARPlanes]; }
        set { SetKey(PropertyFlag.ShowARPlanes, value); }
    }

    /// <summary>
    /// Should the detected AR point clouds be displayed in the live video?
    /// </summary>
    public bool ShowARFeaturePoints
    {
        get { return configValues[PropertyFlag.ShowARFeaturePoints]; }
        set { SetKey(PropertyFlag.ShowARFeaturePoints, value); }
    }

    /// <summary>
    /// Is the expert microphone on by default?
    /// </summary>
    public bool DefaultExpertMicrophoneOn
    {
        get { return configValues[PropertyFlag.DefaultExpertMicrophoneOn];   }
        set { SetKey(PropertyFlag.DefaultExpertMicrophoneOn, value); }
    }

    /// <summary>
    /// Is the on-size worker microphone on by default?
    /// </summary>
    public bool DefaultClientMicrophoneOn
    {
        get
        {
            if (IsSmartGlassCommunication)
                return true;
            return configValues[PropertyFlag.DefaultClientMicrophoneOn];
        }
        set { SetKey(PropertyFlag.DefaultClientMicrophoneOn, value); }
    }

    /// <summary>
    /// Is the expert speaker on by default?
    /// </summary>
    public bool DefaultExpertSpeakerOn
    {
        get { return configValues[PropertyFlag.DefaultExpertSpeakerOn];   }
        set { SetKey(PropertyFlag.DefaultExpertSpeakerOn, value); }
    }

    /// <summary>
    /// Is the on-site worker speaker on by default?
    /// </summary>
    public bool DefaultClientSpeakerOn
    {
        get
        {
            if (IsSmartGlassCommunication)
                return true;
            return configValues[PropertyFlag.DefaultClientSpeakerOn];
        }
        set { SetKey(PropertyFlag.DefaultClientSpeakerOn, value); }
    }

    /// <summary>
    /// Does the drawing toolbar have the color selection tool?
    /// </summary>
    public bool DrawingHasColorTool
    {
        get { return configValues[PropertyFlag.DrawingHasColorTool]; }
        set { SetKey(PropertyFlag.DrawingHasColorTool, value); }
    }

    /// <summary>
    /// Can the pen widths be adjusted via the drawing toolbar?
    /// </summary>
    public bool DrawingHasPenWidthTool
    {
        get
        {
            if (!isServer)
                return false;
            if (!DrawingHasPenTool)
                return false;
            return configValues[PropertyFlag.DrawingHasPenWidthTool];
        }
        set { SetKey(PropertyFlag.DrawingHasPenWidthTool, value); }
    }

    /// <summary>
    /// Can the color transparency be adjusted via the drawing toolbar?
    /// </summary>
    public bool DrawingHasPenTransparencyTool
    {
        get
        {
            if (!isServer)
                return false;
            if (!DrawingHasPenTool)
                return false;
            return configValues[PropertyFlag.DrawingHasPenTransparencyTool];
        }
        set { SetKey(PropertyFlag.DrawingHasPenTransparencyTool, value); }
    }

    /// <summary>
    /// Does the drawing toolbar have the freehand drawing tool?
    /// </summary>
    public bool DrawingHasPenTool
    {
        get { return configValues[PropertyFlag.DrawingHasPenTool]; }
        set { SetKey(PropertyFlag.DrawingHasPenTool, value); }
    }

    /// <summary>
    /// Does the drawing toolbar have the eraser tool?
    /// </summary>
    public bool DrawingHasEraserTool
    {
        get
        {
            if (!isServer)
                return false;
            if (!DrawingHasPenTool)
                return false;
            return configValues[PropertyFlag.DrawingHasEraserTool];
        }
        set { SetKey(PropertyFlag.DrawingHasEraserTool, value); }
    }

    /// <summary>
    /// Does the drawing toolbar have the shape tool?
    /// </summary>
    public bool DrawingHasShapeTool
    {
        get
        {
            if (!isServer)
                return false;
            return configValues[PropertyFlag.DrawingHasShapeTool];
        }
        set { SetKey(PropertyFlag.DrawingHasShapeTool, value); }
    }

    /// <summary>
    /// Does the drawing toolbar have the pictogram tool?
    /// </summary>
    public bool DrawingHasImageTool
    {
        get
        {
            if (!isServer)
                return false;
            return configValues[PropertyFlag.DrawingHasImageTool];
        }
        set { SetKey(PropertyFlag.DrawingHasImageTool, value); }
    }

    /// <summary>
    /// Does the drawing toolbar have the arrow tool?
    /// </summary>
    public bool DrawingHasArrowTool
    {
        get
        {
            if (DrawingHasImageTool)
                return false;
            return configValues[PropertyFlag.DrawingHasArrowTool];
        }
        set { SetKey(PropertyFlag.DrawingHasArrowTool, value); }
    }

    /// <summary>
    /// Does the drawing toolbar have the text tool?
    /// </summary>
    public bool DrawingHasTextTool
    {
        get
        {
            if (!isServer)
                return false;
            return configValues[PropertyFlag.DrawingHasTextTool];
        }
        set { SetKey(PropertyFlag.DrawingHasTextTool, value); }
    }

    /// <summary>
    /// Does the drawing toolbar have the drawing layers tool?
    /// </summary>
    public bool DrawingHasLayerTool
    {
        get
        {
            if (!isServer)
                return false;
            return configValues[PropertyFlag.DrawingHasLayerTool];
        }
        set { SetKey(PropertyFlag.DrawingHasLayerTool, value); }
    }

    /// <summary>
    /// Does the drawing toolbar have the cleanup tool?
    /// </summary>
    public bool DrawingHasClearTool
    {
        get
        {
            if (DrawingHasEraserTool && DrawingHasLayerTool)
                return false;
            return configValues[PropertyFlag.DrawingHaClearTool];
        }
        set { SetKey(PropertyFlag.DrawingHaClearTool, value); }
    }

    /// <summary>
    /// Can the chat function be used?
    /// </summary>
    public bool HasChatFeature
    {
        get
        {
            if (IsSmartGlassCommunication)
                return false;
            return configValues[PropertyFlag.HasChatFeature];
        }
        set { SetKey(PropertyFlag.HasChatFeature, value); }
    }

    /// <summary>
    /// Is live drawing active by default?
    /// </summary>
    public bool DefaultValueLiveDrawing
    {
        get { return configValues[PropertyFlag.DefaultValueLiveDrawing]; }
        set { SetKey(PropertyFlag.DefaultValueLiveDrawing, value); }
    }

    /// <summary>
    /// Is the single image or live video stream mode active by default?
    /// </summary>
    public bool DefaultValueLowBandwidthModeOn
    {
        get { return configValues[PropertyFlag.DefaultValueLowBandwidthModeOn]; }
        set { SetKey(PropertyFlag.DefaultValueLowBandwidthModeOn, value); }
    }

    public bool VideoTransmissionStarted
    {
        get { return configValues[PropertyFlag.VideoTransmissionStarted]; }
        set
        {
            if (VideoTransmissionStarted != value)
                SetKey(PropertyFlag.VideoTransmissionStarted, value);
        }
    }

    public bool GenerateUniqueKey
    {
        get { return configValues[PropertyFlag.GenerateUniqueKey]; }
        set { SetKey(PropertyFlag.GenerateUniqueKey, value); }
    }

    private float fieldOfView = -1;
    public float FieldOfView
    {
        get
        {
            if (fieldOfView == -1)
                fieldOfView = CameraHelper.ActiveARModeCamera.fieldOfView;
            return fieldOfView;
        }
        set { fieldOfView = value; }
    }
}
