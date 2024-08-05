using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

public class TNT : BoardElement
{
    [SerializeField] private GameObject ParticleGO;
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
        ParticleGO.SetActive(false);
        _popped = false;
    }

    public override void Pop()
    {
        if (_popped)
        {
            return;
        }
        ParticleGO.SetActive(true);
        _popped = true;
        DOVirtual.DelayedCall(.15f, () =>
        {

            for (int i = -ExplosionSize.x; i <= ExplosionSize.x; i++)
            {
                for (int j = -ExplosionSize.y; j <= ExplosionSize.y; j++)
                {
                    if (i == 0 && j == 0) continue;
                    var c = new int2(PositionOnBoard.x + i, PositionOnBoard.y + j);
                    if (_board.AreValidCoordinates(c))
                    {
                        if (_board[c] != null)
                            _board[c].HitByExplosion();
                    }
                }
            }
            Despawn();
        });
    }
}
