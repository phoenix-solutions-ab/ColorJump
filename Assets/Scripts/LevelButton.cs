using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public string Level;
    public int Order;

    [InspectorName("Is unlocked")]
    public bool isUnlocked;
    // Start is called before the first frame update
    void Start()
    {
    }

    public void SelectButton()
    {
        if(isUnlocked)
        {
            LevelManager.instance.SetSelectedButton(GetComponent<Button>());
        }
    }
}
