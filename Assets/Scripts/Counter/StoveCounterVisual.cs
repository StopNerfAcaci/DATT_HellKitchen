using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounterVisual : MonoBehaviour
{
    [SerializeField] private StoveCounter stove;
    [SerializeField] GameObject stoveOnGO;
    [SerializeField] GameObject particleGO;

    private void Start()
    {
        stove.OnStateChanged += Stove_OnStateChanged;
    }

    private void Stove_OnStateChanged(object sender, StoveCounter.OnStateChangedEventArgs e)
    {
        bool showVisual = e.state == StoveCounter.State.Frying || e.state == StoveCounter.State.Fried;
        stoveOnGO.SetActive(showVisual);
        particleGO.SetActive(showVisual);
    }
}
