using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop : BoardElement
{
    [SerializeField] private BoardElementType Type;
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

