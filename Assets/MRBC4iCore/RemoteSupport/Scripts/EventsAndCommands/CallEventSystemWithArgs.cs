using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handle Event Messages of type T between different behavior scripts
/// </summary>
/// <typeparam name="T">Type of the event message</typeparam>
public class CallEventSystemWithArgs<T>
{
    private Dictionary<EventName, UnityEvent<T>> eventDictionary;

    private static CallEventSystemWithArgs<T> globalEventSystem;

    /// <summary>
    /// singleton property
    /// </summary>
    public static CallEventSystemWithArgs<T> instance
    {
        get
        {
            if (globalEventSystem == null)
            {
                globalEventSystem = new CallEventSystemWithArgs<T>();

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
            eventDictionary = new Dictionary<EventName, UnityEvent<T>>();
        }
    }

    /// <summary>
    /// start listen to events of event name
    /// </summary>
    /// <param name="eventName">listen event name</param>
    /// <param name="listener">callback function</param>
    public static void StartListening(EventName eventName, UnityAction<T> listener)
    {
        UnityEvent<T> thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEventT();
            thisEvent.AddListener(listener);
            instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    /// <summary>
    /// stop listen to events of event name
    /// </summary>
    /// <param name="eventName">listen event name</param>
    /// <param name="listener">callback function</param>
    public static void StopListening(EventName eventName, UnityAction<T> listener)
    {
        if (globalEventSystem == null) return;
        UnityEvent<T> thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    /// <summary>
    /// fire connected events to event name
    /// </summary>
    /// <param name="eventName">event name</param>
    public static void TriggerEvent(EventName eventName, T msg)
    {
        UnityEvent<T> thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(msg);
        }
    }

    /// <summary>
    /// helper class
    /// </summary>
    public class UnityEventT : UnityEvent<T>
    {
        public UnityEventT() : base()
        {
        }
    }
}
