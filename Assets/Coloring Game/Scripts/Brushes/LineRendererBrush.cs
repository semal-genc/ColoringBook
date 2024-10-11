using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public class LineRendererBrush : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] LineRenderer linePrefab;
    LineRenderer currentLineRenderer;

    [Header("Settings")]
    [SerializeField] private Color color;
    Vector2 lastClikedPosition;
    [SerializeField] float distanceThreshold;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0) 
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began) 
            {
                CreateLine(); 
            }
            else if (touch.phase == TouchPhase.Moved) 
            {
                PaintOnCanvas(); 
            }
        }
    }


    void CreateLine()
    {
        RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity);

        if (hit.collider == null)
        {
            return;
        }

        currentLineRenderer = Instantiate(linePrefab, Vector3.zero, Quaternion.identity, transform);
        currentLineRenderer.SetPosition(0, hit.point);

        currentLineRenderer.startColor = color;
        currentLineRenderer.endColor = currentLineRenderer.startColor;

        lastClikedPosition = Input.mousePosition;
    }

    void PaintOnCanvas()
    {
        if (currentLineRenderer == null)
        {
            return;
        }

        if (Vector2.Distance(lastClikedPosition, Input.mousePosition) < distanceThreshold)
        {
            return;
        }

        lastClikedPosition = Input.mousePosition;

        RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity);

        if (hit.collider == null)
        {
            return;
        }

        AddPoint(hit.point);
    }

    void AddPoint(Vector3 wordPos)
    {
        currentLineRenderer.positionCount++;
        currentLineRenderer.SetPosition(currentLineRenderer.positionCount - 1, wordPos);
    }
}
