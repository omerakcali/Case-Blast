using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PopParticle : MonoBehaviour
{
    [SerializeField] private ParticleSystem ParticleSystem;

    private ParticleManager _particleManager;
    private ParticleSystem.TextureSheetAnimationModule _textureSheetAnimation;
    
    public void Init(ParticleManager particleManager)
    {
        _particleManager = particleManager;
        _textureSheetAnimation = ParticleSystem.textureSheetAnimation;
        gameObject.SetActive(false);
    }
    public void Play(IPopsWithParticle drop)
    {
        for (int i = 0; i < drop.GetParticleSprites().Count; i++)
        {
            _textureSheetAnimation.AddSprite(drop.GetParticleSprites()[i]);
        }
        transform.position = drop.GetPosition();
        gameObject.SetActive(true);
        DOVirtual.DelayedCall(1.5f, Die).SetLink(gameObject);
    }

    private void Die()
    {
        for (int i = _textureSheetAnimation.spriteCount - 1; i >= 0; i--)
        {
            _textureSheetAnimation.RemoveSprite(i);
        }
        _particleManager.RecycleParticle(this);
    }
}

public interface IPopsWithParticle
{
    public List<Sprite> GetParticleSprites();
    public Vector3 GetPosition();
}
