using UnityEngine;
using UnityEngine.EventSystems;

public class TouchInputManager : MonoBehaviour
{
    [Header("Zoom and Drag Settings")]
    private PencilManager pencilManager; // PencilManager referansý
    private float dragSpeed;
    private float zoomSpeed;
    private float maxZoom;

    private Camera mainCamera;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Vector3 touchStart;
    private bool isDragging = false;
    private bool isPointerOverUI = false;

    private void Start()
    {
        mainCamera = Camera.main;
        pencilManager = FindObjectOfType<PencilManager>(); // PencilManager'ý bul
        if (pencilManager != null)
        {
            // PencilManager'dan deðerleri al
            zoomSpeed = pencilManager.zoomSpeed;
            maxZoom = pencilManager.maxZoom;
            dragSpeed = pencilManager.dragSpeed;
        }
        else
        {
            Debug.LogError("PencilManager script not found.");
        }

        originalScale = transform.localScale;
        originalPosition = transform.position;
        CalculateBounds();
    }

    private void Update()
    {
        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                isPointerOverUI = IsPointerOverUI();

                if (isPointerOverUI)
                {
                    return;
                }

                touchStart = mainCamera.ScreenToWorldPoint(touch.position);
                isDragging = false;
            }

            if (touch.phase == TouchPhase.Moved && !isPointerOverUI && IsImageZoomed())
            {
                Vector3 direction = touchStart - mainCamera.ScreenToWorldPoint(touch.position);
                mainCamera.transform.position += direction * dragSpeed;
                isDragging = true;
            }

            if (touch.phase == TouchPhase.Ended && !isDragging && !isPointerOverUI)
            {
                Vector2 touchPosition = touch.position;
                Vector2 worldPosition = mainCamera.ScreenToWorldPoint(touchPosition);
                FindObjectOfType<FloodFill>()?.HandleTouchEnd(worldPosition);
            }
        }
        else if (Input.touchCount == 2)
        {
            HandleZoom();
        }
    }

    private void HandleZoom()
    {
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        float prevMagnitude = (touch1.position - touch1.deltaPosition - (touch2.position - touch2.deltaPosition)).magnitude;
        float currentMagnitude = (touch1.position - touch2.position).magnitude;
        float difference = currentMagnitude - prevMagnitude;

        Zoom(difference * zoomSpeed);
    }

    private bool IsImageZoomed()
    {
        return transform.localScale.x > originalScale.x || transform.localScale.y > originalScale.y;
    }

    public void Zoom(float increment)
    {
        Vector3 newScale = transform.localScale + Vector3.one * increment;
        newScale.x = Mathf.Clamp(newScale.x, originalScale.x, originalScale.x * maxZoom);
        newScale.y = Mathf.Clamp(newScale.y, originalScale.y, originalScale.y * maxZoom);

        transform.localScale = newScale;

        if (newScale.x <= originalScale.x && newScale.y <= originalScale.y)
        {
            ResetToOriginal();
        }
        else
        {
            CalculateBounds();
        }
    }

    private void ResetToOriginal()
    {
        transform.localScale = originalScale;
        transform.position = originalPosition;
        mainCamera.transform.position = new Vector3(0, 0, -10);
    }

    private void CalculateBounds()
    {
        float vertExtent = mainCamera.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        float spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x * transform.localScale.x;
        float spriteHeight = GetComponent<SpriteRenderer>().bounds.size.y * transform.localScale.y;

        float minX = GetComponent<SpriteRenderer>().bounds.min.x + horzExtent - spriteWidth / 2f;
        float maxX = GetComponent<SpriteRenderer>().bounds.max.x - horzExtent + spriteWidth / 2f;
        float minY = GetComponent<SpriteRenderer>().bounds.min.y + vertExtent - spriteHeight / 2f;
        float maxY = GetComponent<SpriteRenderer>().bounds.max.y - vertExtent + spriteHeight / 2f;
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
