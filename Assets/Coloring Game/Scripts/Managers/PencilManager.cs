using System.Collections.Generic;
using UnityEngine;

public class PencilManager : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private PencilContainer pencilContainerPrefab;
    [SerializeField] private Transform pencilContainersParent;
    [SerializeField] private List<FloodFill> floodFillObjects; // Birden fazla FloodFill objesi
    private FloodFill activeFloodFill;

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

    void Start()
    {
        SetActiveFloodFill(); // Baþlangýçta aktif FloodFill'i bulalým
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
        if (!activeFloodFill || !activeFloodFill.gameObject.activeInHierarchy)
        {
            // Eðer aktif FloodFill objesi devre dýþýysa baþka bir aktif FloodFill objesi seç
            SetActiveFloodFill();
        }
    }

    private void SetActiveFloodFill()
    {
        foreach (var floodFill in floodFillObjects)
        {
            if (floodFill.gameObject.activeInHierarchy)
            {
                activeFloodFill = floodFill;
                break; // Ýlk aktif objeyi bulduðumuzda döngüden çýkarýz
            }
        }

        if (!activeFloodFill)
        {
            Debug.LogError("Aktif FloodFill objesi bulunamadý!");
        }
    }

    private void ShowPencils(Color[] colorsToShow)
    {
        HideAllPencils();

        activePencilContainers.Clear();
        foreach (Color color in colorsToShow)
        {
            PencilContainer pencilContainer = CreatePencil(color);
            activePencilContainers.Add(pencilContainer);
        }

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

        if (activeFloodFill != null)
        {
            activeFloodFill.SetFillColor(pencilContainer.GetColor());
        }
    }
}
