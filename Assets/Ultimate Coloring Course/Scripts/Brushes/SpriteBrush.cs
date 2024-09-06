using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBrush : MonoBehaviour
{
    [Header("Elements")]
    SpriteRenderer currentSpriteRenderer;
    
    [Header("Settings")]
    [SerializeField] private Color color;
    [SerializeField] private int brushSize;
    Dictionary<int, Texture2D> originalTextures = new Dictionary<int, Texture2D>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastSprites();
        }
        else if (Input.GetMouseButton(0))
        {
            RaycastCurrentSprite();
        }
    }

    void RaycastSprites()
    {
        Vector2 origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = Vector2.zero;
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction);

        if (hits.Length <= 0)
        {
            return;
        }

        int higestOrderInLayer = -1000;
        int topIndex = -1;

        for (int i = 0; i < hits.Length; i++)
        {
            SpriteRenderer spriteRenderer = hits[i].collider.GetComponent<SpriteRenderer>();
            if (!spriteRenderer)
            {
                continue;
            }

            int orderInLayer = spriteRenderer.sortingOrder;

            if (orderInLayer > higestOrderInLayer)
            {
                higestOrderInLayer = orderInLayer;
                topIndex = i;
            }
        }

        Collider2D highestCollider = hits[topIndex].collider;

        currentSpriteRenderer = highestCollider.GetComponent<SpriteRenderer>();

        int key = currentSpriteRenderer.transform.GetSiblingIndex();

        if (!originalTextures.ContainsKey(key))
        {
            originalTextures.Add(key, currentSpriteRenderer.sprite.texture);
        }

        ColorSpriteAtPosition(highestCollider, hits[topIndex].point);
    }

    void RaycastCurrentSprite()
    {
        Vector2 origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = Vector2.zero;

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction);

        if (hits.Length <= 0) { return; }

        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i].collider.TryGetComponent(out SpriteRenderer spriteRenderer)) 
            {
                continue;
            }
            if (spriteRenderer == currentSpriteRenderer) 
            {
                ColorSpriteAtPosition(hits[i].collider, hits[i].point);
                break;
            }
        }
    }

    void ColorSpriteAtPosition(Collider2D collider, Vector2 hitPoint)
    {
        SpriteRenderer spriteRenderer = collider.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            return;
        }

        Vector2 texturePoint = WorldToTexturePoint(spriteRenderer, hitPoint);

        Sprite sprite = spriteRenderer.sprite;

        int key = currentSpriteRenderer.transform.GetSiblingIndex();
        Texture2D originalTexture=originalTextures[key];

        Texture2D tex = new Texture2D(sprite.texture.width, sprite.texture.height);
        Graphics.CopyTexture(sprite.texture, tex);


        /*
         * Squarry Brush
        for (int x = -brushSize / 2; x < brushSize / 2; x++)
        {
            for (int y = -brushSize / 2; y < brushSize / 2; y++)
            {
                int pixelX = x + (int)texturePoint.x;
                int pixelY = y + (int)texturePoint.y;

                Color pixelColor = color;
                pixelColor.a = sprite.texture.GetPixel(pixelX, pixelY).a;

                pixelColor = pixelColor * originalTexture.GetPixel(pixelX, pixelY);

                tex.SetPixel(pixelX, pixelY, pixelColor);
            }
        }
        */


        /*
         * Rounded Pixelated Brush
        for (int x = -brushSize / 2; x < brushSize / 2; x++)
        {
            for (int y = -brushSize / 2; y < brushSize / 2; y++)
            {
                int pixelX = x + (int)texturePoint.x;
                int pixelY = y + (int)texturePoint.y;

                Vector2 pixelPoint = new Vector2(pixelX, pixelY);

                if (Vector2.Distance(pixelPoint, texturePoint) > brushSize / 2) 
                {
                    continue;
                }

                Color pixelColor = color;
                pixelColor.a = sprite.texture.GetPixel(pixelX, pixelY).a;

                pixelColor = pixelColor * originalTexture.GetPixel(pixelX, pixelY);

                tex.SetPixel(pixelX, pixelY, pixelColor);
            }
        }
        */


        // Rounded Smooth Brush
        for (int x = -brushSize / 2; x < brushSize / 2; x++)
        {
            for (int y = -brushSize / 2; y < brushSize / 2; y++)
            {
                int pixelX = x + (int)texturePoint.x;
                int pixelY = y + (int)texturePoint.y;

                float squaredRadius = x * x + y * y;
                float factor = Mathf.Exp(-squaredRadius / brushSize);

                Color previousColor=sprite.texture.GetPixel(pixelX, pixelY);
                Color pixelColor=Color.Lerp(previousColor, color, factor);

                pixelColor.a = previousColor.a;
                pixelColor *= originalTexture.GetPixel(pixelX, pixelY);

                tex.SetPixel(pixelX, pixelY, pixelColor);
            }
        }


        tex.Apply();

        Sprite newSprite = Sprite.Create(tex, sprite.rect, Vector2.one / 2, sprite.pixelsPerUnit);
        spriteRenderer.sprite = newSprite;
    }

    Vector2 WorldToTexturePoint(SpriteRenderer sr, Vector2 worldPos)
    {
        Vector2 texturePoint = sr.transform.InverseTransformPoint(worldPos);

        texturePoint.x /= sr.bounds.size.x;
        texturePoint.y /= sr.bounds.size.y;

        texturePoint += Vector2.one / 2;

        texturePoint.x *= sr.sprite.rect.width;
        texturePoint.y *= sr.sprite.rect.height;

        texturePoint.x += sr.sprite.rect.x;
        texturePoint.y += sr.sprite.rect.y;

        return texturePoint;
    }

    void ColorSprite(Collider2D collider)
    {
        SpriteRenderer spriteRenderer = collider.GetComponent<SpriteRenderer>();

        if (!spriteRenderer)
        {
            return;
        }

        //Texture2D tex = spriteRenderer.sprite.texture;
        Sprite sprite = spriteRenderer.sprite;

        Texture2D tex = new Texture2D(sprite.texture.width, sprite.texture.height);

        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                Color pixelColor = color;
                pixelColor.a = sprite.texture.GetPixel(x, y).a;

                pixelColor = pixelColor * sprite.texture.GetPixel(x, y);

                tex.SetPixel(x, y, pixelColor);
            }
        }
        
        tex.Apply();

        Sprite newSprite = Sprite.Create(tex, sprite.rect, Vector2.one / 2, sprite.pixelsPerUnit);
        spriteRenderer.sprite = newSprite;
    }
}
