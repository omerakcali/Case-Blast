using System;
using System.Collections.Generic;
using UnityEngine;

public class Vase:BoardElement
{
    public override BoardElementType ElementType => BoardElementType.Vase;
    public override bool DoesFall => true;

    [SerializeField] private int HitPoint = 2;

    private int _currentHp;

    public override void OnSpawn()
    {
        _currentHp = HitPoint;
        Refresh();
        base.OnSpawn();
    }

    public override void OnClick(Action makeMoveAction)
    {
    }

    public override void AlertMatchOnNeighborCell()
    {
        Damage();
    }

    public override void HitByExplosion()
    {
        Damage();
    }

    private void Damage()
    {
        _currentHp--;
        if (_currentHp <= 0)
        {
            Pop();
        }
        else
        {
            ParticleManager.Instance.PlayParticle(this);
            Refresh();
        }
    }

    private void Refresh()
    {
        SetState($"HP_{_currentHp}");
    }
}