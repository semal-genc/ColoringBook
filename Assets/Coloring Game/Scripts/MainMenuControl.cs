using System.Collections.Generic;
using UnityEngine;

public class MainMenuControl : MonoBehaviour
{
    [SerializeField] private GameObject pencilUI;
    [SerializeField] private GameObject menuUI;

    // Tüm karakter nesnelerini listeye alýyoruz
    [SerializeField] private List<GameObject> characters;

    // Ýndeks numarasýna göre karakter seçimi yap
    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Count)
        {
            Debug.LogError("Geçersiz karakter indeksi!");
            return;
        }

        pencilUI.SetActive(true);
        menuUI.SetActive(false);

        for (int i = 0; i < characters.Count; i++)
        {
            characters[i].SetActive(i == characterIndex);
        }
    }

    // Menüye geri dön
    public void Back()
    {
        pencilUI.SetActive(false);
        menuUI.SetActive(true);

        foreach (GameObject character in characters)
        {
            character.SetActive(false);
        }
    }
}
