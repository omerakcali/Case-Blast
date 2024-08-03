using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TNT : BoardElement
{
    [SerializeField] private Vector2Int ExplosionSize;
    public override BoardElementType ElementType => BoardElementType.TNT;
    public override bool DoesFall => true;

    private bool _popped;
    
    public override void OnClick(Action makeMoveAction)
    {
        if (_popped)
        {
            return;
        }

        Pop();
        makeMoveAction.Invoke();
    }

    public override void OnSpawn()
    {
        _popped = false;
    }

    public override void Pop()
    {
        if (_popped)
        {
            return;
        }
        _popped = true;
        for (int i = -ExplosionSize.x; i <= ExplosionSize.x; i++)
        {
            for (int j = -ExplosionSize.y; j <= ExplosionSize.y; j++)
            {
                if(i==0 && j ==0) continue;
                var c = new int2(PositionOnBoard.x+i, PositionOnBoard.y +j);
                if (_board.AreValidCoordinates(c))
                {
                    if(_board[c]!=null)
                        _board[c].HitByExplosion();
                }
            }
        }
        base.Pop();
    }
}
