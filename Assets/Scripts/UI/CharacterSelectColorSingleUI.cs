using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectColorSingleUI : MonoBehaviour
{
    [SerializeField] private int colorId;
    [SerializeField] private Image image;
    [SerializeField] private GameObject selectedGameObject;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.ChangePlayerColor(colorId);

        });
    }
    private void KitchenGameMultiplayer_OnPlayerDataNetworkChanged(object sender, System.EventArgs e)
    {
        UpdateIsSelected();
    }

    private void Start()
    {
        KitchenGameMultiplayer.Instance.OnPlayerDataNetworkChanged += KitchenGameMultiplayer_OnPlayerDataNetworkChanged;
        image.color = KitchenGameMultiplayer.Instance.GetPlayerColor(colorId);
        UpdateIsSelected();
    }
    private void OnDestroy()
    {
        KitchenGameMultiplayer.Instance.OnPlayerDataNetworkChanged -= KitchenGameMultiplayer_OnPlayerDataNetworkChanged;
    }
    private void UpdateIsSelected()
    {
        if(KitchenGameMultiplayer.Instance.GetPlayerData().colorId == colorId)
        {
            selectedGameObject.SetActive(true);
        }
        else
        {
            selectedGameObject.SetActive(false);
        }
}
}
