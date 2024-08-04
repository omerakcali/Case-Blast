using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalItem : MonoBehaviour
{
    [SerializeField] private Image Image;
    [SerializeField] private TextMeshProUGUI AmountText;
    [SerializeField] private GameObject CheckMarkGO;

    private BoardElementType _type;

    public void Init(Sprite sprite, BoardElementType type)
    {
        Image.sprite = sprite;
        _type = type;
        Refresh();
    }

    public void Refresh(bool hit = false)
    {
        var amount = GameGoalTracker.Instance.GetCurrentGoalAmount(_type);
        AmountText.gameObject.SetActive(amount > 0);
        CheckMarkGO.gameObject.SetActive(amount<=0);
        AmountText.SetText(amount.ToString());
    }
}