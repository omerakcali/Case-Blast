using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PopParticle : MonoBehaviour
{
    [SerializeField] private ParticleSystem ParticleSystem;

    private ParticleManager _particleManager;
    private ParticleSystem.MainModule _main;
    
    public void Init(ParticleManager particleManager)
    {
        _particleManager = particleManager;
        _main = ParticleSystem.main;
        gameObject.SetActive(false);
    }
    /*public void Play(IPopsWithParticle drop)
    {
        _main.startColor = drop.GetParticleColor();
        transform.position = drop.GetPosition();
        gameObject.SetActive(true);
        DOVirtual.DelayedCall(1.5f, Die);
    }*/

    private void Die()
    {
        _particleManager.RecycleParticle(this);
    }
}
