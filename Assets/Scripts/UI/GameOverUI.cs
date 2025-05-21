using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    //[SerializeField] private Button restartBtn;
    [SerializeField] private Image closingSceneImg;
    [SerializeField] private GameObject gameResult;

    [SerializeField] private TextMeshProUGUI currentlevelText;
    [SerializeField] private TextMeshProUGUI rcpDeliveredText;
    [SerializeField] private TextMeshProUGUI rcpFailedText;
    [SerializeField] private TextMeshProUGUI totalScore;
    [SerializeField] private TextMeshProUGUI checkWin;
    [SerializeField] private TextMeshProUGUI joinTriggerText;

    private float idleDuration = 1f;
    private float fillDuration = 1f;
    private float waitToDisplayDuration = 1.5f;
    private void Awake()
    {
        closingSceneImg.fillAmount = 0;
        closingSceneImg.gameObject.SetActive(false);
        joinTriggerText.text = "Press 'E' to join";
        joinTriggerText.gameObject.SetActive(false);

    }
    private void Start()
    {

        GameOverNoticeUI.Instance.OnGameComplete += GameOverNoticeUI_OnGameComplete;
        KitchenGameManager.Instance.OnLocalPlayerJoinChanged += KitchenGameManager_LocalPlayerChanged;
        Hide();
    }

    private void KitchenGameManager_LocalPlayerChanged(object sender, EventArgs e)
    {
        joinTriggerText.text = "Wait for other players";
    }

    private void GameOverNoticeUI_OnGameComplete(object sender, System.EventArgs e)
    {
        if (KitchenGameManager.Instance.IsGameOver())
        {
            StartCoroutine(DisplayResult());
        }
        else
        {
            Hide();
        }
    }
    IEnumerator DisplayResult()
    {

        yield return new WaitForSeconds(idleDuration);
        closingSceneImg.gameObject.SetActive(true);
        closingSceneImg.DOComplete();
        closingSceneImg.DOFillAmount(1, fillDuration);
        yield return GetResultData();
    }
    IEnumerator GetResultData()
    {
        yield return new WaitForSeconds(waitToDisplayDuration);
        Show();
        int successAmount = DeliveryManager.Instance.GetSuccessfulRecipeAmount();
        int failedAmount = DeliveryManager.Instance.GetFailedRecipeAmount();
        rcpDeliveredText.text = string.Format(successAmount.ToString());
        rcpFailedText.text = string.Format("{0} x 20 = {1}", failedAmount.ToString(), (failedAmount * 20).ToString());
        int total = MoneyManager.instance.GetLevelMoney() - failedAmount * 10;
        totalScore.text = total.ToString();
        yield return CheckWin(total);
        yield return new WaitForSeconds(idleDuration / 2);
        joinTriggerText.gameObject.SetActive(true);

    }
    IEnumerator CheckWin(float score)
    {

        if (KitchenGameManager.Instance.GetRequireScore() <= score)
        {
            checkWin.text = "You Win!";
            checkWin.color = Color.green;
        }
        else
        {
            checkWin.text = "You Lose!";
            checkWin.color = Color.red;
        }
        checkWin.gameObject.SetActive(true);

        var rectTF = checkWin.GetComponent<RectTransform>();
        rectTF.localScale = new Vector3(1.5f, 1.5f, 1f);
        rectTF.localPosition = new Vector3(19, 30, 0);
        rectTF.DOComplete();
        rectTF.DOLocalMove(new Vector3(0, 0, 0), 1f).SetEase(Ease.OutBack);
        yield return rectTF.DOScale(new Vector3(1, 1, 1), 1f).SetEase(Ease.OutBack);
    }
    private void Show()
    {
        gameResult.SetActive(true);
    }
    private void Hide()
    {
        gameResult.SetActive(false);
    }
}
