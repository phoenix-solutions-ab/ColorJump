using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public string Level;
    public int Order;

    public string AdToUnlock;

    [InspectorName("Is unlocked")]
    public bool isUnlocked;
    // Start is called before the first frame update
    void Start()
    {
        InitUnlocked();
    }

    public void InitUnlocked()
    {
        if (!isUnlocked)
        {
            isUnlocked = PlayerPrefs.GetInt("UNLOCKED_" + Level, 0) > 0;
        }
    }

    public void SelectButton()
    {
        if(isUnlocked)
        {
            LevelManager.instance.SetSelectedButton(GetComponent<Button>());
        }
        else
        {
            AdManager.instance.RequestRewardBasedVideo(AdToUnlock);
        }
    }
}
