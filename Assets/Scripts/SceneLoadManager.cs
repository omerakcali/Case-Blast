using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    public static SceneLoadManager Instance;
    private const int GameScene = 1;
    private const int MenuScene = 0;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene(GameScene);
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene(MenuScene);
    }
}
