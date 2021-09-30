using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitAnimationPhone : MonoBehaviour
{
    public Image image = null;
    public int frames = 10;
    public float cycleTime = 1.0f;
    public AnimationCurve scale;
    public AnimationCurve rotate;

    private int frame = 0;
    private Coroutine cr = null;
    private Vector3 originalScale;


    private void OnEnable()
    {
        originalScale = transform.localScale;

        StopAnimation();
        cr = StartCoroutine("Animate");
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    private IEnumerator Animate()
    {
        while (true)
        {
            float time = cycleTime / frames;
            yield return new WaitForSeconds(time);

            frame = (frame + 1) % frames;
            Vector3 newScale = originalScale * scale.Evaluate(frame / (float)frames);

            transform.localScale = newScale;

            float angle = rotate.Evaluate(frame / (float)frames);
            transform.localEulerAngles = new Vector3(0.0f, 0.0f, angle);
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
