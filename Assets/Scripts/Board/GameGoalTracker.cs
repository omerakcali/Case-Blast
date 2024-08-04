using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGoalTracker : MonoBehaviour
{
    [SerializeField] private GameManager GameManager;
    [SerializeField] private GoalArea GoalArea;
    
    public static GameGoalTracker Instance;
    private Dictionary<BoardElementType, int> _goals;

    private void Awake()
    {
        Instance = this;
        Board.LevelLoadEvent += OnLevelLoad;
    }

    private void OnLevelLoad(LevelInfo obj)
    {
        _goals = new();
        foreach (var goal in obj.Goals)
        {
            _goals.Add(goal.GoalType,goal.Amount);
        }
    }

    public bool HasGoal(BoardElementType elementType)
    {
        return _goals.ContainsKey(elementType) && _goals[elementType]>0;
    }

    public void ProgressGoal(BoardElementType elementType)
    {
        _goals[elementType]--;

        CheckLevelFinish();
    }

    private void CheckLevelFinish()
    {
        bool win = true;
        foreach (var goal in _goals)
        {
            if (goal.Value > 0) win = false;
        }

        if (win)
        {
            GameManager.WinLevel();
        }
    }

    public GoalItem GetGoalItemTarget(BoardElementType element)
    {
        return GoalArea.GetGoalItem(element);
    }

    public int GetCurrentGoalAmount(BoardElementType type)
    {
        return _goals[type];
    }
}
