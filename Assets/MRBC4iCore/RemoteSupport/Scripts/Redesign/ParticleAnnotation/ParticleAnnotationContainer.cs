using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ParticleAnnotationType
{
    crosshair,
    line,
    arrow
}

public class ParticleAnnotationContainer : MonoBehaviour
{
    public ParticleAnnotationType activeParticleAnnotation = ParticleAnnotationType.crosshair;

    public ParticleAnnotation CrosshairInstance;
    public ParticleAnnotation LineInstance;
    public ParticleAnnotation ArrowInstance;

    protected float fadeOutStartTime = -1;
    protected float fadeOutDuration = -1;
    public float RemindingFadeOutTime
    {
        get
        {
            if (fadeOutStartTime > 0)
            {
                var fadeOutRunTime = (Time.time - fadeOutStartTime);
                if (fadeOutRunTime < fadeOutDuration)
                    return fadeOutDuration - fadeOutRunTime;
                else
                    return 0;
            }
            return float.MaxValue;
        }
    }

    public ParticleAnnotation createInstance(ParticleAnnotationType creationType)
    {
        switch (creationType)
        {
            case ParticleAnnotationType.crosshair:
                return GameObject.Instantiate(CrosshairInstance, transform);
            case ParticleAnnotationType.line:
                return GameObject.Instantiate(LineInstance, transform);
            case ParticleAnnotationType.arrow:
                return GameObject.Instantiate(ArrowInstance, transform);
            default:
                return null;
        }
    }

    protected ParticleAnnotation[] AnnotationList
    {
        get
        {
            return GetComponentsInChildren<ParticleAnnotation>();
        }
    }


    private void Update()
    {
        if (!hasParticles())
        {
            gameObject.SetActive(false);
        }
    }

    public void setParticleAnnotationType(ParticleAnnotationType creationType)
    {
        activeParticleAnnotation = creationType;
    }

    public void showParticleAnnotationType(Vector3 newPosition, Quaternion newRotation)
    {
        if (!AnimationIsRunning && IsEmitting)
        {
            StopEmission(RemindingFadeOutTime);
        }
        
        transform.position = newPosition;
        transform.rotation = newRotation;

        if (!IsEmitting)
        {
            createInstance(activeParticleAnnotation);
            AnimationIsRunning = true;
            IsEmitting = true;
            SetColor(selectedColor);
        }

        var size = MousePositionCalculation.getScaleFactor(transform);
        setSize(size.x);

        gameObject.SetActive(true);
    }

    public void clearAll()
    {
        foreach (var item in AnnotationList)
        {
            GameObject.Destroy(item.gameObject);
        }
        gameObject.SetActive(false);
    }

    public Color selectedColor = Color.red;
    public void SetColor(Color color)
    {
        selectedColor = color;
        foreach (var item in AnnotationList)
        {
            item.SetColor(color);
        }
    }

    public bool hasParticles()
    {
        foreach (var item in AnnotationList)
        {
            if (item.hasParticles())
            {
                return true;
            }
            else
            {
                GameObject.Destroy(item.gameObject);
            }
        }
        return false;
    }

    private bool isEmitting = false;
    public bool IsEmitting
    {
        get
        {
            return isEmitting;
            foreach (var item in AnnotationList)
            {
                if (item.IsEmitting())
                {
                    return true;
                }
            }
            return false;
        }
        set
        {
            isEmitting = value;
        }
    }

    private bool animationIsRunning = false;
    public bool AnimationIsRunning
    {
        get { return animationIsRunning; }
        set { animationIsRunning = value; }
    }


    public void FadeOutAnimation()
    {
        if (AnimationIsRunning)
        {
            AnimationIsRunning = false;
            fadeOutStartTime = Time.time;
            fadeOutDuration = MousePositionCalculation.MaxDisplayTime;
            StartCoroutine("fadeOutParticleAnimation");
        }
    }

    public void StopEmission(float waitTime = -1)
    {
        if (IsEmitting)
        {
            StopCoroutine("fadeOutParticleAnimation");
            stopParticleEmission(waitTime);
        }
    }

    private void stopParticleEmission(float waitTime = -1)
    {
        IsEmitting = false;
        if (waitTime < 0) waitTime = MousePositionCalculation.MaxDisplayTime;

        for (int i = 0; i < AnnotationList.Length; i++)
        {
            AnnotationList[i].StopEmission(waitTime);
        }
    }

    private IEnumerator fadeOutParticleAnimation()
    {
        yield return new WaitForSeconds(MousePositionCalculation.MaxDisplayTime);
        stopParticleEmission(RemindingFadeOutTime);
    }

    public virtual void setSize(float size)
    {
        for (int i = 0; i < AnnotationList.Length; i++)
        {
            AnnotationList[i].setSize(size);
        }
    }
}
