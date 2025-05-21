using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DeliveryManager;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager instance;

    public int levelMoney = 0;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, RecipeSuccessEventArgs e)
    {
        levelMoney += e.Money;
        Debug.Log("Money: "+e.Money);
        LevelIncomeUI.Instance.GetIncomeUI(levelMoney.ToString());
    }
    public int GetLevelMoney()
    {
        return levelMoney;
    }
}
