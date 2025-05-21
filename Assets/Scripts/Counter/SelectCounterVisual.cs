using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SelectCounterVisual : MonoBehaviour
{
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualGOArray;
    private void Start()
    {
        if(Player.LocalInstance != null)
        {
            Player.LocalInstance.OnSelectedCounterChanged += Instance_OnSelectedCounterChanged;
        }
        else
        {
            Player.OnAnyPlayerSpawned += Player_OnSelectedCounterChanged;
        }

    }

    private void Player_OnSelectedCounterChanged(object sender, EventArgs e)
    {
        if (Player.LocalInstance != null)
        {
            Player.LocalInstance.OnSelectedCounterChanged -= Instance_OnSelectedCounterChanged;
            Player.LocalInstance.OnSelectedCounterChanged += Instance_OnSelectedCounterChanged;
        }
    }

    private void Instance_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e)
    {
        if (e.selectedCounter == baseCounter)
        {
            Show();
        }
        else { Hide(); }
    }
    private void Show()
    {
        foreach(GameObject visualGO in visualGOArray)
        {
            if (visualGO != null)
            {
                visualGO.SetActive(true);
            }
        }

    }
    private void Hide()
    {
        foreach (GameObject visualGO in visualGOArray)
        {
            if (visualGO != null)
            {
                visualGO.SetActive(false);
            }
        }
    }


    private void OnDisable()
    {
        Player.LocalInstance.OnSelectedCounterChanged -= Instance_OnSelectedCounterChanged;
    }
}
