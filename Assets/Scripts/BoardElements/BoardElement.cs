using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public abstract class BoardElement : MonoBehaviour
{
    [SerializeField] private SpriteRenderer SpriteRenderer;
    public abstract BoardElementType ElementType { get; }
    private BoardElementPool<BoardElement> _pool;

    [System.Serializable]
    struct FallingState
    {
        public float fromY, toY, duration, progress;
    }

    FallingState falling;

    public virtual void Pop()
    {
        Despawn();
    }

    public BoardElement Spawn(Transform parent, Vector3 pos = new Vector3())
    {
        var instance = _pool.GetInstance(this);
        instance._pool = _pool;
        instance.falling.progress = -1f;
        instance.transform.position = pos;
        instance.transform.SetParent(parent);
        enabled = false;
        return instance;
    }

    public void Despawn() => _pool.Recycle(this);

    public float Fall(float toY, float speed)
    {
        falling.fromY = transform.localPosition.y;
        falling.toY = toY;
        falling.duration = (falling.fromY - toY) / speed;
        falling.progress = 0f;
        transform.DOLocalMoveY(falling.toY, falling.duration).SetEase(Ease.Linear);
        //enabled = true;
        return falling.duration;
    }

    void Update()
    {
        if (falling.progress >= 0f)
        {
            Vector3 position = transform.localPosition;
            falling.progress += Time.deltaTime;
            if (falling.progress >= falling.duration)
            {
                falling.progress = -1f;
                position.y = falling.toY;
            }
            else
            {
                position.y = Mathf.Lerp(
                    falling.fromY, falling.toY, falling.progress / falling.duration
                );
            }
            transform.localPosition = position;
        }
    }

    public virtual void SetSortingOrder(int order)
    {
        SpriteRenderer.sortingOrder = order;
    }

    public Sprite GetSprite() => SpriteRenderer.sprite;

    public Tween Merge(Vector3 targetPosition)
    {
        var tween = transform.DOMove(targetPosition, .25f);
        tween.SetEase(Ease.InOutSine);
        tween.OnComplete(() =>
        {
            Despawn();
        });
        return tween;
    }

    public virtual float GetPopAnimationDuration()
    {
        return 0;
    }

    public void AlertMatchOnNeighborCell()
    {
        
    }
}

public enum BoardElementType
{
    Empty = 0,
    GreenDrop = 1,
    BlueDrop = 2,
    PurpleDrop = 3,
    YellowDrop = 4,
    RedDrop = 5,
    Box = 11,
    Stone = 12,
    Vase =13,
    TNT = 101,
}

public static class BoardElementTypeExtension
{
    public static bool IsDrop(this BoardElementType type) => (int)type is < 10 and > 0;

    public static bool IsObstacle(this BoardElementType type) => (int)type is < 100 and > 10;

    public static bool IsSpecial(this BoardElementType type) =>
        type is BoardElementType.TNT;
}