using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DeliveryManager;

public class DeliveryManagerUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    //Prefab
    [SerializeField] private Transform recipeTemplate;

    private void Awake()
    {
        recipeTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSpawned += DeliveryManager_OnRecipeSpawned;
        DeliveryManager.Instance.OnRecipeExpired += DeliveryManager_OnRecipeExpired;
        DeliveryManager.Instance.OnRecipeCompleted += DeliveryManager_OnRecipeCompleted;
    }

    private void DeliveryManager_OnRecipeExpired(object sender, System.EventArgs e)
    {
        Debug.Log("Expire recipe");
        RemoveRecipe(1);
        //UpdateVisual();
    }

    private void DeliveryManager_OnRecipeSpawned(object sender, System.EventArgs e)
    {
       // Debug.Log("Spawn recipe");
        CreateRecipe();
    }

    private void DeliveryManager_OnRecipeCompleted(object sender, RecipeCompleteEventArgs e)
    {
        Debug.Log("Complete recipe");
        RemoveRecipe(e.RecipeIndex+1);
    }

    private void CreateRecipe()
    {
        RecipeSO lastRecipe = DeliveryManager.Instance.GetWaitingRecipeSOList()[DeliveryManager.Instance.GetWaitingRecipeSOList().Count-1];
        Transform recipeTransform = Instantiate(recipeTemplate, container);
        recipeTransform.gameObject.SetActive(true);
        recipeTransform.GetComponent<DeliveryMangerSingleUI>().SetRecipeSO(lastRecipe);
    }
    private void RemoveRecipe(int index)
    {
        Destroy(container.GetChild(index).gameObject);
    }
}
