using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class AnchorPointInteraction : MonoBehaviour
{

    [Tooltip("Show this context menu on long press on existing anchor")]
    public GameObject ContextMenu;

    private AnchorPointManager anchorPointManager;
    // used to distinguish between tap and long press
    private float touchBeginTime;
    private bool isTouching = false;


    #region Unity
    void Start()
    {
        anchorPointManager = AManager<AnchorPointManager>.Instance;
        ShowContextMenu(false);
    }

    void Update()
    {
        // check if mouse/finger is over UI-element
        var fingerID = -1;
        if(!Application.isEditor && Input.touchCount > 0)
        {
            fingerID = Input.GetTouch(0).fingerId;
        }

        if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject(fingerID))
        {
            if (SystemInfo.deviceType == DeviceType.Desktop) // Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                UpdateMouseEvents();
            }
            else
            {
                UpdateTouchEvents();
            }
        }

    }

    #endregion


    #region Private

    private void UpdateMouseEvents()
    {
        // simulate with mouse input for playmode

        if (Input.GetMouseButtonDown(0))
        {
            touchBeginTime = Time.time;
        }
        else if (Input.GetMouseButton(0))
        {
            if (Time.time - touchBeginTime > 0.5f && RaycastAnchorPoint(Input.mousePosition))
            {
                ShowContextMenu(true, Input.mousePosition);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (!IsContextMenuEnabled() && !RaycastAnchorPoint(Input.mousePosition))
            {
                AddAnchor(Input.mousePosition.x, Input.mousePosition.y);
            }
        }
        else if (Input.GetMouseButtonUp(1))
        {
            var pose = new Pose(Input.mousePosition, CameraHelper.MainCamera.transform.rotation);
            anchorPointManager.SetNullPoint(pose, true);
        }
    }

    private void UpdateTouchEvents()
    {
        if(Input.touchCount == 0)
        {
            isTouching = false;
            return;
        }

        var touch = Input.GetTouch(0);
        bool tapEnded = touch.phase.Equals(TouchPhase.Ended);
        bool longPress = touch.phase.Equals(TouchPhase.Stationary) && Time.time - touchBeginTime > 0.5f;

        if (touch.phase.Equals(TouchPhase.Began))
        {
            // touch-begin - store time for detecting long press
            touchBeginTime = Time.time;
            isTouching = true;
        }
        else if(isTouching)
        {
            // touch-end - test if anchor point is selected
            if ((tapEnded || longPress) && RaycastAnchorPoint(touch.position))
            {
                if (longPress)
                {
                    ShowContextMenu(true, touch.position);
                }
            }
            else if(tapEnded)
            {
                // no anchor point selected => create a new one
                AddAnchor(touch.position.x, touch.position.y);
            }


        }
    }

    private bool RaycastAnchorPoint(Vector2 position)
    {
        // Construct a ray from the current touch coordinates
        Ray ray = CameraHelper.MainCamera.ScreenPointToRay(position);

        // check for intersections with 3D colliders
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var anchor = hit.collider.GetComponent<AnchorPoint>();
            if (anchor == null)
            {
                anchor = hit.collider.GetComponentInParent<AnchorPoint>();
            }
            if (anchor != null)
            {
                anchorPointManager.SelectedAnchor = anchor;
                return true;
            }
        }


        // check for intersections with 2D colliders
        var hit2d = Physics2D.GetRayIntersection(ray);
        if (hit2d)
        {
            var anchor = hit2d.collider.GetComponent<AnchorPoint>();
            if (anchor != null)
            {
                anchorPointManager.SelectedAnchor = anchor;
                return true;
            }
        }

        return false;
    }


    private void ShowContextMenu(bool show)
    {
        if (ContextMenu != null)
            ContextMenu.SetActive(show);

    }
    private void ShowContextMenu(bool show, Vector2 position)
    {
        if (ContextMenu != null)
        {
            var menu = ContextMenu.GetComponentInChildren<ContextMenu>();


            menu.transform.position = position;
            ContextMenu.SetActive(show);

        }
    }

    private bool IsContextMenuEnabled()
    {
        return ContextMenu != null && ContextMenu.activeSelf;
    }

    private void AddAnchor(float x, float y)
    {
        var anchor = anchorPointManager.AddAnchorPoint(x, y);

    }

    #endregion
}
