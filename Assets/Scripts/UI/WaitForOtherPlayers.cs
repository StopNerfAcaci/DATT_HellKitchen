using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WaitForOtherPlayers : MonoBehaviour
{
    private void Start()
    {
        KitchenGameManager.Instance.OnLocalPlayerReadyChanged += KitchenGameManager_LocalPlayerChanged;
        KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        Hide();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            //Server shut down
            Debug.Log("Detected host disconnection at: "+NetworkManager.Singleton.LocalClientId);
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
    private void KitchenGameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (KitchenGameManager.Instance.IsCountDownToStartActive())
        {
            Hide();
        }
    }

    private void KitchenGameManager_LocalPlayerChanged(object sender, EventArgs e)
    {
        if (KitchenGameManager.Instance.IsCountDownToStartActive())
        {
            // Always hide during countdown
            Hide();
        }
        else if (KitchenGameManager.Instance.IsLocalPlayerReady())
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
