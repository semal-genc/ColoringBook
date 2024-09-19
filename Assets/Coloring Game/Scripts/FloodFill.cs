using System.Collections.Generic;
using UnityEngine;

public class FloodFill : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Texture2D texture;
    [SerializeField] private Color fillColor = Color.red;
    [SerializeField] private Color boundaryColor = Color.black;
    [SerializeField] private Color targetColor = Color.white;
    [SerializeField] private float colorTolerance = 0.55f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float maxZoom = 3.0f;
    [SerializeField] private float dragSpeed = 0.05f;

    private SpriteRenderer spriteRenderer;
    private HashSet<Vector2Int> originalBoundaryPixels = new HashSet<Vector2Int>();
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Camera mainCamera;
    private Vector3 touchStart;

    private float minX, maxX, minY, maxY;
    private Vector3 previousCameraPosition;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found.");
            return;
        }

        StoreOriginalBoundaryPixels();

        originalScale = transform.localScale;
        originalPosition = transform.position;

        mainCamera = Camera.main;

        CalculateBounds();
        previousCameraPosition = mainCamera.transform.position;
    }

    private void Update()
    {
        HandleTouchInput();

        if (spriteRenderer == null || IsPointerOverUI())
        {
            return;
        }

        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPosition = Input.GetTouch(0).position;
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);
            Vector2Int pixelPosition = WorldToPixel(worldPosition);

            if (IsPixelValid(pixelPosition))
            {
                Color clickedColor = texture.GetPixel(pixelPosition.x, pixelPosition.y);
                targetColor = clickedColor;

                FloodFillArea(pixelPosition, fillColor);
            }
        }
    }

    private void LateUpdate()
    {
        // Optimize camera movement calculations
        if (mainCamera.transform.position != previousCameraPosition)
        {
            LimitCameraToBounds();
            previousCameraPosition = mainCamera.transform.position;
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStart = mainCamera.ScreenToWorldPoint(touch.position);
            }

            if (touch.phase == TouchPhase.Moved && IsImageZoomed())
            {
                Vector3 direction = touchStart - mainCamera.ScreenToWorldPoint(touch.position);
                mainCamera.transform.position += direction * dragSpeed;

                // Trigger LateUpdate() to handle boundary limits
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
            Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

            float prevMagnitude = (touch1PrevPos - touch2PrevPos).magnitude;
            float currentMagnitude = (touch1.position - touch2.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            Zoom(difference * zoomSpeed);

            // Trigger LateUpdate() to handle boundary limits
        }
    }

    private bool IsImageZoomed()
    {
        return transform.localScale.x > originalScale.x || transform.localScale.y > originalScale.y;
    }

    private void Zoom(float increment)
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

    private void LimitCameraToBounds()
    {
        Vector3 camPos = mainCamera.transform.position;
        float cameraSize = mainCamera.orthographicSize;
        float cameraAspect = Screen.width / (float)Screen.height;

        float halfWidth = cameraSize * cameraAspect;
        float halfHeight = cameraSize;

        float cameraMinX = minX + halfWidth;
        float cameraMaxX = maxX - halfWidth;
        float cameraMinY = minY + halfHeight;
        float cameraMaxY = maxY - halfHeight;

        camPos.x = Mathf.Clamp(camPos.x, cameraMinX, cameraMaxX);
        camPos.y = Mathf.Clamp(camPos.y, cameraMinY, cameraMaxY);
        mainCamera.transform.position = camPos;
    }

    private void CalculateBounds()
    {
        float vertExtent = mainCamera.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        float spriteWidth = spriteRenderer.bounds.size.x * transform.localScale.x;
        float spriteHeight = spriteRenderer.bounds.size.y * transform.localScale.y;

        minX = (spriteRenderer.bounds.min.x + horzExtent) - spriteWidth / 2f;
        maxX = (spriteRenderer.bounds.max.x - horzExtent) + spriteWidth / 2f;
        minY = (spriteRenderer.bounds.min.y + vertExtent) - spriteHeight / 2f;
        maxY = (spriteRenderer.bounds.max.y - vertExtent) + spriteHeight / 2f;
    }

    public void SetFillColor(Color newColor)
    {
        fillColor = newColor;
    }

    private void FloodFillArea(Vector2Int startPoint, Color newColor)
    {
        if (texture == null)
        {
            Debug.LogError("Texture is not specified.");
            return;
        }

        int width = texture.width;
        int height = texture.height;

        Color[] pixelArray = texture.GetPixels();
        Queue<Vector2Int> pixels = new Queue<Vector2Int>();
        pixels.Enqueue(startPoint);

        Color startColor = texture.GetPixel(startPoint.x, startPoint.y);

        if (!IsSimilarColor(startColor, targetColor) || originalBoundaryPixels.Contains(startPoint))
            return;

        HashSet<Vector2Int> visitedPixels = new HashSet<Vector2Int>();
        visitedPixels.Add(startPoint);

        while (pixels.Count > 0)
        {
            Vector2Int currentPixel = pixels.Dequeue();
            int x = currentPixel.x;
            int y = currentPixel.y;

            if (x < 0 || x >= width || y < 0 || y >= height)
                continue;

            int index = y * width + x;
            Color currentColor = pixelArray[index];

            if (originalBoundaryPixels.Contains(currentPixel))
                continue;

            if (IsSimilarColor(currentColor, targetColor))
            {
                pixelArray[index] = newColor;

                Vector2Int[] directions = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };
                foreach (var dir in directions)
                {
                    Vector2Int neighbor = currentPixel + dir;
                    if (!visitedPixels.Contains(neighbor) && IsPixelValid(neighbor))
                    {
                        pixels.Enqueue(neighbor);
                        visitedPixels.Add(neighbor);
                    }
                }
            }
        }

        texture.SetPixels(pixelArray);
        texture.Apply();
    }

    private bool IsSimilarColor(Color c1, Color c2)
    {
        return Mathf.Abs(c1.r - c2.r) < colorTolerance &&
               Mathf.Abs(c1.g - c2.g) < colorTolerance &&
               Mathf.Abs(c1.b - c2.b) < colorTolerance;
    }

    private Vector2Int WorldToPixel(Vector2 worldPosition)
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found.");
            return Vector2Int.zero;
        }

        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        float ppu = spriteRenderer.sprite.pixelsPerUnit;
        int x = Mathf.FloorToInt(localPos.x * ppu + texture.width / 2f);
        int y = Mathf.FloorToInt(localPos.y * ppu + texture.height / 2f);
        return new Vector2Int(x, y);
    }

    private bool IsPixelValid(Vector2Int pixelPosition)
    {
        return pixelPosition.x >= 0 && pixelPosition.x < texture.width &&
               pixelPosition.y >= 0 && pixelPosition.y < texture.height;
    }

    private bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    private void StoreOriginalBoundaryPixels()
    {
        int width = texture.width;
        int height = texture.height;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color pixelColor = texture.GetPixel(x, y);
                if (IsSimilarColor(pixelColor, boundaryColor))
                {
                    originalBoundaryPixels.Add(new Vector2Int(x, y));
                }
            }
        }
    }
}
