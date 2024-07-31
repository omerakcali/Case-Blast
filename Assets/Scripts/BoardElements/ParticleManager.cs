using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private PopParticle ParticleInstance;

    public static ParticleManager Instance;
    
    private Stack<PopParticle> _disabledParticles;
    private const int PoolStartCount = 15;

    private void Awake()
    {
        Instance = this;
        _disabledParticles = new();
        for (int i = 0; i < PoolStartCount-1; i++)
        {
            var instance = Instantiate(ParticleInstance,transform);
            instance.Init(this);
            _disabledParticles.Push(instance);
        }
        _disabledParticles.Push(ParticleInstance);
        ParticleInstance.Init(this);
    }

    /*public void PlayParticle(IPopsWithParticle boardElement)
    {
        _disabledParticles.TryPop(out var particleToPlay);

        if (particleToPlay == null)
        {
            particleToPlay = Instantiate(ParticleInstance,transform);
            particleToPlay.Init(this);
        }

        particleToPlay.Play(boardElement);
    }*/

    public void RecycleParticle(PopParticle particle)
    {
        particle.gameObject.SetActive(false);
        _disabledParticles.Push(particle);
    }
}
