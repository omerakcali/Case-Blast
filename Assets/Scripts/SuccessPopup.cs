using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SuccessPopup : MonoBehaviour
{
    [SerializeField] private CanvasGroup CanvasGroup;
    [SerializeField] private Transform Root;
    [SerializeField] private Button Button;

    private void Awake()
    {
        GameManager.LevelFinishEvent += OnLevelFinish;
    }

    private void Start()
    {
        gameObject.SetActive(false);
        Button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        SceneLoadManager.Instance.LoadMenuScene();
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

    private void OnDestroy()
    {
        GameManager.LevelFinishEvent -= OnLevelFinish;
    }
}
