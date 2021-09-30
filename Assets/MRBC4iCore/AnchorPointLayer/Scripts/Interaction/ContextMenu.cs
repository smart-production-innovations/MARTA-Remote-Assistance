using UnityEngine;
using System.Collections;

public class ContextMenu : MonoBehaviour
{
    private AnchorPointManager manager;

    private int clicked = 0;

    void Start()
    {
        manager = AManager<AnchorPointManager>.Instance;
    }

    private void OnEnable()
    {
        clicked = 0;
    }

    private void Update()
    {
        if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            if (Input.GetMouseButtonUp(0))
                clicked++;
        }
        else
        {
            for (int i = 0; i < Input.touchCount; ++i)
            {
                if (Input.GetTouch(i).phase.Equals(TouchPhase.Ended))
                {
                    clicked++;
                    break;
                }
            }
        }

        // first click is enabling the context menu
        if(clicked >= 2)
        {
            StopAllCoroutines();
            StartCoroutine(DisableContextMenu());
        }
    }

    IEnumerator DisableContextMenu()
    {
        // wait until UI interactions are handled
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        gameObject.SetActive(false);
    }


    public void Delete()
    {
        var selectedAnchor = manager.SelectedAnchor;
        if (selectedAnchor == null)
            return;

        manager.RemoveAnchorPoint(selectedAnchor);
        Destroy(selectedAnchor.gameObject);
    }

    public void AlignToCamera()
    {
        var selectedAnchor = manager.SelectedAnchor;
        if (selectedAnchor == null)
            return;

        manager.AlignAnchor(selectedAnchor, AnchorPointManager.Alignment.Camera);
    }
    public void AlignToPlane()
    {
        var selectedAnchor = manager.SelectedAnchor;
        if (selectedAnchor == null)
            return;

        manager.AlignAnchor(selectedAnchor, AnchorPointManager.Alignment.Plane);

    }


}
