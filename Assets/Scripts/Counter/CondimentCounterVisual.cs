using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CondimentCounterVisual : MonoBehaviour
{
    private const string POURING = "Pouring";
    [SerializeField] private CondimentCounter container;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        container.OnPlayerPourObj += Container_OnPlayerGrabObj;
    }

    private void Container_OnPlayerGrabObj(object sender, EventArgs e)
    {
        animator.SetTrigger(POURING);
    }
}
