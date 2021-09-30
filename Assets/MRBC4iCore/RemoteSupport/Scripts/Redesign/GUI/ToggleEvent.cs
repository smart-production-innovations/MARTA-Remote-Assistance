using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ToggleEvent : MonoBehaviour
{
    public UnityEvent OnClickA;
    public UnityEvent OnClickB;

    public UnityEvent OnClickAPreCalculation;
    public UnityEvent OnClickBPreCalculation;

    public UnityEvent OnClickACancle;
    public UnityEvent OnClickBCancle;

    public EventName AWaitEvent;
    public EventName BWaitEvent;

    public bool nextOptionIsA = true;
    private bool toggleToA;

    private Animator animator;

    private bool wait;

    public bool isAOnOption = true;
    public bool isOn
    {
        get { return !nextOptionIsA; }
        set
        {
            var oldValue = nextOptionIsA;
            if (isAOnOption)
                nextOptionIsA = !value;
            else
                nextOptionIsA = value;

            if (gameObject.activeInHierarchy && enabled && nextOptionIsA != oldValue)
                Toggle();
        }
    }


    public bool Wait
    {
        get { return wait; }
        set
        {
            wait = value;
            if (Animator)
                Animator.SetBool("wait", wait);
        }
    }


    public Animator Animator
    {
        get
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            return animator;
        }
    }

    private void OnEnable()
    {
        toggleToA = !nextOptionIsA;
        Toggle();
    }


    void Awake()
    {
        toggleToA = nextOptionIsA;
        if (OnClickAPreCalculation.GetPersistentEventCount() > 0)
        {
            ActionEventManager.Subscribe(AWaitEvent, ToggleActionAfterWaitA);
        }
        if (OnClickBPreCalculation.GetPersistentEventCount() > 0)
        {
            ActionEventManager.Subscribe(BWaitEvent, ToggleActionAfterWaitB);
        }
    }

    void OnDestroy()
    {
        ActionEventManager.Subscribe(AWaitEvent, ToggleActionAfterWaitA);
        ActionEventManager.Subscribe(BWaitEvent, ToggleActionAfterWaitB);
    }

    private bool isToggling = false;
    public void Toggle()
    {
        if (isToggling) return;

        isToggling = true;
        Wait = false;
        if (toggleToA)
        {
            toggleToA = !toggleToA;
            if (OnClickAPreCalculation.GetPersistentEventCount() > 0)
            {
                Wait = true;
                OnClickAPreCalculation?.Invoke();
            }
            else
                OnClickA?.Invoke();
        }
        else
        {
            toggleToA = !toggleToA;
            if (OnClickBPreCalculation.GetPersistentEventCount() > 0)
            {
                Wait = true;
                OnClickBPreCalculation?.Invoke();
            }
            else
                OnClickB?.Invoke();
        }
        isToggling = false;
    }

    private void ToggleActionAfterWaitA()
    {
        if (!toggleToA && Wait)
            OnClickA?.Invoke();
        else
            OnClickACancle?.Invoke();

        if (Wait)
            Wait = false;
    }

    private void ToggleActionAfterWaitB()
    {
        if (toggleToA && Wait)
            OnClickB?.Invoke();
        else
            OnClickBCancle?.Invoke();

        if (Wait)
            Wait = false;
    }
}
