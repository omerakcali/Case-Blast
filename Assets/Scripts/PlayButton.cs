using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
    [SerializeField] private LevelLibrary Levels;
    [SerializeField] private Button Button;
    [SerializeField] private TextMeshProUGUI Text;
    
    private bool _completed;

    private void Awake()
    {
        Refresh();
    }

    private void Start()
    {
        Button.onClick.AddListener(OnClick);
    }

    
    private void Refresh()
    {
        int currentLevel = PlayerPrefs.GetInt("LevelIndex",0);

        if (currentLevel >= Levels.Levels.Count - 1)
        {
            SetCompleteMode();
            return;
        }
        
        Text.SetText($"Level {currentLevel+1}");
    }

    private void SetCompleteMode()
    {
        _completed = true;
        Text.SetText("Completed");
    }

    private void OnClick()
    {
        if(_completed) return;
        SceneLoadManager.Instance.LoadGameScene();
    }
}



