using UnityEngine;
using UnityEngine.EventSystems;

public class MouseTouchInputManager : MonoBehaviour
{
    // Previous position of mouse/touch
    private Vector2 previousPosition;
    // Flag to track if we're dragging
    private bool isDragging = false;
    // You can adjust sensitivity as needed
    public float sensitivity = 1.0f;
    public FHClientController clientController;
    public RectTransform touchArea;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Handle mouse input
        if (Input.GetMouseButtonDown(0))
        {
            // Check if the mouse position is within the touchArea
            if (IsPointInTouchArea(Input.mousePosition))
            {
                // Left mouse button pressed down
                isDragging = true;
                previousPosition = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // Left mouse button released
            isDragging = false;
        }
        else if (isDragging && Input.GetMouseButton(0))
        {
            // Left mouse button is being held down
            Vector2 currentPosition = Input.mousePosition;
            Vector2 delta = (currentPosition - previousPosition) * sensitivity;
            // Now delta contains the movement increment since last frame
            HandleDrag(delta);
            // Update previous position
            previousPosition = currentPosition;
        }

        // Handle touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // Check if the touch position is within the touchArea
                if (IsPointInTouchArea(touch.position))
                {
                    // Touch began
                    isDragging = true;
                    previousPosition = touch.position;
                }
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                // Touch ended or was cancelled
                isDragging = false;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                // Touch is moving
                Vector2 currentPosition = touch.position;
                Vector2 delta = (currentPosition - previousPosition) * sensitivity;
                // Now delta contains the movement increment since last frame
                HandleDrag(delta);
                // Update previous position
                previousPosition = currentPosition;
            }
        }
    }

    private bool IsPointInTouchArea(Vector2 screenPoint)
    {
        if (touchArea == null)
            return true; // If no touchArea is assigned, allow touch anywhere

        // Convert screen point to local point in RectTransform
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            touchArea,
            screenPoint,
            null, // Use null for overlay canvas or mainCamera for camera canvas
            out Vector2 localPoint);

        // Check if the point is inside the rect
        return touchArea.rect.Contains(localPoint);
    }

    private void HandleDrag(Vector2 delta)
    {
        // Do something with delta movement
        if (delta != Vector2.zero)
        {
            Debug.Log("Movement delta: " + delta);
            clientController.Send(DataTypeEnum.LG20001, OrderTypeEnum.Rotate, delta.x + "," + delta.y);
        }
        // Example: Move an object based on delta
        // transform.Translate(new Vector3(delta.x, delta.y, 0) * Time.deltaTime);
        // Or rotate camera/object
        // transform.Rotate(new Vector3(-delta.y, delta.x, 0) * Time.deltaTime);
    }
}