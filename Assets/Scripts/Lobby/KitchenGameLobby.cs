using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameLobby : MonoBehaviour
{
    private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";
    public static KitchenGameLobby Instance { get; private set; }

    public event EventHandler OnCreateLobbyStarted; 
    public event EventHandler OnCreateLobbyFailed; 
    public event EventHandler OnJoinStarted; 
    public event EventHandler OnJoinFailed; 
    public event EventHandler OnQuickJoinFailed; 
    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> LobbyList { get; set; }
    }

    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float listLobbiesTimer;
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeUnityAuthentication();
    }

    private async void InitializeUnityAuthentication()
    {
        if(UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());
            await UnityServices.InitializeAsync(initializationOptions);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

    }
    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(KitchenGameMultiplayer.maxPlayerAmount - 1);
            return allocation;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Failed to allocate relay: {e}");

            return default;
        }
    }
    private async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Failed to get relay join code: {e}");
            return default;
        }
    }
    private async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            return joinAllocation;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Failed to join relay: {e}");
            return default;

        }
    }
    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, KitchenGameMultiplayer.maxPlayerAmount, new CreateLobbyOptions
            {
                IsPrivate = isPrivate
            });
            Allocation allocation = await AllocateRelay();
            string relayJoinCode = await GetRelayJoinCode(allocation);

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                IsPrivate = isPrivate,
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

            KitchenGameMultiplayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to create lobby: {e}");
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
        }

    }
    public async void QuickJoinLobby()
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation =await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
            KitchenGameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to create lobby: {e}");
            OnQuickJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }
    public async void JoinWithCode(string lobbyCode)
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
            KitchenGameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to join lobby: {e}");
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }
    public async void JoinWithId(string lobbyId)
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            //joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
            KitchenGameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
            Debug.LogError($"Failed to join lobby: {e}");

        }
    }
    public async void DeleteLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to delete lobby: {e}");
            }
        }
    }
    public async void LeaveLobby()
    {
        if(joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to leave lobby: {e}");
            }
        }

    }
    public async void KickPlayerFromLobby(string playerId)
    {
        if(IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Failed to leave lobby: {e}");
            }
        }

    }
    public Lobby GetLobby()
    {
        return joinedLobby;
    }
    private void Update()
    {
        HandleHeartBeat();
        HandlePeriodicLobbyList();
    }

    private void HandlePeriodicLobbyList()
    {
        if(joinedLobby == null && AuthenticationService.Instance.IsSignedIn && SceneManager.GetActiveScene().name ==Loader.Scene.LobbyScene.ToString())
        {
            listLobbiesTimer -= Time.deltaTime;
            if (listLobbiesTimer <= 0f)
            {
                float listLobbiesTimerMax = 5f;
                listLobbiesTimer = listLobbiesTimerMax;
                ListLobbies();
            }
        }

    }

    private void HandleHeartBeat()
    {
        if (IsLobbyHost())
        {
            heartbeatTimer -= Time.deltaTime;
            if(heartbeatTimer <= 0f)
            {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;
                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }
    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 10,
                Filters = new List<QueryFilter>
            {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0",QueryFilter.OpOptions.GT)
            }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs
            {
                LobbyList = queryResponse.Results
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to list lobbies: {e}");
        }
        
    }
    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }
}
