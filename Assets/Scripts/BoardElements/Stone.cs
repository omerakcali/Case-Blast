using System;

public class Stone : BoardElement
{
    public override BoardElementType ElementType => BoardElementType.Stone;
    public override bool DoesFall => false;

    public override void OnClick(Action makeMoveAction)
    {
        
    }

    public override void HitByExplosion()
    {
        Pop();
    }
}