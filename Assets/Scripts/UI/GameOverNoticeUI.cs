using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System;

public class GameOverNoticeUI : MonoBehaviour
{
    public static GameOverNoticeUI Instance;
    public event EventHandler OnGameComplete;
    [SerializeField] TextMeshProUGUI noticeText;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;
        Hide();
    }

    private void KitchenGameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (KitchenGameManager.Instance.IsGameOver())
        {
            Show();
            noticeText.transform.localScale = new Vector3(.1f,.1f,.1f);
            noticeText.DOComplete();
            noticeText.transform.DOScale(Vector3.one,.6f);
            OnGameComplete?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Hide();
        }
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
