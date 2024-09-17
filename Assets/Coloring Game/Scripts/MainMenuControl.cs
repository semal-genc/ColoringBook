using System.Collections.Generic;
using UnityEngine;

public class MainMenuControl : MonoBehaviour
{
    [SerializeField] private GameObject pencilUI;
    [SerializeField] private GameObject menuUI;

    // T�m karakter nesnelerini listeye al�yoruz
    [SerializeField] private List<GameObject> characters;

    // �ndeks numaras�na g�re karakter se�imi yap
    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= characters.Count)
        {
            Debug.LogError("Ge�ersiz karakter indeksi!");
            return;
        }

        pencilUI.SetActive(true);
        menuUI.SetActive(false);

        for (int i = 0; i < characters.Count; i++)
        {
            characters[i].SetActive(i == characterIndex);
        }
    }

    // Men�ye geri d�n
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
