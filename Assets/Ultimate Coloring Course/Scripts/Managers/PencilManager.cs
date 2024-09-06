using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PencilManager : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private PencilContainer pencilContainerPrefab;
    [SerializeField] private Transform pencilContainersParent;

    [Header("Settings")]
    [SerializeField] private Color[] colors;
    PencilContainer previousPencilContainer;

    // Start is called before the first frame update
    void Start()
    {
        CreatePencils();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreatePencils()
    {
        for (int i = 0; i < colors.Length; i++)
        {
            CreatePencil(colors[i]);
        }
    }

    void CreatePencil(Color color)
    {
        PencilContainer pencilContainerInstance = Instantiate(pencilContainerPrefab, pencilContainersParent);
        pencilContainerInstance.Configure(color,this);
    }

    public void PencilContainerClickedCallback(PencilContainer pencilContainer)
    {
        if (previousPencilContainer != null && previousPencilContainer == pencilContainer)
        {
            return;
        }

        pencilContainer.Select();

        if (previousPencilContainer != null)
        {
            previousPencilContainer.UnSelect();
        }

        previousPencilContainer = pencilContainer;

        GPUSpriteBrush.Instance.SetColor(pencilContainer.GetColor());
    }
}
