using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestNetcodeUI : MonoBehaviour
{
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    private void Awake()
    {
        startHostButton.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.StartClient();
            Debug.Log("Host started");
            Hide();
        });
        startClientButton.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.StartClient();
            Debug.Log("Client started");
            Hide();
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
