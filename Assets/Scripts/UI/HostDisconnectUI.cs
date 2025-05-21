using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HostDisconnectUI : MonoBehaviour
{
    [SerializeField] private Button replayButton;
    private void Awake()
    {
        replayButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
    }
    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientStopped += NetworkManager_OnClientStopped;
        Hide();
    }

    private void NetworkManager_OnClientStopped(bool wasHost)
    {
        if (!wasHost)
        {
            Debug.Log("Client stopped (not host). Showing UI...");
            Show();
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.OnClientStopped -= NetworkManager_OnClientStopped;
        }

    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log("Check if host disconnect");
        if (clientId == NetworkManager.ServerClientId)
        {
            //Server shut down
            Debug.Log("Detected host disconnection");
            Show();
        }

    }

    private void Show()
    {
        Debug.Log("Show host disconnect");
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
