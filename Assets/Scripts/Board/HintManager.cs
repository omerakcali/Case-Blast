using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    [SerializeField] private Board Board;

    private List<int2> _checkedCoordinates= new();

    private void Awake()
    {
        GameManager.MoveMadeEvent += OnMoveMade;
        GameManager.BoardPlayableEvent += OnBoardPlayable;
    }

    private void OnBoardPlayable()
    {
        CheckHints();
    }

    private List<int2> _foundGroup=new();
    private void CheckHints()
    {
        for (int i = 0; i < Board.Size.y; i++)
        {
            for (int j = 0; j < Board.Size.x; j++)
            {
                var c = new int2(j, i);
                if(_checkedCoordinates.Contains(c)) continue;
                if(Board[c]==null) continue;
                if (FindGroups(c))
                {
                    bool isTnt = _foundGroup.Count >= Board.TNTCellCount;
                    foreach (var coordinates in _foundGroup)
                    {
                        _checkedCoordinates.Add(coordinates);
                        if(isTnt)Board[coordinates].SetState("Hint_TNT");
                    } 
                }
                else
                {
                    _checkedCoordinates.Add(c);
                }
            }
        }
    }
    
    List<int2> _openList = new();
    List<int2> _closedList = new();
    
    private bool FindGroups(int2 origin) 
    {
        if (!Board[origin].ElementType.IsDrop()) return false;
        _foundGroup.Clear();
        _openList.Clear();
        _closedList.Clear();
        _openList.Add(origin);
        var selectedDropType = Board[origin].ElementType;
        while (_openList.Count > 0)
        {
            var tile = _openList[^1];
            _openList.RemoveAt(_openList.Count - 1);
            if (_closedList.Contains(tile)) continue;
            _closedList.Add(tile);

            for (int i = -1; i <= +1; i++)
            {
                if (i == 0) continue;

                var horizontalNeighbor = tile + new int2(i, 0);
                if (Board.AreValidCoordinates(horizontalNeighbor)
                    && Board[horizontalNeighbor] != null
                    && !_closedList.Contains(horizontalNeighbor)
                    && !_openList.Contains(horizontalNeighbor)
                    && Board[horizontalNeighbor].ElementType == selectedDropType)
                {
                    _openList.Add(horizontalNeighbor);
                    _foundGroup.Add(horizontalNeighbor);
                }

                var verticalNeighbor = tile + new int2(0, i);
                if (Board.AreValidCoordinates(verticalNeighbor)
                    && Board[verticalNeighbor] != null
                    && !_closedList.Contains(verticalNeighbor)
                    && !_openList.Contains(verticalNeighbor)
                    && Board[verticalNeighbor].ElementType == selectedDropType)
                {
                    _openList.Add(verticalNeighbor);
                    _foundGroup.Add(verticalNeighbor);
                }
            }
        }

        if (_foundGroup.Count > 0) _foundGroup.Insert(0,origin);

        return _foundGroup.Count > 0;
    }
    

    private void OnMoveMade()
    {
        _checkedCoordinates.Clear();
        SetAllDropsToDefault();
    }

    private void SetAllDropsToDefault()
    {
        for (int i = 0; i < Board.Size.y; i++)
        {
            for (int j = 0; j < Board.Size.x; j++)
            {
                var element = Board[j, i];
                if(element == null) continue;
                if (element.ElementType.IsDrop()) element.SetState("Default");
            }
        }
    }
    
    private void OnDestroy()
    {
        GameManager.MoveMadeEvent -= OnMoveMade;
        GameManager.BoardPlayableEvent -= OnBoardPlayable;
    }
}
