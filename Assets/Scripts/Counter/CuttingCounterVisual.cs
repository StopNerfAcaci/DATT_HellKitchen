using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounterVisual : MonoBehaviour
{
    private const string CUT = "Cut";
    [SerializeField] private CuttingCounter cutting;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        cutting.OnCut += Cutting_OnCut;
    }

    private void Cutting_OnCut(object sender, EventArgs e)
    {
        animator.SetTrigger(CUT);
    }
}
