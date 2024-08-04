using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalArea : MonoBehaviour
{
    [SerializeField] private List<GoalItem> GoalItems;
    [SerializeField] private BoardPoolManager BoardPoolManager;

    private Dictionary<BoardElementType, GoalItem> _dictionary;

    private void Start()
    {
        Board.LevelLoadEvent += OnLevelLoad;
    }

    private void OnLevelLoad(LevelInfo obj)
    {
        _dictionary = new();
        for (var i = 0; i < GoalItems.Count; i++)
        {
            if(i< obj.Goals.Count)
            {
                var goal = obj.Goals[i];
                GoalItems[i].gameObject.SetActive(true);
                GoalItems[i].Init(BoardPoolManager.GetBoardElementSprite(goal.GoalType), goal.GoalType);
                _dictionary.Add(goal.GoalType, GoalItems[i]);
            }else GoalItems[i].gameObject.SetActive(false);
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            
        }
    }

    public GoalItem GetGoalItem(BoardElementType type)
    {
        return _dictionary[type];
    }
}