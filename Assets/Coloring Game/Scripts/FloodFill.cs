using System.Collections.Generic;
using UnityEngine;

public class FloodFill : MonoBehaviour
{
    [SerializeField] Texture2D texture;            // Texture dosyasý
    [SerializeField] Color fillColor = Color.red;  // Boyanacak renk
    [SerializeField] Color boundaryColor = Color.black;  // Sýnýr rengi (siyah)
    [SerializeField] Color targetColor = Color.white;    // Boyanacak alanýn rengi (beyaz)
    [SerializeField] float colorTolerance = 0.55f;        // Daha küçük bir renk toleransý

    private SpriteRenderer spriteRenderer;
    private HashSet<Vector2Int> visitedPixels = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> originalBoundaryPixels = new HashSet<Vector2Int>();

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("GameObject'te SpriteRenderer bileþeni bulunamadý.");
            return;
        }

        // Orijinal sýnýr piksellerini kaydet
        StoreOriginalBoundaryPixels();
    }

    private void StoreOriginalBoundaryPixels()
    {
        // Texture boyutlarýný al
        int width = texture.width;
        int height = texture.height;

        // Tüm pikselleri tarayarak siyah sýnýr olanlarý kaydet
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

        // UI'ya týklanýp týklanmadýðýný kontrol et
        if (IsPointerOverUI())
        {
            return; // Eðer UI'ya týklandýysa, boyama iþlemini yapma
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Fare pozisyonunu al
            Vector2 mousePosition = Input.mousePosition;
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            Vector2Int pixelPosition = WorldToPixel(worldPosition);

            // Piksel geçerli mi diye kontrol et
            if (IsPixelValid(pixelPosition))
            {
                // Týklanan renk
                Color clickedColor = texture.GetPixel(pixelPosition.x, pixelPosition.y);
                targetColor = clickedColor; // targetColor'ý güncelle

                // Flood fill algoritmasýný baþlat
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
            Debug.LogError("Texture belirtilmemiþ.");
            return;
        }

        int width = texture.width;
        int height = texture.height;

        Stack<Vector2Int> pixels = new Stack<Vector2Int>();
        pixels.Push(startPoint);

        Color startColor = texture.GetPixel(startPoint.x, startPoint.y);

        // Baþlangýç rengi hedef renge benzemiyor veya baþlangýç pikselleri sýnýr olarak tanýmlanmýþsa iþlemi sonlandýr
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

            // Pikselin sýnýrlar içinde olup olmadýðýný kontrol et
            if (x < 0 || x >= width || y < 0 || y >= height)
                continue;

            // Piksel daha önce ziyaret edildiyse devam etme
            if (visitedPixels.Contains(currentPixel))
                continue;

            int index = y * width + x;
            Color currentColor = pixelArray[index];

            // Eðer piksel orijinal sýnýr rengindeyse, deðiþiklik yapma
            if (originalBoundaryPixels.Contains(currentPixel))
                continue;

            // Eðer renk targetColor'a benziyorsa, o zaman boyayalým
            if (IsSimilarColor(currentColor, targetColor))
            {
                // Bu pikseli yeni renkle boya
                pixelArray[index] = newColor;

                // Ziyaret edilen piksellere ekle
                visitedPixels.Add(currentPixel);

                // Komþu pikselleri stack'e ekle
                foreach (var dir in directions)
                {
                    Vector2Int neighbor = currentPixel + dir;
                    pixels.Push(neighbor);
                }
            }
        }

        // Tüm pikselleri tek seferde uygula
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
            Debug.LogError("SpriteRenderer bileþeni bulunamadý.");
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
