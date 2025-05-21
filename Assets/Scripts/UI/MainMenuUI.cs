using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playSingleButton;
    [SerializeField] private Button playMultiButton;
    [SerializeField] private Button guideButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject guidePanel;
    private bool guideOn = false;
    private void Awake()
    {
        guidePanel.SetActive(false);
        playSingleButton.onClick.AddListener(() =>
        {
            //Click
            KitchenGameMultiplayer.playMultiplayer = false;
            StartGame();
        });
        playMultiButton.onClick.AddListener(() =>
        {
            //Click
            KitchenGameMultiplayer.playMultiplayer = true;
            StartGame();

        });
        quitButton.onClick.AddListener(() =>
        {
            //Click
            if (guideOn) return;
            Application.Quit();

        });
        guideButton.onClick.AddListener(() =>
        {
            if (guideOn) return;
            guideOn = true;
            guidePanel.SetActive(true);

        });
        Time.timeScale = 1f;
    }
    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (!guideOn) return;
        guideOn = false;
        guidePanel.SetActive(false);
    }

    void StartGame()
    {
        if (guideOn) return;
        //Loader.Load(Loader.Scene.LevelSelectScene);
        Loader.Load(Loader.Scene.LobbyScene);
    }
}
