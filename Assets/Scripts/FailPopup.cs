using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FailPopup : MonoBehaviour
{
    [SerializeField] private Button TryAgainButton;
    [SerializeField] private Button ReturnButton;
    [SerializeField] private Button CloseButton;
    [SerializeField] private Transform Root;
    [SerializeField] private CanvasGroup CanvasGroup;
    [SerializeField] private GameManager GameManager;

    private void Start()
    {
        GameManager.LevelFinishEvent += OnLevelFinish;
        
        TryAgainButton.onClick.AddListener(OnTryAgainPressed);
        ReturnButton.onClick.AddListener(ReturnToMenu);
        CloseButton.onClick.AddListener(ReturnToMenu);
        gameObject.SetActive(false);
    }

    private void ReturnToMenu()
    {
        throw new NotImplementedException();
    }

    private void OnTryAgainPressed()
    {
        DOClose().OnComplete(GameManager.LoadCurrentLevel);
    }

    private void OnLevelFinish(bool isWin)
    {
        if(isWin) return;
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
        CanvasGroup.interactable = false;
        CanvasGroup.DOFade(1f, .3f).From(.35f);
        Root.DOScale(1f, .3f).From(.15f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            CanvasGroup.interactable = true;
        });
    }

    private Tween DOClose()
    {
        return CanvasGroup.DOFade(0f, .3f).OnStart(() =>
        {
            CanvasGroup.interactable = false;
            Root.DOScale(.2f, .3f);
        });
    }
}
