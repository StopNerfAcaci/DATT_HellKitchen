using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField] private Button resumeBtn;
    [SerializeField] private Button mainMenuBtn;
    [SerializeField] private Button optionBtn;

    private void Awake()
    {

        resumeBtn.onClick.AddListener(() =>
        {
            Debug.Log("Resume Button Clicked");
            KitchenGameManager.Instance.TogglePauseGame();
        });
        mainMenuBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();    
            Loader.Load(Loader.Scene.MainMenuScene);
        });
        optionBtn.onClick.AddListener(() =>
        {
            OptionsUI.Instance.Show();
        });
    }
    private void Start()
    {
        KitchenGameManager.Instance.OnLocalGamePaused += KitchenGameManager_OnLocalGamePaused;
        KitchenGameManager.Instance.OnLocalGameUnpaused += KitchenGameManager_OnLocalGameUnpaused;
        Hide();
    }

    private void KitchenGameManager_OnLocalGameUnpaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void KitchenGameManager_OnLocalGamePaused(object sender, System.EventArgs e)
    {
        Show();
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
