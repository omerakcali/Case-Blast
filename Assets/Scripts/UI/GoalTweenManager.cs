using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GoalTweenManager : MonoBehaviour
{
    [SerializeField] private GoalTweenItem ItemInstance;

    public static GoalTweenManager Instance;
    
    private Stack<GoalTweenItem> _disabledItems;

    private const int PoolStartCount = 15;
    
    private void Awake()
    {
        Instance = this;
        _disabledItems = new();
        for (int i = 0; i < PoolStartCount-1; i++)
        {
            var instance = Instantiate(ItemInstance,transform);
            instance.Init(this);
            _disabledItems.Push(instance);
        }
        _disabledItems.Push(ItemInstance);
        ItemInstance.Init(this);
    }

    public GoalTweenItem GetTweenItem()
    {
        if (_disabledItems.TryPop(out var instance))
        {
            return instance;
        }

        var tweenItem = Instantiate(ItemInstance,transform);
        tweenItem.Init(this);
        return tweenItem;
    }

    public void Return(GoalTweenItem item)
    {
        _disabledItems.Push(item);
    }
}
