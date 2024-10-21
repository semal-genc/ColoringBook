using System.Collections.Generic;
using UnityEngine;

public class PencilManager : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private PencilContainer pencilContainerPrefab;
    [SerializeField] private Transform pencilContainersParent;
    private CharacterManager characterManager;
    private FloodFill activeFloodFill;

    [Header("Flood Fill Color Settings")]
    public Color fillColor = Color.red;
    public Color boundaryColor = Color.black;
    public Color targetColor = Color.white;
    public float colorToleranceForFloodFill = 0.01f;
    public float colorTolerance = 0.1f;

    [Header("Flood Fill Zoom Settings")]
    public float zoomSpeed = 0.1f;
    public float maxZoom = 3.0f;
    public float dragSpeed = 0.05f;

    [Header("Color Settings")]
    [SerializeField] private Color[] kirmizi;
    [SerializeField] private Color[] pembe;
    [SerializeField] private Color[] sari;
    [SerializeField] private Color[] turuncu;
    [SerializeField] private Color[] yesil;
    [SerializeField] private Color[] kMavi;
    [SerializeField] private Color[] mor;
    [SerializeField] private Color[] mavi;
    [SerializeField] private Color[] gri;

    private Dictionary<ColorCategory, Color[]> colorCategoryMap;
    private List<PencilContainer> createdPencilContainers = new List<PencilContainer>();
    private List<PencilContainer> activePencilContainers = new List<PencilContainer>();
    private PencilContainer previousPencilContainer;

    public enum ColorCategory
    {
        All,
        Kirmizi,
        Pembe,
        Sari,
        Turuncu,
        Yesil,
        Mavi,
        Mor,
        KMavi,
        Gri
    }

    private void Start()
    {
        characterManager = FindObjectOfType<CharacterManager>();
        InitializeColorCategories();
        SetActiveFloodFill(); // Baþlangýçta aktif FloodFill'i bulalým
        ShowPencils(ColorCategory.All); // Tüm renkleri göster
    }

    private void InitializeColorCategories()
    {
        // Diðer renk kategorilerini tanýmla
        colorCategoryMap = new Dictionary<ColorCategory, Color[]>
        {
            { ColorCategory.Kirmizi, kirmizi },
            { ColorCategory.Pembe, pembe },
            { ColorCategory.Sari, sari },
            { ColorCategory.Turuncu, turuncu },
            { ColorCategory.Yesil, yesil },
            { ColorCategory.Mavi, mavi },
            { ColorCategory.Mor, mor },
            { ColorCategory.KMavi, kMavi },
            { ColorCategory.Gri, gri }
        };

        // Tüm renk kategorilerini birleþtir ve allColors kategorisine ata
        var allColorsList = new List<Color>();
        foreach (var colorArray in colorCategoryMap.Values)
        {
            allColorsList.AddRange(colorArray); // Her renk kategorisini birleþtir
        }
        colorCategoryMap[ColorCategory.All] = allColorsList.ToArray(); // allColors kategorisini dinamik olarak oluþtur
    }

    public void ShowPencils(ColorCategory category)
    {
        if (colorCategoryMap.TryGetValue(category, out var colorsToShow))
        {
            DisplayPencils(colorsToShow);
        }
    }

    private void DisplayPencils(Color[] colorsToShow)
    {
        HideAllPencils();
        activePencilContainers.Clear();

        foreach (Color color in colorsToShow)
        {
            PencilContainer pencilContainer = CreatePencil(color);
            pencilContainer.gameObject.SetActive(true);
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

    private void Update()
    {
        if (activeFloodFill == null || !activeFloodFill.gameObject.activeInHierarchy)
        {
            SetActiveFloodFill();
        }
    }

    private void SetActiveFloodFill()
    {
        if (characterManager == null || characterManager.characters == null)
        {
            Debug.LogError("CharacterManager veya characters listesi boþ!");
            return;
        }

        // Aktif olan ilk FloodFill objesini bul
        foreach (var character in characterManager.characters)
        {
            if (character.gameObject.activeInHierarchy)
            {
                activeFloodFill = character.GetComponent<FloodFill>(); // FloodFill component'ini al
                if (activeFloodFill != null)
                {
                    return; // Ýlk aktif objeyi bulduðumuzda döngüden çýkarýz
                }
            }
        }

        activeFloodFill = null; // Aktif FloodFill bulunamadýysa sýfýrla
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
            Color selectedColor = pencilContainer.GetColor();
            fillColor = selectedColor; // Seçilen rengi fillColor olarak ayarla
        }
    }

    // Bu metodlarý butonlara baðlayarak renklere göre kalemleri gösterebilirsiniz
    public void OnAllPencilsButton() => ShowPencils(ColorCategory.All);
    public void OnKirmiziPencilsButton() => ShowPencils(ColorCategory.Kirmizi);
    public void OnPembePencilsButton() => ShowPencils(ColorCategory.Pembe);
    public void OnSariPencilsButton() => ShowPencils(ColorCategory.Sari);
    public void OnTuruncuPencilsButton() => ShowPencils(ColorCategory.Turuncu);
    public void OnYesilPencilsButton() => ShowPencils(ColorCategory.Yesil);
    public void OnMaviPencilsButton() => ShowPencils(ColorCategory.Mavi);
    public void OnMorPencilsButton() => ShowPencils(ColorCategory.Mor);
    public void OnKMaviPencilsButton() => ShowPencils(ColorCategory.KMavi);
    public void OnGriPencilsButton() => ShowPencils(ColorCategory.Gri);
}
