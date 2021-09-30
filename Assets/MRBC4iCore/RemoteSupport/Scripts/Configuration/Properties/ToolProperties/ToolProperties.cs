using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A list of possible flags that influence the visibility or selection status of the ToolPorperties.
/// </summary>
public enum ToolFeature
{
    AR,
    Chat,
    Bandwidth,
    Projection,
    Livedrawing,
    Color,
    PenWidth,
    PenTransparency,
    Pen,
    Eraser,
    Shape,
    Image,
    Arrow,
    Text,
    DrawingLayer,
    Clear,
    SupportsARMode,
    Mute,
    Speaker,
    DrawingAnnotation,
    CursorAnnotation,
    ARPlanes,
    ARFeaturePoints,
    Gallery,
    Properties,
    Debug,
    DefaultValueLiveDrawing,
    DefaultValueLowBandwidthModeOn,
    IsSmartGlass,
    LowBandwidthActive,
    LowBandwidthInactive,
    IsSmartGlassCommunication,
    DrawingActive,
    ShowDrawingTools,
    IsServer,
    IsClient,
    VideoTransmissionStarted
}

/// <summary>
/// Mark UI elements that are only active when the selected flags (ToolFeature) are set to active. 
/// </summary>
public class ToolProperties : MarkerItem<ToolProperties>
{
    /// <summary>
    /// List of connected flags
    /// </summary>
    public ToolFeature[] inactiveWithoutFeature = new ToolFeature[1];

