using System.Collections.Generic;
using UnityEngine;

public class MainMenuControl : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject pencilUI;
    [SerializeField] private GameObject menuUI;

    // Tüm karakter nesnelerini listeye alýyoruz
    CharacterManager characterManager;

    private void Start()
    {
        characterManager=FindObjectOfType<CharacterManager>();
    }

    // Ýndeks numarasýna göre karakter seçimi yap
    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characterManager.characters.Count)
        {
            Debug.LogError("Geçersiz karakter indeksi!");
            return;
        }

        pencilUI.SetActive(true);
        menuUI.SetActive(false);

        for (int i = 0; i < characterManager.characters.Count; i++)
        {
            characterManager.characters[i].SetActive(i == characterIndex);
        }
    }

    // Menüye geri dön
    public void Back()
    {
        pencilUI.SetActive(false);
        menuUI.SetActive(true);

        foreach (GameObject character in characterManager.characters)
        {
            character.SetActive(false);
        }
    }
}
