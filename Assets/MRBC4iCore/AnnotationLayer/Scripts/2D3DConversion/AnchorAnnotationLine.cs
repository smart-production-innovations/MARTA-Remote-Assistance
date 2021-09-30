using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorAnnotationLine : AnchorContent, IAnchorAnnotationBase
{
    public AnnotationOwner AnnotationOwner { get; set; } = AnnotationOwner.Client;

    protected bool isEmpty;
    /// <summary>
    /// Has the annotation a permanent saved content. 
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            return isEmpty;
        }
    }

    private LineRenderer line;

    public LineRenderer Line
    {
        get
        {
            if (!line)
                line = GetComponent<LineRenderer>();
            return line;
        }
    }

    public Color Color { get; set; }


    protected Snapshot snapshot;
    /// <summary>
    /// Get the screenshot which is connected to the annotation
    /// </summary>
    /// <returns></returns>
    public Snapshot GetSnapshot()
    {
        if (snapshot == null)
            snapshot = GetComponent<Snapshot>();
        return snapshot;
    }


    /// <summary>
    /// initialize a new annotation
    /// </summary>
    protected override void instantiate()
    {
        base.instantiate();

        //create snapshot to draw over while editing
        snapshot = GetComponent<Snapshot>();
        if (snapshot) snapshot.Instantiate();
        isEmpty = true;
    }

    public void AddPoint(Vector2 screenPoint)
    {

    }
}
