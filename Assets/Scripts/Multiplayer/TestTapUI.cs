using System;
using Unity.Netcode;
using UnityEngine;

public class TestTapUI : MonoBehaviour
{
    private void Start()
    {
        Show();
        KitchenGameManager.Instance.OnLocalPlayerReadyChanged += KitchenGameManager_LocalPlayerChanged;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
    }
    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            //Server shut down
            Debug.Log("Detected host disconnection at: " + NetworkManager.Singleton.LocalClientId);
            Hide();
        }
    }
    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
        }
    }
    private void KitchenGameManager_LocalPlayerChanged(object sender, EventArgs e)
    {
       Hide();
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
