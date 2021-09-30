using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnchorContent : MonoBehaviour
{
    protected AnchorPoint anchorPoint;
    protected bool componentsSet = false;

    public AnchorPoint Anchor
    {
        get
        {
            if (!componentsSet)
                SetComponents();

            return anchorPoint;
        }
    }

    public virtual void SetComponents()
    {
        componentsSet = true;

        anchorPoint = GetComponentInParent<AnchorPoint>();
        if (anchorPoint == null)
            anchorPoint = transform.parent.GetComponent<AnchorPoint>();
        if (anchorPoint == null)
        {
            var list = GetComponentsInParent<AnchorPoint>(true);
            if (list.Length > 0)
                anchorPoint = list[0];
        }
        if (anchorPoint == null)
            anchorPoint = GetComponent<AnchorPoint>();
        // disable component if no anchor point is found in scene graph
        if (anchorPoint == null)
            this.enabled = false;
    }

    public virtual void Start()
    {
        Instantiate();
        OnDisplayInitEmptyContent();
    }

    private bool isInstantiated = false;
    public void Instantiate()
    {
        if (!isInstantiated)
        {
            isInstantiated = true;
            instantiate();
        }
    }

    protected virtual void instantiate()
    {
        if (!componentsSet)
            SetComponents();
    }


    public virtual void OnDisplayInitEmptyContent()
    {
    }
}
