using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop : BoardElement, IPopsWithParticle
{
    [SerializeField] private Color ParticleColor;
    [SerializeField] private BoardElementType Type;
    public Color GetParticleColor() => ParticleColor;
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

    public override void Pop()
    {
        /*ParticleManager.Instance.PlayParticle(this);
        SoundManager.Instance.PlaySound("pop_cube");*/
        base.Pop();
    }
}

public interface IPopsWithParticle
{
    public Color GetParticleColor();
    public Vector3 GetPosition();
}

