using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerCounterVisual : MonoBehaviour
{
    private const string OPEN_CLOSE = "OpenClose";
    [SerializeField] private ContainerCounter container;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        container.OnPlayerGrabObj += Container_OnPlayerGrabObj;
    }

    private void Container_OnPlayerGrabObj(object sender, EventArgs e)
    {
        animator.SetTrigger(OPEN_CLOSE);
    }
}
