using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    private int TagSelect;
    public GameObject HouseTag, TreeTag, OtherTag;
    public GameObject HouseTagSelect, TreeTagSelect, OtherTagSelect;
    public GameObject House, Tree, Other;
    public GameObject PauseButton, PauseMenu;
    public GameObject StopDrawButton, CancelButton;
    public GameObject SettingsPage, InstructionsPage;
    public GameObject SettingsSelect, InstructionsSelect;
    public GameObject DeleteButton, QuitDeleteButton;
    public GameObject DeleteText;
    public RectTransform SelectUI;
    public RectTransform MenuUI;
    public RectTransform SettingsUI;
    public DrawPath drawPath;
    public ModelPlacer MP;
    // Start is called before the first frame update
    void Start()
    {
        PauseMenu.SetActive(false);
        TagSelect = 1;
        House.SetActive(true);
        Tree.SetActive(false);
        Other.SetActive(false);
        StopDrawButton.SetActive(false);
        CancelButton.SetActive(false);
        InstructionsPage.SetActive(false);
        QuitDeleteButton.SetActive(false);
        DeleteText.SetActive(false);
        TreeTagSelect.SetActive(false);
        OtherTagSelect.SetActive(false);
        InstructionsSelect.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (TagSelect == 1)
        {
            HouseTagSelect.SetActive(true);
            TreeTagSelect.SetActive(false);
            OtherTagSelect.SetActive(false);
        }
        else if (TagSelect == 2)
        {
            HouseTagSelect.SetActive(false);
            TreeTagSelect.SetActive(true);
            OtherTagSelect.SetActive(false);
        }
        else if (TagSelect == 3)
        {
            HouseTagSelect.SetActive(false);
            TreeTagSelect.SetActive(false);
            OtherTagSelect.SetActive(true);
        }
    }
    public void OnClickHouseTag()
    {
        TagSelect = 1;
        House.SetActive(true);
        Tree.SetActive(false);
        Other.SetActive(false);
    }
    public void OnClickTreeTag()
    {
        TagSelect = 2;
        House.SetActive(false);
        Tree.SetActive(true);
        Other.SetActive(false);
    }
    public void OnClickOtherTag()
    {
        TagSelect = 3;
        House.SetActive(false);
        Tree.SetActive(false);
        Other.SetActive(true);
    }
    public void OnClickHouse1()
    {
        MP.OnClick_Select_House1();
        CancelButton.SetActive(true);
        DeleteButton.SetActive(false);
        MoveUI();
    }
    public void OnClickHouse2()
    {
        MP.OnClick_Select_House2();
        CancelButton.SetActive(true);
        DeleteButton.SetActive(false);
        MoveUI();
    }
    public void OnClickHouse3()
    {
        MP.OnClick_Select_House3();
        CancelButton.SetActive(true);
        DeleteButton.SetActive(false);
        MoveUI();
    }
    public void OnClickHouse4()
    {
        MP.OnClick_Select_House4();
        CancelButton.SetActive(true);
        DeleteButton.SetActive(false);
        MoveUI();
    }
    public void OnClickHouse5()
    {
        MP.OnClick_Select_House5();
        CancelButton.SetActive(true);
        DeleteButton.SetActive(false);
        MoveUI();
    }
    public void OnClickTree1()
    {
        MP.OnClick_Select_Tree1();
        CancelButton.SetActive(true);
        DeleteButton.SetActive(false);
        MoveUI();
    }
    public void OnClickTree2()
    {
        MP.OnClick_Select_Tree2();
        CancelButton.SetActive(true);
        DeleteButton.SetActive(false);
        MoveUI();
    }
    public void OnClickTree3()
    {
        MP.OnClick_Select_Tree3();
        CancelButton.SetActive(true);
        DeleteButton.SetActive(false);
        MoveUI();
    }
    public void OnClickOther1()
    {
        MP.OnClick_Select_Other1();
        CancelButton.SetActive(true);
        DeleteButton.SetActive(false);
        MoveUI();
    }
    public void OnClickOther2()
    {
        MP.OnClick_Select_Other2();
        CancelButton.SetActive(true);
        DeleteButton.SetActive(false);
        MoveUI();
    }
    public void OnClickOther3()
    {
        MP.OnClick_Select_Other3();
        CancelButton.SetActive(true);
        DeleteButton.SetActive(false);
        MoveUI();
    }
    public void OnClickOther4()
    {
        MP.OnClick_Select_Other4();
        CancelButton.SetActive(true);
        DeleteButton.SetActive(false);
        MoveUI();
    }
    public void OnClickOther5()
    {
        MP.OnClick_Select_Other5();
        CancelButton.SetActive(true);
        DeleteButton.SetActive(false);
        MoveUI();
    }
    public void CancelPlacing()
    {
        MP.CancelPlacing();
        CancelButton.SetActive(false);
        DeleteButton.SetActive(true);
        MoveBackUI();
    }
    public void CompletePlacing()
    {
        CancelButton.SetActive(false);
        DeleteButton.SetActive(true);
        MoveBackUI();
    }
    public void PauseGame()
    {
        Time.timeScale = 0;
        PauseMenu.SetActive(true);
        PauseButton.SetActive(false);
    }
    public void ResumeGame()
    {
        Time.timeScale = 1;
        PauseMenu.SetActive(false);
        PauseButton.SetActive(true);
    }
    public void BackToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
    }
    public void LoadSetting()
    {
        StartCoroutine(MoveMenu());
        IEnumerator MoveMenu()
        {
            float duration = 0.5f;
            float currentTime = 0;
            Vector3 startPos = MenuUI.position;
            Vector3 endPos = new Vector3(MenuUI.position.x, MenuUI.position.y + 1000, MenuUI.position.z);
            while (currentTime <= duration)
            {
                MenuUI.position = Vector3.Lerp(startPos, endPos, currentTime / duration);
                currentTime += Time.unscaledDeltaTime;
                yield return null;
            }
            MenuUI.position = endPos;
        }
        StartCoroutine(MoveSettingsUI());
        IEnumerator MoveSettingsUI()
        {
            float duration = 0.5f;
            float currentTime = 0;
            Vector3 startPos = SettingsUI.position;
            Vector3 endPos = new Vector3(SettingsUI.position.x, SettingsUI.position.y + 1000, SettingsUI.position.z);
            while (currentTime <= duration)
            {
                SettingsUI.position = Vector3.Lerp(startPos, endPos, currentTime / duration);
                currentTime += Time.unscaledDeltaTime;
                yield return null;
            }
            SettingsUI.position = endPos;
        }
    }
    public void BackToPauseMenu()
    {
        StartCoroutine(MoveMenu());
        IEnumerator MoveMenu()
        {
            float duration = 0.5f;
            float currentTime = 0;
            Vector3 startPos = MenuUI.position;
            Vector3 endPos = new Vector3(MenuUI.position.x, MenuUI.position.y - 1000, MenuUI.position.z);
            while (currentTime <= duration)
            {
                MenuUI.position = Vector3.Lerp(startPos, endPos, currentTime / duration);
                currentTime += Time.unscaledDeltaTime;
                yield return null;
            }
            MenuUI.position = endPos;
        }
        StartCoroutine(MoveSettingsUI());
        IEnumerator MoveSettingsUI()
        {
            float duration = 0.5f;
            float currentTime = 0;
            Vector3 startPos = SettingsUI.position;
            Vector3 endPos = new Vector3(SettingsUI.position.x, SettingsUI.position.y - 1000, SettingsUI.position.z);
            while (currentTime <= duration)
            {
                SettingsUI.position = Vector3.Lerp(startPos, endPos, currentTime / duration);
                currentTime += Time.unscaledDeltaTime;
                yield return null;
            }
            SettingsUI.position = endPos;
        }
    }
    public void Settings()
    {
        SettingsPage.SetActive(true);
        InstructionsPage.SetActive(false);
        SettingsSelect.SetActive(true);
        InstructionsSelect.SetActive(false);
    }
    public void Instructions()
    {
        SettingsPage.SetActive(false);
        InstructionsPage.SetActive(true);
        SettingsSelect.SetActive(false);
        InstructionsSelect.SetActive(true);
    }
    public void StartDrawPath()
    {
        MoveUI();
        drawPath.SetState(DrawPath.State.Drawing);
        DeleteButton.SetActive(false);
        StopDrawButton.SetActive(true);
    }
    public void StopDrawPath()
    {
        MoveBackUI();
        drawPath.SetState(DrawPath.State.Disabled);
        DeleteButton.SetActive(true);
        StopDrawButton.SetActive(false);
    }
    public void StartDelete()
    {
        MP.ToggleDeleteMode();
        drawPath.SetState(DrawPath.State.Deleting);
        QuitDeleteButton.SetActive(true);
        DeleteButton.SetActive(false);
        DeleteText.SetActive(true);
        MoveUI();
    }
    public void StopDelete()
    {
        MP.ToggleDeleteMode();
        drawPath.SetState(DrawPath.State.Disabled);
        QuitDeleteButton.SetActive(false);
        DeleteButton.SetActive(true);
        DeleteText.SetActive(false);
        MoveBackUI();
    }
    public void MoveUI()
    {
        StartCoroutine(MoveSelectUI());
        IEnumerator MoveSelectUI()
        {
            float duration = 0.3f;
            float currentTime = 0;
            Vector3 startPos = SelectUI.position;
            Vector3 endPos = new Vector3(960, 0, SelectUI.position.z);
            while (currentTime <= duration)
            {
                SelectUI.position = Vector3.Lerp(startPos, endPos, currentTime / duration);
                currentTime += Time.deltaTime;
                yield return null;
            }
            SelectUI.position = endPos;
        }
    }
    public void MoveBackUI()
    {
        StartCoroutine(MoveBackSelectUI());
        IEnumerator MoveBackSelectUI()
        {
            float duration = 0.3f;
            float currentTime = 0;
            Vector3 startPos = SelectUI.position;
            Vector3 endPos = new Vector3(960, 540, SelectUI.position.z);
            while (currentTime <= duration)
            {
                SelectUI.position = Vector3.Lerp(startPos, endPos, currentTime / duration);
                currentTime += Time.deltaTime;
                yield return null;
            }
            SelectUI.position = endPos;
        }
    }
}
