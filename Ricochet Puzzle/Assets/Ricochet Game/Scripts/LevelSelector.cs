using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    public GameObject player;
    public GameObject levelMenu;
    private float distance = 1.0f;

    public Button[] buttons;
    public void createLevelMenu()
    {
        GameObject levelUI = Instantiate(levelMenu);
        levelMenu.transform.position = player.transform.position + (player.transform.forward * distance) - new Vector3(0,1f,0);
        levelMenu.transform.rotation = player.transform.rotation;

        buttons = new Button[10];
        for (int i = 0; i < 10; i++)
        {
            buttons[i] = levelUI.transform.GetChild(i).GetComponent<Button>();
        }

        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }
        for (int i = 0; i < unlockedLevel; i++)
        {
            buttons[i].interactable = true;
        }
    }

    public void openLevel(int levelNum)
    {
        SceneManager.LoadScene(levelNum);
    }

    public void UnlockNewLevel()
    {
        if (SceneManager.GetActiveScene().buildIndex >= PlayerPrefs.GetInt("ReachedIndex"))
        {
            PlayerPrefs.SetInt("ReachedIndex", SceneManager.GetActiveScene().buildIndex + 1);
            PlayerPrefs.SetInt("UnlockedLevel", PlayerPrefs.GetInt("UnlockedLevel", 1) + 1);
        }
        openLevel(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
