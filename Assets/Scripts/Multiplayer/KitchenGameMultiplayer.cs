using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameMultiplayer : NetworkBehaviour  
{

    public const int maxPlayerAmount = 3;
    public const string PLAYERPREFS_PLAYERNAME_MULTIPLAYER = "PlayerNameMultiplayer";
    public static KitchenGameMultiplayer Instance { get; private set; }

    public static bool playMultiplayer;
    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkChanged;

    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;
    [SerializeField] private List<Color> playerColorList;

    private NetworkList<PlayerData> playerDataNetworkList;
    private string playerName;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerName = PlayerPrefs.GetString(PLAYERPREFS_PLAYERNAME_MULTIPLAYER, "PlayerName"+ UnityEngine.Random.Range(100,10000));
        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }
    private void Start()
    {
        if (!playMultiplayer)
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", 7777);
            //Singleplay
            StartHost();
            Debug.Log("Load level");
            Loader.LoadNetwork(Loader.Scene.LevelSelectScene);
        }
    }
    public string GetPlayerName()
    {
        return playerName;
    }
    public void SetPlayerName(string name)
    {
        playerName = name;

        PlayerPrefs.SetString(PLAYERPREFS_PLAYERNAME_MULTIPLAYER, playerName);
    }
    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManagerConnectionCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_DisconnectionCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManagerConnectionCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,
            colorId = GetFirstUnusedColorId()
        });
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }
    private void NetworkManager_Server_DisconnectionCallback(ulong clientId)
    {

        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                //Disconnected
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }
    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if(SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString())
        {
            response.Approved = false;
            response.Reason = "Game has already started";
            return;
        }
        if(NetworkManager.Singleton.ConnectedClients.Count > maxPlayerAmount)
        {
            response.Approved = false;
            response.Reason = "Game is full";
            return;
        }
        response.Approved = true;
     }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_ConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_DisconnectionApprovalCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_ConnectedCallback(ulong clientId)
    {
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }
    [ServerRpc(RequireOwnership = false)]   
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.playerName = playerName;
        playerDataNetworkList[playerDataIndex] = playerData;
    }
    [ServerRpc(RequireOwnership = false)]   
    private void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.playerId = playerId;
        playerDataNetworkList[playerDataIndex] = playerData;
    }

    private void NetworkManager_Client_DisconnectionApprovalCallback(ulong obj)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }



    public void SpawnKitchenObj(KitchenObjectSO kitchenObjectSO, IKitchenObjParent parent, bool isDirty = false)
    {
        SpawnKitchenObjServerRpc(GetKitchenObjectSOIndex(kitchenObjectSO), parent.GetNetworkObject(), isDirty);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjServerRpc(int kitchenObjectSOIndex, NetworkObjectReference parentNetwork, bool isDirty = false)
    {
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        Transform kitchenObjTransform = Instantiate(kitchenObjectSO.prefab);
        NetworkObject kitchenObjNetwork = kitchenObjTransform.GetComponent<NetworkObject>();
        kitchenObjNetwork.Spawn(true); //Require to let client spawn
        KitchenObject kitchenObj = kitchenObjTransform.GetComponent<KitchenObject>();
        if (kitchenObj.TryGetPlate(out PlateKitchenObject plate))
        {
            plate.isDirty = isDirty;
        }
        SetPlateDirtyStateClientRpc(kitchenObjNetwork, isDirty);
        parentNetwork.TryGet(out NetworkObject KitchenObjectParentNetworkObject);
        IKitchenObjParent parent = KitchenObjectParentNetworkObject.GetComponent<IKitchenObjParent>();
        kitchenObj.SetKitchenObjParent(parent);
    }
    public int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectListSO.kitchenObjectSOList.IndexOf(kitchenObjectSO);
    }
    public KitchenObjectSO GetKitchenObjectSOFromIndex(int index)
    {
        KitchenObjectSO kitchenObjectSO = kitchenObjectListSO.kitchenObjectSOList[index];
        return kitchenObjectSO;
    }
    [ClientRpc]
    private void SetPlateDirtyStateClientRpc(NetworkObjectReference plateObjectRef, bool isDirty)
    {
        plateObjectRef.TryGet(out NetworkObject plateNetworkObject);
        if (plateNetworkObject != null)
        {
            PlateKitchenObject plate = plateNetworkObject.GetComponent<PlateKitchenObject>();
            if (plate != null)
            {
                plate.isDirty = isDirty;
            }
        }
    }
    public void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        DestroyKitchenObjectServerRpc(kitchenObject.NetworkObject);
    }
    [ServerRpc(RequireOwnership = false)]
    private void DestroyKitchenObjectServerRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
        KitchenObject kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();
        ClearKitchenObjectClientRpc(kitchenObjectNetworkObjectReference);
        kitchenObject.DestroyItself();
    }

    [ClientRpc]
    private void ClearKitchenObjectClientRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
        KitchenObject kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();
        kitchenObject.ClearKitchenObjectOnParent();
    }

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex< playerDataNetworkList.Count;
    }
    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for(int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                return i;
            }
        }
        return -1;
    }
    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
            {
                return playerData;
            }
        }
        return default;
    }
    public PlayerData GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }
    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }

    public Color GetPlayerColor(int colorId)
    {
        return playerColorList[colorId];
    }
    public void ChangePlayerColor(int colorId)
    {
        ChangePlayerColorServerRpc(colorId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerColorServerRpc(int colorId, ServerRpcParams serverRpcParams = default)
    {
        if (!IsColorAvailable(colorId)) return;
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];
        playerData.colorId = colorId;
        playerDataNetworkList[playerDataIndex] = playerData;
    }
    private bool IsColorAvailable(int colorId)
    {
        foreach(PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.colorId == colorId)
            {
                return false;
            }
        }
        return true;
    }
    public int GetFirstUnusedColorId()
    {
        for (int i = 0; i < playerColorList.Count; i++)
        {
            if (IsColorAvailable(i))
            {
                return i;
            }
        }
        return -1;
    }
    public void KickPlayer(ulong playerId)
    {
        NetworkManager.Singleton.DisconnectClient(playerId);
        NetworkManager_Server_DisconnectionCallback(playerId);
    }
}
