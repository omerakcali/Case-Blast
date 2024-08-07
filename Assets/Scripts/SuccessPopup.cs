using DG.Tweening;
using UnityEngine;

public class SuccessPopup : MonoBehaviour
{
    [SerializeField] private CanvasGroup CanvasGroup;
    [SerializeField] private Transform Root;

    private void Start()
    {
        gameObject.SetActive(false);
        GameManager.LevelFinishEvent += OnLevelFinish;
    }

    private void OnLevelFinish(bool isWin)
    {
        if(!isWin) return;

        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
        CanvasGroup.interactable = false;
        CanvasGroup.DOFade(1f, .35f).From(.1f);
        Root.DOScale(1f, .5f).From(.15f).OnComplete(() =>
        {
            CanvasGroup.interactable = true;
        });
    }
}
