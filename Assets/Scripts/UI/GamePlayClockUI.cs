using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayClockUI : MonoBehaviour
{
    [SerializeField] private Image timerImage;
    [SerializeField] private TextMeshProUGUI timerText;

    private void Update()
    {
        UpdateTimerUI(KitchenGameManager.Instance.GetGamePlayTimer());
        timerImage.fillAmount = KitchenGameManager.Instance.GetPlayingTimerNormalize();
    }
    private void UpdateTimerUI(float second)
    {
        if(second <= 0)
        {
            timerText.text = "0:00";
            return;
        }
        int minutes = Mathf.FloorToInt(second / 60);
        int secs = Mathf.FloorToInt(second % 60);
        timerText.text = string.Format("{0:0}:{1:00}", minutes, secs);
    }
}
