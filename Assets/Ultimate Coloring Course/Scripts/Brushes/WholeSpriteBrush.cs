using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WholeSpriteBrush : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Color color;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastSprites();
        }
    }

    void RaycastSprite()
    {
        Vector2 origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = Vector2.zero;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction);

        if (!hit.collider)
        {
            return;
        }

        ColorSprite(hit.collider);
        Debug.Log(hit.collider.name);
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
        ColorSprite(highestCollider);
    }

    void ColorSprite(Collider2D collider)
    {
        SpriteRenderer spriteRenderer = collider.GetComponent<SpriteRenderer>();

        if (!spriteRenderer) 
        {
            return;
        }

        spriteRenderer.color = color;
    }
}
