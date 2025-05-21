using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private Image imageBar;
    [SerializeField] private GameObject hasProgressGO;
    private IHasProgress hasProgress;
    private void Start()
    {
        hasProgress = hasProgressGO.GetComponent<IHasProgress>();
        if (hasProgress == null)
        {
            Debug.LogError("Does not have component implements has progress");
        }

        hasProgress.OnProgressChanged += HasProgress_OnProgressChanged;
        imageBar.fillAmount = 0f;
        Hide();
    }

    private void HasProgress_OnProgressChanged(object sender, IHasProgress.OnProgressChangeEventArgs e)
    {

        imageBar.fillAmount = e.progressNormalize;
        if (e.progressNormalize == 0 || e.progressNormalize == 1)
        {
            Hide();
        }
        else
        {
            Show();
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
