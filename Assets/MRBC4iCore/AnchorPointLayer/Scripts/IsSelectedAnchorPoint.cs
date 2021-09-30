using UnityEngine;
using System.Collections;

/// <summary>
/// Rendering is activated or deactivated based on the IsSelected-property of a parent anchor point
/// </summary>
public class IsSelectedAnchorPoint : MonoBehaviour
{
    private AnchorPoint anchorPoint;
    private Renderer goRenderer;

    void Start()
    {
        // get anchor point, either in parent or same game object
        anchorPoint = GetComponentInParent<AnchorPoint>();
        if (anchorPoint == null)
            anchorPoint = GetComponent<AnchorPoint>();

        // disable this component if no anchor point is found in scene graph
        if (anchorPoint == null)
            this.enabled = false;


        goRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (goRenderer != null)
        {
            if (anchorPoint.IsSelected != goRenderer.enabled)
            {
                goRenderer.enabled = anchorPoint.IsSelected;
            }
        }
    }

}
