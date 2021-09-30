using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Types of different event messages between behavior scripts
/// </summary>
public enum EventName
{
    Append,
    AppendDebug,
    SendCommandMsg,
    SendData,
    SetActiveVideoDevice,
    SendImageToClient,
    SetImageToLastAnchor,
    SetARAnchor,
    UniqueKeyCalculated,
    RemoteCallAccepted,
    RemoteCallEnded,
    RemoteMessage,
    RemoteData,
    BarcodeScaned,
    MenuStepDone, //unused
    WebRTCSend,
    ClearMessageBox,
    ShowARHelper,
    SetImageToAnchor,
    HideARHelper,
    AnchorCreated
}

/// <summary>
/// Data that will be send by the Event SetARAnchor
/// </summary>
public struct ARAnchorData
{
    public Vector2Int coordinate;
    public float drawingAreaScale;
}

/// <summary>
/// General static methods for calling the event system
/// </summary>
public class ActionEventManager
{
    public static void Subscribe<T>(EventName eventName, UnityAction<T> callback)
    {
        CallEventSystemWithArgs<T>.StartListening(eventName, callback);
    }

    public static void Unsubscribe<T>(EventName eventName, UnityAction<T> callback)
    {
        CallEventSystemWithArgs<T>.StopListening(eventName, callback);
    }

    public static void SendEvent<T>(EventName eventName, T msg)
    {
        CallEventSystemWithArgs<T>.TriggerEvent(eventName, msg);
    }

    public static void Subscribe(EventName eventName, UnityAction callback)
    {
        CallEventSystem.StartListening(eventName, callback);
    }

    public static void Unsubscribe(EventName eventName, UnityAction callback)
    {
        CallEventSystem.StopListening(eventName, callback);
    }

    public static void SendEvent(EventName eventName)
    {
        CallEventSystem.TriggerEvent(eventName);
    }

}

/// <summary>
/// Specific static methods for calling the event system
/// </summary>
public class EventNameManager : ActionEventManager
{
    public static void SendEventAppend(string msg)
    {
        CallEventSystemWithArgs<string>.TriggerEvent(EventName.Append, msg);
    }

    public static void SendEventAppendDebug(string msg)
    {
        CallEventSystemWithArgs<string>.TriggerEvent(EventName.AppendDebug, msg);
    }

    public static void SendEventCommandMsg(CommandMsg msg)
    {
        CallEventSystemWithArgs<CommandMsg>.TriggerEvent(EventName.SendCommandMsg, msg);
    }

    public static void SendEventData(byte[] msg)
    {
        CallEventSystemWithArgs<byte[]>.TriggerEvent(EventName.SendData, msg);
    }

    public static void SendEventActiveVideoDevice(string msg)
    {
        CallEventSystemWithArgs<string>.TriggerEvent(EventName.SetActiveVideoDevice, msg);
    }

    public static void SendEventImageToClient()
    {
        CallEventSystem.TriggerEvent(EventName.SendImageToClient);
    }

    public static void SendEventImageToLastAnchor(byte[] msg)
    {
        CallEventSystemWithArgs<byte[]>.TriggerEvent(EventName.SetImageToLastAnchor, msg);
    }

    public static void SendEventImageToAnchor(AnnotationImageData msg)
    {
        CallEventSystemWithArgs<AnnotationImageData>.TriggerEvent(EventName.SetImageToAnchor, msg);
    }

    public static void SendEventARAnchor(Vector2Int msg, float drawingAreaScale = 1)
    {
        CallEventSystemWithArgs<ARAnchorData>.TriggerEvent(EventName.SetARAnchor, new ARAnchorData() { coordinate = msg, drawingAreaScale = drawingAreaScale });
    }

    public static void SendEventShowARHelper(string animationName)
    {
        CallEventSystemWithArgs<string>.TriggerEvent(EventName.ShowARHelper, animationName);
    }

    public static void SendEventHideARHelper()
    {
        CallEventSystem.TriggerEvent(EventName.HideARHelper);
    }

    public static void SendEventAnchorCreated()
    {
        CallEventSystem.TriggerEvent(EventName.AnchorCreated);
    }
}