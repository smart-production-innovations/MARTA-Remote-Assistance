using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class takePictureEffect : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(PlayEffect());
    }

    IEnumerator PlayEffect()
    {
        yield return new WaitForSeconds(0.2f);
        gameObject.SetActive(false);
    }
}
