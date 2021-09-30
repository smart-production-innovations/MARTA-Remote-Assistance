using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handle Event Messages between different behavior scripts
/// </summary>
public class CallEventSystem
{
    private Dictionary<EventName, UnityEvent> eventDictionary;

    private static CallEventSystem globalEventSystem;

    /// <summary>
    /// singleton property
    /// </summary>
    public static CallEventSystem instance
    {
        get
        {
            if (globalEventSystem == null)
            {
                globalEventSystem = new CallEventSystem();

                if (globalEventSystem == null)
                {
                    Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
                }
                else
                {
                    globalEventSystem.Init();
                }
            }

            return globalEventSystem;
        }
    }

    /// <summary>
    /// initialize event massage system
    /// </summary>
    void Init()
    {
        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<EventName, UnityEvent>();
        }
    }

    /// <summary>
    /// start listen to events of event name
    /// </summary>
    /// <param name="eventName">listen event name</param>
    /// <param name="listener">callback function</param>
    public static void StartListening(EventName eventName, UnityAction listener)
    {
        UnityEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener(listener);
            instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    /// <summary>
    /// stop listen to events of event name
    /// </summary>
    /// <param name="eventName">listen event name</param>
    /// <param name="listener">callback function</param>
    public static void StopListening(EventName eventName, UnityAction listener)
    {
        if (globalEventSystem == null) return;
        UnityEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    /// <summary>
    /// fire connected events to event name
    /// </summary>
    /// <param name="eventName">event name</param>
    public static void TriggerEvent(EventName eventName)
    {
        UnityEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke();
        }
    }
}
