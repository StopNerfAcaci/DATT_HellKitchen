using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectMenu : NetworkBehaviour
{
    public event EventHandler<LevelSelectedChangedEventArgs> OnLevelSelectedChanged;
    public class LevelSelectedChangedEventArgs : EventArgs
    {
        public Loader.Scene selectedScene;
    }
    private NetworkVariable<int> networkChosenScene = new NetworkVariable<int>(
        (int)Loader.Scene.LevelSelectScene
    );
    public static LevelSelectMenu Instance { get; private set; }
    [SerializeField] private Button homeButton;
    [SerializeField] private Button startButton;

    public override void OnNetworkSpawn()
    {
        networkChosenScene.OnValueChanged += OnNetworkChosenSceneChanged;
        UpdateStartButton();
    }

    private void Awake()
    {
        Instance = this;
        homeButton.onClick.AddListener(BackToHome);
        startButton.onClick.AddListener(StartGame);
    }
    private void OnNetworkChosenSceneChanged(int previousValue, int newValue)
    {
        chosenScene = (Loader.Scene)newValue;

        // Notify all listeners about the change
        OnLevelSelectedChanged?.Invoke(this, new LevelSelectedChangedEventArgs
        {
            selectedScene = chosenScene
        });
    }
    private void UpdateStartButton()
    {
        startButton.gameObject.SetActive(IsServer);
    }

    private Loader.Scene chosenScene = Loader.Scene.Level_1;

    public void BackToHome()
    {
        NetworkManager.Singleton.Shutdown();
        Loader.Load(Loader.Scene.MainMenuScene);
    }
    private void StartGame()
    {
        Loader.LoadNetwork(chosenScene);
    }
    public void SetChosenScene(Loader.Scene scene)
    {
        if (!IsServer) return;
        SetChosenSceneServerRpc((int)scene);

    }
    [ServerRpc(RequireOwnership = false)]
    private void SetChosenSceneServerRpc(int sceneIndex)
    {
        // Convert back to enum
        Loader.Scene scene = (Loader.Scene)sceneIndex;

        // Update server's local value
        chosenScene = scene;

        // Update network variable to propagate to all clients
        networkChosenScene.Value = sceneIndex;
    }
    public Loader.Scene GetChosenScene()
    {
        return chosenScene;
    }
}
