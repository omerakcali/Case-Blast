using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

public abstract class BoardElement : MonoBehaviour, IPopsWithParticle
{
    [SerializeField] private SpriteRenderer SpriteRenderer;

    [SerializeField] private List<BoardElementState> States = new();
    [SerializeField] private List<Sprite> ParticleSprites;
    public abstract BoardElementType ElementType { get; }
    public abstract bool DoesFall { get; }
    public int2 PositionOnBoard { get; set; }
    private BoardElementPool<BoardElement> _pool;
    
    [System.Serializable]
    struct FallingState
    {
        public float fromY, toY, duration, progress;
    }

    FallingState falling;
    protected Board _board;
    private void Awake()
    {
        if(States.All(a => a.Key != "Default")) States.Add(new BoardElementState("Default",SpriteRenderer.sprite));
    }

    public void SetBoard(Board board)
    {
        _board = board;
    }

    public abstract void OnClick(Action makeMoveAction);

    public virtual void Pop()
    {
        if (GameGoalTracker.Instance.HasGoal(ElementType))
        {
            GameGoalTracker.Instance.ProgressGoal(ElementType);
            /*var tweenItem = GoalTweenManager.Instance.GetTweenItem();
            tweenItem.PlayTween(this);*/
        }

        ParticleManager.Instance.PlayParticle(this);
        
        Despawn();
    }

    public virtual void SetState(string state)
    {
        var elementState = States.Find(s => s.Key == state);
        SpriteRenderer.sprite = elementState.Sprite;
    }

    public BoardElement Spawn(Transform parent, Vector3 pos = new Vector3())
    {
        var instance = _pool.GetInstance(this);
        instance._pool = _pool;
        instance.falling.progress = -1f;
        instance.transform.position = pos;
        instance.transform.SetParent(parent);
        instance.SetState("Default");
        instance.OnSpawn();
        enabled = false;
        return instance;
    }

    public virtual void OnSpawn()
    {
        
    }

    public void Despawn()
    {
        _board.ElementDespawned(PositionOnBoard);
        _pool.Recycle(this);
    }

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

    /*void Update()
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
    }*/

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
            if (GameGoalTracker.Instance.HasGoal(ElementType))
            {
                GameGoalTracker.Instance.ProgressGoal(ElementType);
                GameGoalTracker.Instance.GetGoalItemTarget(ElementType).Refresh();
            }
            
            Despawn();
        });
        return tween;
    }

    public virtual void AlertMatchOnNeighborCell()
    {
        
    }

    public virtual void HitByExplosion()
    {
        Pop();
    }

    [Serializable]
    private class BoardElementState
    {
        public string Key;
        public Sprite Sprite;

        public BoardElementState(string key, Sprite sprite)
        {
            Key = key;
            Sprite = sprite;
        }
    }

    public List<Sprite> GetParticleSprites()
    {
        return ParticleSprites;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}

public enum BoardElementType
{
    RandomDrop = 0, //only for leveldata
    GreenDrop = 1,
    BlueDrop = 2,
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