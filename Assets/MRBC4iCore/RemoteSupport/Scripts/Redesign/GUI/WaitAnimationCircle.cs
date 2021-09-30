using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitAnimationCircle : MonoBehaviour
{
    public Image image = null;
    public int numberOfFrames = 18;
    public float cycleTime = 1.0f;
    public bool reverse = false;

    private Coroutine cr = null;

    private void OnEnable()
    {
        StopAnimation();
        cr = StartCoroutine("Rotate");
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    private IEnumerator Rotate()
    {
        while (true)
        {
            float time = cycleTime / numberOfFrames;
            yield return new WaitForSeconds(time);

            float angle = 360.0f / numberOfFrames;
            if (reverse) angle = -angle;

            transform.Rotate(0.0f, 0.0f, angle, Space.Self);
        }
    }

    private void StopAnimation()
    {
        if (cr != null)
        {
            StopCoroutine(cr);
            cr = null;
        }
    }
}
