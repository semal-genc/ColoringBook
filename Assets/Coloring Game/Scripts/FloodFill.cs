using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FloodFill : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Texture2D texture;

    private SpriteRenderer spriteRenderer;
    private HashSet<Vector2Int> originalBoundaryPixels = new HashSet<Vector2Int>();

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Vector3 touchStart;
    private Vector3 previousCameraPosition;

    private bool isDragging = false;
    private Camera mainCamera;
    private float minX, maxX, minY, maxY;

    private PencilManager pencilManager;

    private void Start()
    {
        pencilManager = FindObjectOfType<PencilManager>();
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

    public void UpdateTargetColor(Vector2Int pixelPosition)
    {
        if (IsPixelValid(pixelPosition))
        {
            pencilManager.targetColor = texture.GetPixel(pixelPosition.x, pixelPosition.y);
        }
    }

    private void Update()
    {
        HandleTouchInput();

        if (spriteRenderer == null)
            return;

        // Tek dokunmada boyama yap
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended && !isDragging)
        {
            // UI üzerinde olup olmadýðýmýzý kontrol edelim
            if (IsPointerOverUI()) return;

            Vector2 touchPosition = Input.GetTouch(0).position;
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(touchPosition);
            Vector2Int pixelPosition = WorldToPixel(worldPosition);

            UpdateTargetColor(pixelPosition);
            FloodFillArea(pixelPosition, pencilManager.fillColor);
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
                isDragging = false;
            }

            // Eðer dokunma hareket ettiriliyorsa sürükleme moduna geçiyoruz
            if (touch.phase == TouchPhase.Moved && IsImageZoomed())
            {
                Vector3 direction = touchStart - mainCamera.ScreenToWorldPoint(touch.position);
                mainCamera.transform.position += direction * pencilManager.dragSpeed;
                isDragging = true;
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

        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
        Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

        float prevMagnitude = (touch1PrevPos - touch2PrevPos).magnitude;
        float currentMagnitude = (touch1.position - touch2.position).magnitude;

        float difference = currentMagnitude - prevMagnitude;
        Zoom(difference * pencilManager.zoomSpeed);
    }

    private bool IsImageZoomed()
    {
        return transform.localScale.x > originalScale.x || transform.localScale.y > originalScale.y;
    }

    private void Zoom(float increment)
    {
        Vector3 newScale = transform.localScale + Vector3.one * increment;
        newScale.x = Mathf.Clamp(newScale.x, originalScale.x, originalScale.x * pencilManager.maxZoom);
        newScale.y = Mathf.Clamp(newScale.y, originalScale.y, originalScale.y * pencilManager.maxZoom);

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

        camPos.x = Mathf.Clamp(camPos.x, minX + halfWidth, maxX - halfWidth);
        camPos.y = Mathf.Clamp(camPos.y, minY + halfHeight, maxY - halfHeight);
        mainCamera.transform.position = camPos;
    }

    private void CalculateBounds()
    {
        float vertExtent = mainCamera.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        float spriteWidth = spriteRenderer.bounds.size.x * transform.localScale.x;
        float spriteHeight = spriteRenderer.bounds.size.y * transform.localScale.y;

        minX = spriteRenderer.bounds.min.x + horzExtent - spriteWidth / 2f;
        maxX = spriteRenderer.bounds.max.x - horzExtent + spriteWidth / 2f;
        minY = spriteRenderer.bounds.min.y + vertExtent - spriteHeight / 2f;
        maxY = spriteRenderer.bounds.max.y - vertExtent + spriteHeight / 2f;
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

        if (!IsSimilarColor(startColor, pencilManager.targetColor) || originalBoundaryPixels.Contains(startPoint))
            return;

        HashSet<Vector2Int> visitedPixels = new HashSet<Vector2Int> { startPoint };

        Vector2Int[] directions = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

        while (pixels.Count > 0)
        {
            Vector2Int currentPixel = pixels.Dequeue();
            int x = currentPixel.x;
            int y = currentPixel.y;

            if (x < 0 || x >= width || y < 0 || y >= height || originalBoundaryPixels.Contains(currentPixel))
                continue;

            int index = y * width + x;
            Color currentColor = pixelArray[index];

            if (!IsSimilarColor(currentColor, pencilManager.targetColor))
                continue;

            pixelArray[index] = newColor;

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

        texture.SetPixels(pixelArray);
        texture.Apply();
    }

    private bool IsSimilarColor(Color c1, Color c2)
    {
        float tolerance = pencilManager.colorTolerance;
        return Mathf.Abs(c1.r - c2.r) < tolerance &&
               Mathf.Abs(c1.g - c2.g) < tolerance &&
               Mathf.Abs(c1.b - c2.b) < tolerance;
    }

    private Vector2Int WorldToPixel(Vector2 worldPosition)
    {
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
        return EventSystem.current.IsPointerOverGameObject();
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
                if (IsSimilarColor(pixelColor, pencilManager.boundaryColor))
                {
                    originalBoundaryPixels.Add(new Vector2Int(x, y));
                }
            }
        }
    }
}
