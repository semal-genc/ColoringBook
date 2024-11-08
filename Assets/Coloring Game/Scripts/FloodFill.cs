using System.Collections.Generic;
using UnityEngine;

public class FloodFill : MonoBehaviour
{
    [SerializeField] private Texture2D texture;
    private HashSet<Vector2Int> originalBoundaryPixels = new HashSet<Vector2Int>();
    public SpriteRenderer spriteRenderer;
    private PencilManager pencilManager;

    private void Start()
    {
        pencilManager = FindObjectOfType<PencilManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        StoreOriginalBoundaryPixels();
    }

    public void HandleTouchEnd(Vector2 worldPosition)
    {
        Vector2Int pixelPosition = WorldToPixel(worldPosition);
        UpdateTargetColor(pixelPosition);
        FloodFillArea(pixelPosition, pencilManager.fillColor);
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

        if (IsSimilarColor(startColor, newColor, pencilManager.colorToleranceForFloodFill) || originalBoundaryPixels.Contains(startPoint))
        {
            Debug.Log($"Flood fill canceled: Start color {startColor} is similar to new color {newColor} or it's a boundary pixel.");
            return;
        }

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

            if (IsSimilarColor(currentColor, pencilManager.boundaryColor, pencilManager.colorTolerance) || IsNearBoundaryPixel(currentPixel))
            {
                continue; 
            }

            // Yeni rengi uygula
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




        // Flood fill algoritmasý buraya gelir.
        // ...
    }

    private bool IsNearBoundaryPixel(Vector2Int pixel)
    {
        Vector2Int[] directions = {
        Vector2Int.right,  // Sað
        Vector2Int.left,   // Sol
        Vector2Int.up,     // Üst
        Vector2Int.down    // Alt
    };

        // Komþu pikselleri kontrol et
        foreach (var dir in directions)
        {
            Vector2Int neighbor = pixel + dir; 

            if (originalBoundaryPixels.Contains(neighbor))
            {
                return true; 
            }
        }

        return false;
    }

    private Vector2Int WorldToPixel(Vector2 worldPosition)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        float ppu = spriteRenderer.sprite.pixelsPerUnit;
        int x = Mathf.FloorToInt(localPos.x * ppu + texture.width / 2f);
        int y = Mathf.FloorToInt(localPos.y * ppu + texture.height / 2f);
        return new Vector2Int(x, y);
    }

    private void UpdateTargetColor(Vector2Int pixelPosition)
    {
        if (IsPixelValid(pixelPosition))
        {
            pencilManager.targetColor = texture.GetPixel(pixelPosition.x, pixelPosition.y);
        }
    }

    private bool IsPixelValid(Vector2Int pixelPosition)
    {
        return pixelPosition.x >= 0 && pixelPosition.x < texture.width &&
               pixelPosition.y >= 0 && pixelPosition.y < texture.height;
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
                if (IsSimilarColor(pixelColor, pencilManager.boundaryColor, pencilManager.colorToleranceForFloodFill))
                {
                    originalBoundaryPixels.Add(new Vector2Int(x, y));
                }
            }
        }
    }

    private bool IsSimilarColor(Color c1, Color c2, float tolerance)
    {
        bool rgbCloseEnough = Mathf.Abs(c1.r - c2.r) < tolerance &&
                              Mathf.Abs(c1.g - c2.g) < tolerance &&
                              Mathf.Abs(c1.b - c2.b) < tolerance;

        float brightness1 = (c1.r + c1.g + c1.b) / 3f;
        float brightness2 = (c2.r + c2.g + c2.b) / 3f;
        bool brightnessCloseEnough = Mathf.Abs(brightness1 - brightness2) < tolerance;

        float saturation1 = Mathf.Max(c1.r, c1.g, c1.b) - Mathf.Min(c1.r, c1.g, c1.b);
        float saturation2 = Mathf.Max(c2.r, c2.g, c2.b) - Mathf.Min(c2.r, c2.g, c2.b);
        bool saturationCloseEnough = Mathf.Abs(saturation1 - saturation2) < tolerance;

        return rgbCloseEnough && brightnessCloseEnough && saturationCloseEnough;
    }
}
