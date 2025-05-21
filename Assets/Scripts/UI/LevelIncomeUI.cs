using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelIncomeUI : MonoBehaviour
{
    public static LevelIncomeUI Instance;
    [SerializeField] private TextMeshProUGUI value;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        value.text = "0";
    }
    public void GetIncomeUI(string income)
    {
        value.text = income;
    }
}
