using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TNT : BoardElement
{
    [SerializeField] private Vector2Int ExplosionSize;
    public override BoardElementType ElementType => BoardElementType.TNT;
    public override void OnClick(Action makeMoveAction)
    {
        for (int i = 0; i < ExplosionSize.x; i++)
        {
            for (int j = 0; j < ExplosionSize.y; j++)
            {
                var c = new int2(PositionOnBoard.x- (ExplosionSize.x / 2)+i, (PositionOnBoard.y - ExplosionSize.y / 2)+j);
                if (_board.AreValidCoordinates(c))
                {
                    _board[c].Pop();
                }
            }
        }
        makeMoveAction.Invoke();
    }
}
