using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    private Button[] levelButtons;

    public Button selectedButton;
    private Color selectedColor;
    private Color notSelectedColor;
    private Color lockedColor;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        DontDestroyOnLoad(this);
    }

    internal void SetSelectedButton(Button btn)
    {
        selectedButton = btn;

        FixColors();
    }

    // Start is called before the first frame update
    void Start()
    {
        selectedColor = new Color(0.2f, 0.5f, 1f, 255);
        notSelectedColor = Color.white;
        lockedColor = new Color(0.5f, 0.5f, 0.5f, 1);

        SceneManager.sceneLoaded += OnSceneLoaded;

        FixLevelButtons();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode1)
    {
        if (scene.name == "MainMenuLight" && (levelButtons.Length <= 0 || levelButtons[0] == null))
        {
            FixLevelButtons();
        }
    }

    private void FixLevelButtons()
    {
        var levelbtns = FindObjectsOfType<LevelButton>();
        levelButtons = new Button[levelbtns.Length];

        levelbtns = levelbtns.OrderBy(b => b.Order).ToArray();

        for (int i = 0; i < levelbtns.Length; i++)
        {
            levelButtons[i] = levelbtns[i].GetComponent<Button>();
        }

        selectedButton = levelButtons[0];

        FixColors();
    }

    private void FixColors()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if(levelButtons[i].GetComponent<LevelButton>().isUnlocked)
            {
                levelButtons[i].GetComponent<Image>().color = notSelectedColor;
            }
            else
            {
                levelButtons[i].GetComponent<Image>().color = lockedColor;
                levelButtons[i].transform.GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 0.3f);
            }
        }

        selectedButton.GetComponent<Image>().color = selectedColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
