using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

   public enum GameState
{
    WaitingToStart, CountDownToStart, Play, Over
}
public class KitchenGameManager : NetworkBehaviour
{

    public static KitchenGameManager Instance;

    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnpaused;
    public event EventHandler OnLocalPlayerReadyChanged;
    public event EventHandler OnLocalPlayerJoinChanged;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnpaused;

    [HideInInspector] public NetworkVariable<GameState> state = new NetworkVariable<GameState>(GameState.WaitingToStart);
    private bool isLocalPlayerReady = false;
    private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gameplayTimer = new NetworkVariable<float>(0f);
    [SerializeField] private float gameplayTimerMax = 300f;
    [SerializeField] private float requireScore = 100f;
    [SerializeField] private Transform playerPrefab;
    private bool isLocalGamePaused = false;
    private NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>(false);
    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, bool> playerPauseDictionary;
    private Dictionary<ulong, bool> playerGameOverDictionary;
    private bool autoTestGamePaused = false;

    private void Awake()
    {
        Instance = this;
        playerReadyDictionary = new Dictionary<ulong, bool>();
        playerPauseDictionary = new Dictionary<ulong, bool>();
        playerGameOverDictionary = new Dictionary<ulong, bool>();
    }
    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }
    public override void OnNetworkSpawn()
    {
        Debug.Log("Network spawn");
        state.OnValueChanged += State_OnValueChanged;
        isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        if (IsServer)
        {
            if (!IsSceneManagerEventRegistered())
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
            }
        }
    }
    private bool IsSceneManagerEventRegistered()
    {
        return eventRegistered;
    }

    // Flag to track if the event has been registered
    private bool eventRegistered = false;
    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Debug.Log("SceneManager_OnLoadEventCompleted: "+ sceneName );
        if (sceneName == Loader.Scene.LevelSelectScene.ToString())
        {
            return;
        }

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null && client.PlayerObject.IsSpawned)
            {
                client.PlayerObject.Despawn(true);  // Forcefully despawn player object
                Destroy(client.PlayerObject.gameObject); // Destroy the player object
            }
        }   
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
        eventRegistered = false;
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        autoTestGamePaused = true;
    }

    private void IsGamePaused_OnValueChanged(bool previousValue, bool newValue)
    {
        if(isGamePaused.Value)
        {
            Time.timeScale = 0f;
            OnMultiplayerGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnMultiplayerGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    private void State_OnValueChanged(GameState previousValue, GameState newValue)
    {
        OnStateChanged?.Invoke(this, new EventArgs());
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (state.Value == GameState.WaitingToStart)
        {
            isLocalPlayerReady = true;

            SetPlayerReadyServerRpc();
            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);

        }else if(state.Value == GameState.Over)
        {
            SetPlayerJoinServerRpc();
            OnLocalPlayerJoinChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;//Id of who send this
        bool allClientReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {

            if (!playerReadyDictionary.ContainsKey(clientId)|| !playerReadyDictionary[clientId])
            {
                allClientReady = false;
                break;
            }
        }
        if (allClientReady)
        {
            state.Value = GameState.CountDownToStart;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerJoinServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerGameOverDictionary[serverRpcParams.Receive.SenderClientId] = true;//Id of who send this
        bool allClientReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {

            if (!playerGameOverDictionary.ContainsKey(clientId) || !playerGameOverDictionary[clientId])
            {
                allClientReady = false;
                break;
            }
        }
        if (allClientReady)
        {
            Player.ResetStaticData();
            Loader.LoadNetwork(Loader.Scene.LevelSelectScene);
        }
    }
    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    private void Update()
    {
        if (!IsServer) return;
        switch (state.Value)
        {
            case GameState.WaitingToStart:
                break;
            case GameState.CountDownToStart:
                countdownToStartTimer.Value -= Time.deltaTime;
                if (countdownToStartTimer.Value <= 0f)
                {
                    state.Value = GameState.Play;
                    gameplayTimer.Value = gameplayTimerMax;
                }
                break;
            case GameState.Play:
                gameplayTimer.Value -= Time.deltaTime;
                if (gameplayTimer.Value <= 0f)
                {
                    state.Value = GameState.Over;
                }
                break;
            case GameState.Over:
                break;
        }
    }

    private void LateUpdate()
    {
        if (autoTestGamePaused)
        {
            autoTestGamePaused = false;
            TestGamePauseState();
        }
    }
    public float GetGamePlayTimer()
    {
        return gameplayTimer.Value;
    }
    public bool IsWaitingToStart()
    {
        return state.Value == GameState.WaitingToStart;
    }
    public bool IsGamePlaying()
    {
        return state.Value == GameState.Play;
    }
    public bool IsCountDownToStartActive()
    {
        return state.Value == GameState.CountDownToStart;
    }
    public float GetCountDownToStartTimer()
    {
        return countdownToStartTimer.Value;
    }
    public bool IsGameOver()
    {
        return state.Value == GameState.Over;
    }
    public bool IsLocalPlayerReady()
    {
        return isLocalPlayerReady;
    }
    public float GetPlayingTimerNormalize()
    {
        return 1-(gameplayTimer.Value / gameplayTimerMax);
    }
    public void TogglePauseGame()
    {
        isLocalGamePaused = !isLocalGamePaused;
        if (isLocalGamePaused)
        {
            PauseGameServerRpc();
            OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            UnpauseGameServerRpc();
            OnLocalGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPauseDictionary[serverRpcParams.Receive.SenderClientId] = true;//Id of who send this

        TestGamePauseState();
    }
    [ServerRpc(RequireOwnership = false)]
    private void UnpauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPauseDictionary[serverRpcParams.Receive.SenderClientId] = false;//Id of who send this

        TestGamePauseState();
    }

    private void TestGamePauseState()
    {
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPauseDictionary.ContainsKey(clientId) && playerPauseDictionary[clientId])
            {
                //This player is paused
                isGamePaused.Value = true;
                return;
            }
        }
        //All player unpause
        isGamePaused.Value = false;
    }

    public float GetRequireScore()
    {
        return requireScore;
    }   
}
