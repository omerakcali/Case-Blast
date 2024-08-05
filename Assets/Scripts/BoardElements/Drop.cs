using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop : BoardElement
{
    [SerializeField] private Color ParticleColor;
    [SerializeField] private BoardElementType Type;
    public Vector3 GetPosition() => transform.position;

    public override BoardElementType ElementType => Type;
    public override bool DoesFall => true;

    public override void OnClick(Action makeMoveAction)
    {
        if (_board.TryMove(PositionOnBoard))
        {
            makeMoveAction.Invoke();
        }
    }
}

