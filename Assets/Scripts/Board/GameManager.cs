using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static event Action MoveMadeEvent;
    public static event Action BoardPlayableEvent;
    public static event Action<bool> LevelFinishEvent;
    [SerializeField]
    private LevelLibrary LevelLibrary;
    [SerializeField]
    private Board Board;

    private int2? _mouseDownCoordinate;

    public int CurrentMoveCount { get; private set; }
    public int CurrentLevelIndex { get; private set; }
    public LevelInfo CurrentLevel => LevelLibrary.Levels[CurrentLevelIndex];
    private bool _playableStateDirty=true;

    private void Awake()
    {
        CurrentLevelIndex = PlayerPrefs.GetInt("LevelIndex", 0);
    }

    private void Start()
    {
        LoadCurrentLevel();
    }

    void Update ()
    {
        if (Board.IsPlaying)
        {
            if (Board.CanPlay) 
            {
                if (_playableStateDirty)
                {
                    _playableStateDirty = false;
                    BoardPlayableEvent?.Invoke();
                }
                HandleInput();
            }
            else
            {
                _playableStateDirty = true;
            }
            Board.DoWork();
        }
        /*else if (Input.GetKeyDown(KeyCode.Space))
        {
            LoadCurrentLevel();
        }*/
    }

    public void LoadCurrentLevel()
    {
        CurrentMoveCount = CurrentLevel.MoveCount;
        Board.StartNewGame(CurrentLevel);
    }

    void HandleInput()
    {
        //not yet touched
        if (!_mouseDownCoordinate.HasValue) 
        {
            //touch started
            if(Input.GetMouseButtonDown(0))
                _mouseDownCoordinate = Board.ScreenToTileSpace(Input.mousePosition);
        }
        else
        {
            // still holding
            if (!Input.GetMouseButtonUp(0)) return; 
            
            // touch release
            var mouseUpCoordinate =  Board.ScreenToTileSpace(Input.mousePosition);
            if (_mouseDownCoordinate.Value.Equals(mouseUpCoordinate))
            {
                if (Board.AreValidCoordinates(mouseUpCoordinate.Value))
                {
                    Board[mouseUpCoordinate.Value].OnClick(MakeMove);
                }
            }
            _mouseDownCoordinate = null;
        }
    }

    private void MakeMove()
    {
        CurrentMoveCount--;
        MoveMadeEvent?.Invoke();
        StartCoroutine(CheckMoveCount());
    }

    private IEnumerator CheckMoveCount()
    {
        yield return null;
        while (!Board.CanPlay)
        {
            yield return null;
        }
        if (Board.IsPlaying && CurrentMoveCount <= 0)
        {
            FailLevel();
        }
    }

    private void FailLevel()
    {
        Board.FinishLevel();
        LevelFinishEvent?.Invoke(false);
    }

    public void WinLevel()
    {
        CurrentLevelIndex++;
        PlayerPrefs.SetInt("LevelIndex",CurrentLevelIndex);
        StartCoroutine(WaitBoardAnimation());
    }
    
    private IEnumerator WaitBoardAnimation()
    {
        yield return null;
        while (!Board.CanPlay)
        {
            yield return null;
        }
        Board.FinishLevel();
        LevelFinishEvent?.Invoke(true);
    }
}
