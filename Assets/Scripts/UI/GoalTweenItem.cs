using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GoalTweenItem : MonoBehaviour
{
    [SerializeField] private Image Image;
    [SerializeField] private RectTransform RectTransform;

    [Header("Tween Settings")] [SerializeField]
    private float MoveSpeed;

    [SerializeField] private AnimationCurve XCurve;
    [SerializeField] private AnimationCurve YCurve;

    private GoalTweenManager _manager;

    private Sequence _sequence;

    public void Init(GoalTweenManager manager)
    {
        _manager = manager;
        gameObject.SetActive(false);
    }

    private Canvas a;

    public void PlayTween(BoardElement element)
    {
        Image.sprite = element.GetSprite();
        var elementType = element.ElementType;
        var target = GameGoalTracker.Instance.GetGoalItemTarget(elementType);
        
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(element.transform.position);
        Vector2 result;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), screenPoint, Camera.main, out result);
        RectTransform.anchoredPosition = result;
        
        gameObject.SetActive(true);
        float yDelta = target.transform.position.y - transform.position.y;
        
        _sequence = DOTween.Sequence();
        _sequence
            .Join(transform.DOMoveX(target.transform.position.x, yDelta/MoveSpeed).SetEase(XCurve))
            .Join(transform.DOMoveY(target.transform.position.y, yDelta/MoveSpeed).SetEase(YCurve))
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                GameGoalTracker.Instance.ProgressGoal(elementType);
                target.Refresh(true);
                _manager.Return(this);
            })
            .OnPlay(() => { gameObject.SetActive(true); });
    }
}