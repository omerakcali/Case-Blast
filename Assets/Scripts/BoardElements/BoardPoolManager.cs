using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BoardPoolManager : MonoBehaviour
{
    [SerializeField]
    BoardElement[] boardElementPrefabs;

    private Dictionary<BoardElementType, BoardElement> _dictionary= new();

    private void Awake()
    {
        foreach (var boardElement in boardElementPrefabs)
        {
            _dictionary.Add(boardElement.ElementType,boardElement);
        }
    }

    public BoardElement SpawnBoardElement(BoardElementType boardElementType, float x, float y)
    {
        return _dictionary[boardElementType].Spawn(transform,new Vector3(x,y));
    }

    public Sprite GetBoardElementSprite(BoardElementType type)
    {
        return _dictionary[type].GetSprite();
    }

    private void OnDestroy()
    {
        foreach (var boardElement in boardElementPrefabs)
        {
            boardElement.ClearPool();
        }
    }
}