    /// <summary>
    /// Trigger a specific calculation when the marker state changes.
    /// Check if the connected flags are active.
    /// </summary>
    /// <param name="value">new marker state value</param>
    /// <returns>calculated maker state</returns>
    public override bool CalculateActivityValue(bool value)
    {
        var state = base.CalculateActivityValue(value);

        if (inactiveWithoutFeature != null)
        {
            //Check if the connected flags are active.
            foreach (var feature in inactiveWithoutFeature)
            {
                bool flagValue = true;

                switch (feature)
                {
                    case ToolFeature.AR:
                        flagValue = StatusProperties.Values.ARActive;
                        break;
                    case ToolFeature.Chat:
                        flagValue = StatusProperties.Values.HasChatFeature;
                        break;
                    case ToolFeature.Bandwidth:
                        flagValue = StatusProperties.Values.HasBandwidthOptions;
                        break;
                    case ToolFeature.LowBandwidthActive:
                        flagValue = StatusProperties.Values.LowBandwidthActive;
                        break;
                    case ToolFeature.LowBandwidthInactive:
                        flagValue = !StatusProperties.Values.LowBandwidthActive;
                        break;
                    case ToolFeature.Projection:
                        flagValue = StatusProperties.Values.HasProjectionLayerTool;
                        break;
                    case ToolFeature.Livedrawing:
                        flagValue = StatusProperties.Values.HasLiveDrawingTool;
                        break;
                    case ToolFeature.Color:
                        flagValue = StatusProperties.Values.DrawingHasColorTool;
                        break;
                    case ToolFeature.PenWidth:
                        flagValue = StatusProperties.Values.DrawingHasPenWidthTool;
                        break;
                    case ToolFeature.PenTransparency:
                        flagValue = StatusProperties.Values.DrawingHasPenTransparencyTool;
                        break;
                    case ToolFeature.Pen:
                        flagValue = StatusProperties.Values.DrawingHasPenTool;
                        break;
                    case ToolFeature.Eraser:
                        flagValue = StatusProperties.Values.DrawingHasEraserTool;
                        break;
                    case ToolFeature.Shape:
                        flagValue = StatusProperties.Values.DrawingHasShapeTool;
                        break;
                    case ToolFeature.Image:
                        flagValue = StatusProperties.Values.DrawingHasImageTool;
                        break;
                    case ToolFeature.Arrow:
                        flagValue = StatusProperties.Values.DrawingHasArrowTool;
                        break;
                    case ToolFeature.Text:
                        flagValue = StatusProperties.Values.DrawingHasTextTool;
                        break;
                    case ToolFeature.DrawingLayer:
                        flagValue = StatusProperties.Values.DrawingHasLayerTool;
                        break;
                    case ToolFeature.Clear:
                        flagValue = StatusProperties.Values.DrawingHasClearTool;
                        break;
                    case ToolFeature.SupportsARMode:
                        flagValue = StatusProperties.Values.SupportsARMode;
                        break;
                    case ToolFeature.Mute:
                        if (StatusProperties.Values.isServer)
                            flagValue = !StatusProperties.Values.DefaultExpertMicrophoneOn;
                        else
                            flagValue = !StatusProperties.Values.DefaultClientMicrophoneOn;
                        break;
                    case ToolFeature.Speaker:
                        if (StatusProperties.Values.isServer)
                            flagValue = !StatusProperties.Values.DefaultExpertSpeakerOn;
                        else
                            flagValue = !StatusProperties.Values.DefaultClientSpeakerOn;
                        break;
                    case ToolFeature.CursorAnnotation:
                        if (StatusProperties.Values.isServer)
                            flagValue = StatusProperties.Values.ExpertHasCursorAnnotationActive;
                        break;
                    case ToolFeature.DrawingAnnotation:
                        if (StatusProperties.Values.isServer)
                            flagValue = StatusProperties.Values.ExpertHasDrawingAnnotationActive;
                        else
                            flagValue = StatusProperties.Values.ClientCanAnnotate;
                        break;
                    case ToolFeature.Gallery:
                        flagValue = StatusProperties.Values.ExpertHasDrawingAnnotationActive;
                        break;
                    case ToolFeature.Properties:
                        flagValue = StatusProperties.Values.CanEditProperties;
                        break;
                    case ToolFeature.Debug:
                        flagValue = StatusProperties.Values.DebugTools;
                        break;
                    case ToolFeature.DefaultValueLiveDrawing:
                        flagValue = StatusProperties.Values.DefaultValueLiveDrawing;
                        break;
                    case ToolFeature.DefaultValueLowBandwidthModeOn:
                        flagValue = StatusProperties.Values.DefaultValueLowBandwidthModeOn;
                        break;
                    case ToolFeature.IsSmartGlass:
                        flagValue = !StatusProperties.Values.IsSmartGlass;
                        break;
                    case ToolFeature.IsSmartGlassCommunication:
                        flagValue = !StatusProperties.Values.IsSmartGlassCommunication;
                        break;
                    case ToolFeature.DrawingActive:
                        if (DrawingManager.HasInterfaceInstance)
                            flagValue = !DrawingManager.InterfaceInstance.DrawingActive;
                        break;
                    case ToolFeature.ShowDrawingTools:
                        flagValue = (DrawingSettings.Instance.AnnotationType == AnnotationType.Both);
                        break;
                    case ToolFeature.IsServer:
                        flagValue = StatusProperties.Values.isServer;
                        break;
                    case ToolFeature.IsClient:
                        flagValue = !StatusProperties.Values.isServer;
                        break;
                    case ToolFeature.VideoTransmissionStarted:
                        flagValue = StatusProperties.Values.VideoTransmissionStarted;
                        break;
                    default:
                        break;
                }

                if (!flagValue)
                    return false;
            }
        }

        return state;
    }

    /// <summary>
    /// Define the calculation for the custom type of actions.
    /// Enable or disable the AR feature visualization depending on the selected StatusProperties values.
    /// </summary>
    /// <param name="state">marker state</param>
    protected override void CustomExecuteMarkerType(bool state)
    {
        var visualizer = SearchHelper.FindSceneObjectsOfTypeAll<ARFeatureVisualization>();

        foreach (var arFeature in visualizer)
        {
            if (inactiveWithoutFeature.Contains(ToolFeature.ARPlanes) && arFeature.FeatureType == ARFeatureType.Planes)
                arFeature.DisplayFeature = StatusProperties.Values.ShowARPlanes;
            else if (inactiveWithoutFeature.Contains(ToolFeature.ARFeaturePoints) && arFeature.FeatureType == ARFeatureType.Points)
                arFeature.DisplayFeature = StatusProperties.Values.ShowARFeaturePoints;
        }
    }
}
