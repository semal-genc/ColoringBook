using System.Collections.Generic;
using UnityEngine;

public class FloodFill : MonoBehaviour
{
    [SerializeField] Texture2D texture;            // Texture dosyas�
    [SerializeField] Color fillColor = Color.red;  // Boyanacak renk
    [SerializeField] Color boundaryColor = Color.black;  // S�n�r rengi (siyah)
    [SerializeField] Color targetColor = Color.white;    // Boyanacak alan�n rengi (beyaz)
    [SerializeField] float colorTolerance = 0.55f;        // Daha k���k bir renk tolerans�

    private SpriteRenderer spriteRenderer;
    private HashSet<Vector2Int> visitedPixels = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> originalBoundaryPixels = new HashSet<Vector2Int>();

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("GameObject'te SpriteRenderer bile�eni bulunamad�.");
            return;
        }

        // Orijinal s�n�r piksellerini kaydet
        StoreOriginalBoundaryPixels();
    }

    private void StoreOriginalBoundaryPixels()
    {
        // Texture boyutlar�n� al
        int width = texture.width;
        int height = texture.height;

        // T�m pikselleri tarayarak siyah s�n�r olanlar� kaydet
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

    private void Update()
    {
        if (spriteRenderer == null) return;

        // UI'ya t�klan�p t�klanmad���n� kontrol et
        if (IsPointerOverUI())
        {
            return; // E�er UI'ya t�kland�ysa, boyama i�lemini yapma
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Fare pozisyonunu al
            Vector2 mousePosition = Input.mousePosition;
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            Vector2Int pixelPosition = WorldToPixel(worldPosition);

            // Piksel ge�erli mi diye kontrol et
            if (IsPixelValid(pixelPosition))
            {
                // T�klanan renk
                Color clickedColor = texture.GetPixel(pixelPosition.x, pixelPosition.y);
                targetColor = clickedColor; // targetColor'� g�ncelle

                // Flood fill algoritmas�n� ba�lat
                FloodFillArea(pixelPosition, fillColor);
            }
        }
    }

    public void SetFillColor(Color newColor)
    {
        fillColor = newColor;
    }

    void FloodFillArea(Vector2Int startPoint, Color newColor)
    {
        if (texture == null)
        {
            Debug.LogError("Texture belirtilmemi�.");
            return;
        }

        int width = texture.width;
        int height = texture.height;

        Stack<Vector2Int> pixels = new Stack<Vector2Int>();
        pixels.Push(startPoint);

        Color startColor = texture.GetPixel(startPoint.x, startPoint.y);

        // Ba�lang�� rengi hedef renge benzemiyor veya ba�lang�� pikselleri s�n�r olarak tan�mlanm��sa i�lemi sonland�r
        if (!IsSimilarColor(startColor, targetColor) || originalBoundaryPixels.Contains(startPoint))
            return;

        visitedPixels.Clear();

        Color[] pixelArray = texture.GetPixels();
        Vector2Int[] directions = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

        while (pixels.Count > 0)
        {
            Vector2Int currentPixel = pixels.Pop();
            int x = currentPixel.x;
            int y = currentPixel.y;

            // Pikselin s�n�rlar i�inde olup olmad���n� kontrol et
            if (x < 0 || x >= width || y < 0 || y >= height)
                continue;

            // Piksel daha �nce ziyaret edildiyse devam etme
            if (visitedPixels.Contains(currentPixel))
                continue;

            int index = y * width + x;
            Color currentColor = pixelArray[index];

            // E�er piksel orijinal s�n�r rengindeyse, de�i�iklik yapma
            if (originalBoundaryPixels.Contains(currentPixel))
                continue;

            // E�er renk targetColor'a benziyorsa, o zaman boyayal�m
            if (IsSimilarColor(currentColor, targetColor))
            {
                // Bu pikseli yeni renkle boya
                pixelArray[index] = newColor;

                // Ziyaret edilen piksellere ekle
                visitedPixels.Add(currentPixel);

                // Kom�u pikselleri stack'e ekle
                foreach (var dir in directions)
                {
                    Vector2Int neighbor = currentPixel + dir;
                    pixels.Push(neighbor);
                }
            }
        }

        // T�m pikselleri tek seferde uygula
        texture.SetPixels(pixelArray);
        texture.Apply();
    }



    bool IsSimilarColor(Color c1, Color c2)
    {
        return Mathf.Abs(c1.r - c2.r) < colorTolerance &&
               Mathf.Abs(c1.g - c2.g) < colorTolerance &&
               Mathf.Abs(c1.b - c2.b) < colorTolerance;
    }

    Vector2Int WorldToPixel(Vector2 worldPosition)
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer bile�eni bulunamad�.");
            return Vector2Int.zero;
        }

        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        float ppu = spriteRenderer.sprite.pixelsPerUnit;
        int x = Mathf.FloorToInt(localPos.x * ppu + texture.width / 2f);
        int y = Mathf.FloorToInt(localPos.y * ppu + texture.height / 2f);
        return new Vector2Int(x, y);
    }

    bool IsPixelValid(Vector2Int pixelPosition)
    {
        return pixelPosition.x >= 0 && pixelPosition.x < texture.width &&
               pixelPosition.y >= 0 && pixelPosition.y < texture.height;
    }

    private bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

}
