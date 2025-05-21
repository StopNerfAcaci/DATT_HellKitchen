using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterSelectReady : NetworkBehaviour
{
    public static CharacterSelectReady Instance { get; private set; }
    private Dictionary<ulong, bool> playerReadyDictionary;

    public event EventHandler OnReadyChanged;
    private void Awake()
    {
        Instance = this;
        playerReadyDictionary = new Dictionary<ulong, bool>();
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);//Id of who send this
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;//Id of who send this
        bool allClientReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {

            if (!playerReadyDictionary.ContainsKey(clientId) || playerReadyDictionary[clientId] == false)
            {
                allClientReady = false;
                return;
            }
        }
        if (allClientReady)
        {
            KitchenGameLobby.Instance.DeleteLobby();
            Loader.LoadNetwork(Loader.Scene.LevelSelectScene);
        }
    }
    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId)
    {
        playerReadyDictionary[clientId] = true;
        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyDictionary.ContainsKey(clientId) && playerReadyDictionary[clientId];
    }
}
