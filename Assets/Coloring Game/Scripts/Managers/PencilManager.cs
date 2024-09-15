using System.Collections.Generic;
using UnityEngine;

public class PencilManager : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private PencilContainer pencilContainerPrefab;
    [SerializeField] private Transform pencilContainersParent;
    [SerializeField] private FloodFill floodFill;

    [Header("Settings")]
    [SerializeField] private Color[] colors;
    [SerializeField] private Color[] kirmizi;
    [SerializeField] private Color[] pembe;
    [SerializeField] private Color[] sari;
    [SerializeField] private Color[] turuncu;
    [SerializeField] private Color[] yesil;
    [SerializeField] private Color[] kMavi;
    [SerializeField] private Color[] mor;
    [SerializeField] private Color[] mavi;
    [SerializeField] private Color[] gri;

    private List<PencilContainer> createdPencilContainers = new List<PencilContainer>();
    private List<PencilContainer> activePencilContainers = new List<PencilContainer>();
    private PencilContainer previousPencilContainer;

    // Start is called before the first frame update
    void Start()
    {
        floodFill = FindObjectOfType<FloodFill>();
        ShowPencils(colors);
    }

    public void AllPencils()
    {
        ShowPencils(colors);
    }
    
    public void KirmiziPencils()
    {
        ShowPencils(kirmizi);
    }

    public void PembePencils()
    {
        ShowPencils(pembe);
    }
    
    public void SariPencils()
    {
        ShowPencils(sari);
    }
    
    public void TuruncuPencils()
    {
        ShowPencils(turuncu);
    }

    public void YesilPencils()
    {
        ShowPencils(yesil);
    }
    
    public void KMaviPencils()
    {
        ShowPencils(kMavi);
    }
    
    public void MorPencils()
    {
        ShowPencils(mor);
    }

    public void MaviPencils()
    {
        ShowPencils(mavi);
    }
    
    public void GriPencils()
    {
        ShowPencils(gri);
    }

    private void Update()
    {
        if (!floodFill)
        {
            floodFill = FindObjectOfType<FloodFill>();
        }
    }

    private void ShowPencils(Color[] colorsToShow)
    {
        // Hide all existing pencil containers
        HideAllPencils();

        // Create and show the selected pencils
        activePencilContainers.Clear();
        foreach (Color color in colorsToShow)
        {
            PencilContainer pencilContainer = CreatePencil(color);
            activePencilContainers.Add(pencilContainer);
        }

        // Show only the active pencils
        foreach (var container in activePencilContainers)
        {
            container.gameObject.SetActive(true);
        }
    }

    private void HideAllPencils()
    {
        foreach (var container in createdPencilContainers)
        {
            container.gameObject.SetActive(false);
        }
    }

    private PencilContainer CreatePencil(Color color)
    {
        PencilContainer pencilContainerInstance = Instantiate(pencilContainerPrefab, pencilContainersParent);
        pencilContainerInstance.Configure(color, this);
        createdPencilContainers.Add(pencilContainerInstance);
        return pencilContainerInstance;
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

        if (floodFill != null)
        {
            floodFill.SetFillColor(pencilContainer.GetColor());
        }
    }
}
