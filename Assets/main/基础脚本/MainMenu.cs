using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public RectTransform SettingsMenu;
    public RectTransform MainMenuUI;
    public AudioSource audioSource;
    public Slider volumeSlider;
    public GameObject SettingsPage, GamePage, CreditsPage, InstructionsPage;
    public GameObject SettingsSelect, GameSelect, CreditsSelect, InstructionsSelect;
    // Start is called before the first frame update
    void Start()
    {
        GamePage.SetActive(false);
        CreditsPage.SetActive(false);
        InstructionsPage.SetActive(false);
        GameSelect.SetActive(false);
        CreditsSelect.SetActive(false);
        InstructionsSelect.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetVolume()
    {
        audioSource.volume = volumeSlider.value;
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void StartGame()
    {
        SceneManager.LoadScene("TerrainScene 12");
    }
    public void LoadSettings()
    {
        StartCoroutine(MoveMainMenu());
        IEnumerator MoveMainMenu()
        {
            float duration = 0.5f;
            float currentTime = 0;
            Vector3 startPos = MainMenuUI.position;
            Vector3 endPos = new Vector3(MainMenuUI.position.x, MainMenuUI.position.y + 1000, MainMenuUI.position.z);
            while (currentTime <= duration)
            {
                MainMenuUI.position = Vector3.Lerp(startPos, endPos, currentTime / duration);
                currentTime += Time.deltaTime;
                yield return null;
            }
            MainMenuUI.position = endPos;
        }
        StartCoroutine(MoveSettingsMenu());
        IEnumerator MoveSettingsMenu()
        {
            float duration = 0.5f;
            float currentTime = 0;
            Vector3 startPos = SettingsMenu.position;
            Vector3 endPos = new Vector3(SettingsMenu.position.x, SettingsMenu.position.y + 1000, SettingsMenu.position.z);
            while (currentTime <= duration)
            {
                SettingsMenu.position = Vector3.Lerp(startPos, endPos, currentTime / duration);
                currentTime += Time.deltaTime;
                yield return null;
            }
            SettingsMenu.position = endPos;
        }
    }
    public void BackToMainMenu()
    {
        StartCoroutine(MoveMainMenuBack());
        IEnumerator MoveMainMenuBack()
        {
            float duration = 0.5f;
            float currentTime = 0;
            Vector3 startPos = MainMenuUI.position;
            Vector3 endPos = new Vector3(MainMenuUI.position.x, MainMenuUI.position.y - 1000, MainMenuUI.position.z);
            while (currentTime <= duration)
            {
                MainMenuUI.position = Vector3.Lerp(startPos, endPos, currentTime / duration);
                currentTime += Time.deltaTime;
                yield return null;
            }
            MainMenuUI.position = endPos;
        }
        StartCoroutine(MoveSettingsMenuBack());
        IEnumerator MoveSettingsMenuBack()
        {
            float duration = 0.5f;
            float currentTime = 0;
            Vector3 startPos = SettingsMenu.position;
            Vector3 endPos = new Vector3(SettingsMenu.position.x, SettingsMenu.position.y - 1000, SettingsMenu.position.z);
            while (currentTime <= duration)
            {
                SettingsMenu.position = Vector3.Lerp(startPos, endPos, currentTime / duration);
                currentTime += Time.deltaTime;
                yield return null;
            }
            SettingsMenu.position = endPos;
        }
    }
    public void GameIntroduction()
    {
        GamePage.SetActive(true);
        InstructionsPage.SetActive(false);
        CreditsPage.SetActive(false);
        SettingsPage.SetActive(false);
        GameSelect.SetActive(true);
        CreditsSelect.SetActive(false);
        InstructionsSelect.SetActive(false);
        SettingsSelect.SetActive(false);
    }
    public void Instructions()
    {
        GamePage.SetActive(false);
        InstructionsPage.SetActive(true);
        CreditsPage.SetActive(false);
        SettingsPage.SetActive(false);
        InstructionsSelect.SetActive(true);
        GameSelect.SetActive(false);
        CreditsSelect.SetActive(false);
        SettingsSelect.SetActive(false);    
    }
    public void Credits()
    {
        GamePage.SetActive(false);
        InstructionsPage.SetActive(false);
        CreditsPage.SetActive(true);
        SettingsPage.SetActive(false);
        CreditsSelect.SetActive(true);
        GameSelect.SetActive(false);
        InstructionsSelect.SetActive(false);
        SettingsSelect.SetActive(false);
    }
    public void Settings()
    {
        GamePage.SetActive(false);
        InstructionsPage.SetActive(false);
        CreditsPage.SetActive(false);
        SettingsPage.SetActive(true);
        SettingsSelect.SetActive(true);
        GameSelect.SetActive(false);
        CreditsSelect.SetActive(false);
        InstructionsSelect.SetActive(false);
    }
}
