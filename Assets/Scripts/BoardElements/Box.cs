using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : BoardElement
{
    public override BoardElementType ElementType => BoardElementType.Box;
    public override bool DoesFall => false;
    public override void OnClick(Action makeMoveAction)
    {
        throw new NotImplementedException();
    }
}
