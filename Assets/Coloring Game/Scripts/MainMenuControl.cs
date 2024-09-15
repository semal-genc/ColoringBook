using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuControl : MonoBehaviour
{
    [SerializeField] private GameObject salyangoz;
    [SerializeField] private GameObject pencilUI;
    [SerializeField] private GameObject menulUI;

    public void Salyangoz()
    {
        salyangoz.SetActive(true);
        pencilUI.SetActive(true);
        menulUI.SetActive(false);
    }
}
