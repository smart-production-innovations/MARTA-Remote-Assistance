using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent onClick;
    public UnityEvent OnClickPreCalculation;
    public UnityEvent OnClickCancle;

    public EventName WaitEvent;
    public GameObject activeMark;

    public float transitionTime = 0.5f;
    public float pointerDownScale = 0.9f;
    public List<Transform> scaleTransforms = null;
    public Color hoverColor = Color.magenta;
    public List<Image> colorObjects = null;

    public int sizeClient = 150;
    public int sizeServer = 100;

    private Coroutine crScale = null;
    private bool validClick = false;
    private List<Vector3> originalScale = null;
    private float currentScale = 1.0f;
    private bool isSelectedByDefault = false;

    private Animator animator;
    public Animator Animator
    {
        get
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            return animator;
        }
    }

    private bool wait;

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

    public bool isSelected
    {
        get
        {
            if (activeMark) return activeMark.activeSelf;
            return false;
        }
        set
        {
            if (activeMark) activeMark.SetActive(value);
            if (!value) Wait = false;
        }
    }

    private void OnEnable()
    {
        originalScale = new List<Vector3>();
        for (int i = 0; i < scaleTransforms.Count; i++)
        {
            originalScale.Add(scaleTransforms[i].localScale);
        }

        FadeAlpha(0.0f, 0.0f);
        FadeAlpha(1.0f, transitionTime);

        SetScale(0.0f);
        Scale(1.0f);

        if (isSelectedByDefault != isSelected)
        {
            isSelected = isSelectedByDefault;
            if (isSelectedByDefault)
                invokeButton();
        }
    }

    void Awake()
    {
        isSelectedByDefault = isSelected;
        if (GetComponent<RectTransform>())
            GetComponent<RectTransform>().sizeDelta = (StatusProperties.Values.isServer ? new Vector2(sizeServer, sizeServer) : new Vector2(sizeClient, sizeClient));

        if (OnClickPreCalculation.GetPersistentEventCount() > 0)
        {
            ActionEventManager.Subscribe(WaitEvent, ActionAfterWait);
        }
    }

    void OnDestroy()
    {
        ActionEventManager.Subscribe(WaitEvent, ActionAfterWait);
    }

    public void Enable()
    {
        this.gameObject.SetActive(true);
    }

    public void Disable()
    {
        Scale(0.0f);
        FadeAlpha(0.0f, transitionTime);
        StartCoroutine("DisableDelayed", transitionTime);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        validClick = true;
        Scale(pointerDownScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Scale(1.0f);

        if (validClick)
        {
            invokeButton();
        }
    }

    private void invokeButton()
    {
        Wait = false;
        if (OnClickPreCalculation.GetPersistentEventCount() > 0)
        {
            Wait = true;
            OnClickPreCalculation?.Invoke();
        }
        else
            onClick?.Invoke();
    }

    private void ActionAfterWait()
    {
        if (Wait)
        {
            onClick?.Invoke();
            Wait = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        validClick = true;

        FadeColor(hoverColor, transitionTime);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        validClick = false;

        FadeColor(Color.white, transitionTime);
    }

    private IEnumerator ScaleCoroutine(float endScale)
    {
        float startScale = currentScale;

        float time = 0.0f;

        while (time < transitionTime)
        {
            float t = time / transitionTime;
            SetScale(Mathf.Lerp(startScale, endScale, t));
            yield return null;
            time += Time.deltaTime;
        }
        
        SetScale(endScale);
        crScale = null;
    }

    private IEnumerator DisableDelayed(float time)
    {
        yield return new WaitForSeconds(time);
        this.gameObject.SetActive(false);
    }

    private void Scale(float scale)
    {
        StopScale();
        crScale = StartCoroutine("ScaleCoroutine", scale);
    }

    private void FadeColor(Color color, float time)
    {
        for (int i = 0; i < colorObjects.Count; i++)
        {
            colorObjects[i].CrossFadeColor(color, time, false, false);
        }
    }

    private void FadeAlpha(float alpha, float time)
    {
        for (int i = 0; i < colorObjects.Count; i++)
        {
            colorObjects[i].CrossFadeAlpha(alpha, time, false);
        }
    }

    private void SetScale(float scale)
    {
        for (int i = 0; i < scaleTransforms.Count; i++)
        {
            scaleTransforms[i].localScale = originalScale[i] * scale;
        }
    }

    private void StopScale()
    {
        if (crScale != null)
        {
            StopCoroutine(crScale);
            crScale = null;
        }
    }
    
}
