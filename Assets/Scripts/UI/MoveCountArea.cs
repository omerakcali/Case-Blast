using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoveCountArea : MonoBehaviour
{
    [SerializeField] private GameManager GameManager;
    [SerializeField] private TextMeshProUGUI Text;

    private void Start()
    {
        GameManager.MoveMadeEvent += OnMoveMade;
        Board.LevelLoadEvent += OnLevelLoad;
    }

    private void OnLevelLoad(LevelInfo obj)
    {
        Refresh();
    }

    private void OnMoveMade()
    {
        Refresh();
    }

    private void Refresh()
    {
        Text.SetText(GameManager.CurrentMoveCount.ToString());
    }
}