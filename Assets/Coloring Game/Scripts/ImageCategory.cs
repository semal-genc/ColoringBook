using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageCategory : MonoBehaviour
{
    public enum Category
    {
        All,
        Hayvanlar,
        Doga,
        Hayat,
        Diger
    }

    [SerializeField] private Image[] allImages;
    [SerializeField] private Image[] hayvanlarImages;
    [SerializeField] private Image[] dogaImages;
    [SerializeField] private Image[] hayatImages;
    [SerializeField] private Image[] digerImages;

    private Dictionary<Category, Image[]> categoryImages;

    private void Awake()
    {
        // Dictionary'yi baþlat
        categoryImages = new Dictionary<Category, Image[]>
        {
            { Category.All, allImages },
            { Category.Hayvanlar, hayvanlarImages },
            { Category.Doga, dogaImages },
            { Category.Hayat, hayatImages },
            { Category.Diger, digerImages }
        };
    }

    private void DeactivateAllImages()
    {
        foreach (var category in categoryImages.Values)
        {
            foreach (var img in category)
            {
                img.gameObject.SetActive(false);
            }
        }
    }

    public void ShowImages(Category category)
    {
        DeactivateAllImages();
        if (categoryImages.TryGetValue(category, out var images))
        {
            foreach (var img in images)
            {
                img.gameObject.SetActive(true);
            }
        }
    }

    // Butona týklama olayýný baðlamak
    public void OnAllButtonClicked()
    {
        ShowImages(Category.All);
    }

    public void OnHayvanlarButtonClicked()
    {
        ShowImages(Category.Hayvanlar);
    }

    public void OnDoðaButtonClicked()
    {
        ShowImages(Category.Doga);
    }

    public void OnHayatButtonClicked()
    {
        ShowImages(Category.Hayat);
    }

    public void OnDigerButtonClicked()
    {
        ShowImages(Category.Diger);
    }

}
