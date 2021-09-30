using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Define the structure of the MarkerItemBase implementation.
/// This structure is referenced for the type definition of list items in the MarkerItem class.
/// </summary>
public interface IMarkerItem
{
    /// <summary>
    /// define what type of action should be performed on the components
    /// </summary>
    MarkerType MarkerType {get;}

    /// <summary>
    /// Trigger the marker calculation for all assigned marker components
    /// </summary>
    /// <param name="value">new marker state value</param>
    /// <returns>calculated maker state</returns>
    bool CalculateActivityValueForAllAssignedMarkerItems(bool value);

    /// <summary>
    /// Trigger the marker calculation for all assigned marker components
    /// </summary>
    /// <returns>calculated maker state</returns>
    bool CalculateActivityValueForAllAssignedMarkerItems();

    /// <summary>
    /// define the calculation for the different type of actions
    /// </summary>
    /// <param name="state">marker state</param>
    void ExecuteMarkerType(bool state);

    /// <summary>
    /// is the marker component active
    /// </summary>
    bool Enabled { get; }
}

/// <summary>
/// Type of action to be performed on the components
/// </summary>
public enum MarkerType
{
    SetActive,
    ToggleIsOn,
    Custom
}

/// <summary>
/// Mark game objects that are used in a special context.
/// </summary>
public abstract class MarkerItemBase : MonoBehaviour, IMarkerItem
{
    public MarkerType markerType = MarkerType.SetActive;
    /// <summary>
    /// define what type of action should be performed on the components
    /// </summary>
    public MarkerType MarkerType
    {
        get
        {
            return markerType;
        }
    }

    private bool lastActivatiyCalculationValue;
    private bool isLastActivatiyCalculationValueInitialized = false;
    /// <summary>
    /// remember the last state of the marker action
    /// </summary>
    protected bool LastActivatiyCalculationValue
    {
        get
        {
            if (!isLastActivatiyCalculationValueInitialized)
            {
                LastActivatiyCalculationValue = lastActivatiyCalculationValueInitValue;
            }
            return lastActivatiyCalculationValue;
        }
        set
        {
            isLastActivatiyCalculationValueInitialized = true;
            lastActivatiyCalculationValue = value;
        }
    }

    /// <summary>
    /// define the default value of the marker action
    /// </summary>
    protected virtual bool lastActivatiyCalculationValueInitValue
    {
        get
        {
            return true;
        }
    }

    /// <summary>
    /// is the marker component active
    /// </summary>
    public bool Enabled
    {
        get { return enabled; }
    }

    /// <summary>
    /// Trigger a specific calculation when the marker state changes.
    /// The specific calculation is implemented in the subclasses of MarkerItemBase.
    /// </summary>
    /// <param name="value">new marker state value</param>
    /// <returns>calculated maker state</returns>
    public virtual bool CalculateActivityValue(bool value)
    {
        LastActivatiyCalculationValue = value;
        return value;
    }

    /// <summary>
    /// Trigger the marker calculation for the active marker state.
    /// </summary>
    /// <returns>calculated maker state</returns>
    public virtual bool CalculateActivityValue()
    {
        return CalculateActivityValue(LastActivatiyCalculationValue);
    }

    /// <summary>
    /// Trigger the marker calculation for all assigned marker components
    /// </summary>
    /// <param name="value">new marker state value</param>
    /// <returns>calculated maker state</returns>
    public bool CalculateActivityValueForAllAssignedMarkerItems(bool value)
    {
        var state = CalculateActivityValue(value);
        return CalculateStateForAllAssignedMarkerItems(state);
    }

    /// <summary>
    /// Trigger the marker calculation for all assigned marker components
    /// </summary>
    /// <returns>calculated maker state</returns>
    public bool CalculateActivityValueForAllAssignedMarkerItems()
    {
        return CalculateStateForAllAssignedMarkerItems();
    }

    /// <summary>
    /// Trigger the marker calculation for all assigned marker components
    /// </summary>
    /// <param name="state">initial marker state</param>
    /// <returns>calculated marker state</returns>
    private bool CalculateStateForAllAssignedMarkerItems(bool state = true)
    {
        var items = GetComponents<MarkerItemBase>();
        foreach (var item in items)
        {
            if (item.MarkerType == MarkerType && item.enabled)
            {
                var itemState = item.CalculateActivityValue();
                state = (state && itemState);
            }
        }
        return state;
    }

    /// <summary>
    /// define the calculation for the different type of actions
    /// </summary>
    /// <param name="state">marker state</param>
    public virtual void ExecuteMarkerType(bool state)
    {
        switch (MarkerType)
        {
            case MarkerType.SetActive:
                gameObject.SetActive(state);
                break;
            case MarkerType.ToggleIsOn:
                var toggle = GetComponent<Toggle>();
                if (toggle)
                    toggle.isOn = state;
                /*else
                {
                    var toggleEvent = GetComponent<ToggleEvent>();
                    if (toggleEvent)
                    {
                        toggleEvent.isOn = state;
                    }
                }*/
                break;
            case MarkerType.Custom:
                CustomExecuteMarkerType(state);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// define the calculation for the custom type of actions
    /// The specific calculation is implemented in the subclasses of MarkerItemBase.
    /// </summary>
    /// <param name="state">marker state</param>
    protected virtual void CustomExecuteMarkerType(bool state)
    {

    }
}

/// <summary>
/// Mark game objects that are used in a special context.
/// </summary>
/// <typeparam name="T">reference a list of MarkerItemBase objects</typeparam>
public abstract class  MarkerItem<T> : MarkerItemBase where T : Component, IMarkerItem
{
    private static T[] items = null;
    /// <summary>
    /// Get a list of all game objects which are only used in a special context
    /// </summary>
    public static T[] AllItems
    {
        get
        {
            if (items == null)
                items = SearchHelper.FindSceneObjectsOfTypeAll<T>();

            return items;
        }
    }

    /// <summary>
    /// Define if the game objects of the type T are active.
    /// </summary>
    /// <param name="value">true: game objects are active; false: game objects are inactive</param>
    public static void SetAllItemsActive(bool value)
    {
        foreach (var item in AllItems)
        {
            if (item.Enabled)
            {
                var state = item.CalculateActivityValueForAllAssignedMarkerItems(value);
                item.ExecuteMarkerType(state);
            }
        }
    }

    /// <summary>
    /// Define if the game objects of the type T are active.
    /// </summary>
    public static void SetAllItemsActive()
    {
        foreach (var item in AllItems)
        {
            if (item.Enabled)
            {
                var state = item.CalculateActivityValueForAllAssignedMarkerItems();
                item.ExecuteMarkerType(state);
            }
        }
    }
}

/// <summary>
/// Mark UI elements that are only used in a specific context and require the activeIfFeatureIsOn flag.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class FeatureTool<T> : MarkerItem<T> where T : Component, IMarkerItem
{
    /// <summary>
    /// defines whether the status should be inverted, which affects the UI behavior
    /// </summary>
    public bool activeIfFeatureIsOn = true;

    /// <summary>
    /// Trigger the feater tool calculation when the marker state changes.
    /// </summary>
    /// <param name="value">new marker state value</param>
    /// <returns>calculated maker state</returns>
    public override bool CalculateActivityValue(bool value)
    {
        var state = base.CalculateActivityValue(value);

        if (activeIfFeatureIsOn)
            return state;
        else
            return !state;
    }
}