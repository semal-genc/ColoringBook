using System.Collections.Generic;
using UnityEngine;

public class MainMenuControl : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject pencilUI;
    [SerializeField] private GameObject menuUI;

    // T�m karakter nesnelerini listeye al�yoruz
    CharacterManager characterManager;

    private void Start()
    {
        characterManager=FindObjectOfType<CharacterManager>();
    }

    // �ndeks numaras�na g�re karakter se�imi yap
    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characterManager.characters.Count)
        {
            Debug.LogError("Ge�ersiz karakter indeksi!");
            return;
        }

        pencilUI.SetActive(true);
        menuUI.SetActive(false);

        for (int i = 0; i < characterManager.characters.Count; i++)
        {
            characterManager.characters[i].SetActive(i == characterIndex);
        }
    }

    // Men�ye geri d�n
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
