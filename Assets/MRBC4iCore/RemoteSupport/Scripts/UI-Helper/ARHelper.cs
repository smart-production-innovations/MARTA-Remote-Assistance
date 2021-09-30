using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// show animated hints if users use the application not probably
/// </summary>
[RequireComponent(typeof(Animator))]
public class ARHelper : MonoBehaviour
{
    private Animator arHelper;
    void Awake()
    {
        arHelper = GetComponent<Animator>();
        ActionEventManager.Subscribe<string>(EventName.ShowARHelper, showARHelper);
        ActionEventManager.Subscribe(EventName.HideARHelper, stopARHelper);
    }

    void OnDestroy()
    {
        ActionEventManager.Subscribe<string>(EventName.ShowARHelper, showARHelper);
        ActionEventManager.Subscribe(EventName.HideARHelper, stopARHelper);
    }

    /// <summary>
    /// show animated hint
    /// </summary>
    /// <param name="animationName">name of the animated hint which should be displayed</param>
    private void showARHelper(string animationName)
    {
        if (arHelper && arHelper.gameObject.activeInHierarchy)
            arHelper.Play(animationName);
    }
    /// <summary>
    /// hide all animated hints
    /// </summary>
    private void stopARHelper()
    {
        arHelper.Play("DefaultARHelperState");
    }
}
