using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Profiling;

public class DeliveryManager : NetworkBehaviour
{
    public static DeliveryManager Instance { get; private set; }

    public event EventHandler OnRecipeSpawned;
    public event EventHandler<RecipeCompleteEventArgs> OnRecipeCompleted;
    public event EventHandler<RecipeSuccessEventArgs> OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;
    public event EventHandler OnRecipeExpired;

    public class RecipeCompleteEventArgs : EventArgs
    {
        public int RecipeIndex { get; }

        public RecipeCompleteEventArgs(int recipeIndex)
        {
            RecipeIndex = recipeIndex;
        }
    }
    public class RecipeSuccessEventArgs : EventArgs
    {
        public int Money { get; }

        public RecipeSuccessEventArgs(int money)
        {
            Money = money;
        }
    }

    [SerializeField] private RecipeListSO list;
    [SerializeField] private List<RecipeSO> waitingRecipeSOList;
    [SerializeField] public float expireTime = 10f;
    private List<Order> orderList;
    private float spawnRecipeTimer = 4f;
    private float spawnRecipeTimerMax = 8f;
    private int waitingRecipeMax = 4;

    private int successfulRecipeAmount;
    private int failedRecipeAmount;
    private void Awake()
    {
        Instance = this;
        waitingRecipeSOList = new List<RecipeSO>();
        orderList = new List<Order>();

       // NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        
    }

    private void Update()
    {
        if(!IsServer) return;
        if (!KitchenGameManager.Instance.IsGamePlaying())
        {
            return;
        }
        HandleRecipeSpawn();   
        HandleRecipeExpire();
    }
    private void HandleRecipeSpawn()
    {
        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;
            if (waitingRecipeSOList.Count < waitingRecipeMax)
            {
                int waitingRecipeSOIndex = UnityEngine.Random.Range(0, list.recipeList.Count);
                SpawnNewWaitingRecipeClientRpc(waitingRecipeSOIndex);

            }
        }
    }
    private void HandleRecipeExpire()
    {
        for (int i = waitingRecipeSOList.Count - 1; i >= 0; i--)
        {
            var order = orderList[i];
            if (Time.time - order.remainingTime >= expireTime)
            {
                //waitingRecipeSOList.RemoveAt(i);
                //orderList.RemoveAt(i);
                //OnRecipeExpired?.Invoke(this, EventArgs.Empty);
                ExpireWaitingRecipeClientRpc(i);
            }
        }
    }
    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc(int waitingRecipeSOIndex)
    {
        RecipeSO waitingRecipeSO = list.recipeList[waitingRecipeSOIndex];
        Order newOrder = new Order(waitingRecipeSO, Time.time);
        waitingRecipeSOList.Add(waitingRecipeSO);
        orderList.Add(newOrder);
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }
    [ClientRpc]
    private void ExpireWaitingRecipeClientRpc(int expireRecipeSOIndex)
    {
        failedRecipeAmount++;
        waitingRecipeSOList.RemoveAt(expireRecipeSOIndex);
        orderList.RemoveAt(expireRecipeSOIndex);
        OnRecipeExpired?.Invoke(this, EventArgs.Empty);
    }
    public void DeliveryRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];
            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                bool plateContentMatchesRecipe = true;
                foreach (KitchenObjectSO recipeKitchenObjSO in waitingRecipeSO.kitchenObjectSOList)
                {
                    bool ingredientFound = false;
                    //Cycling through all ingredients
                    foreach (KitchenObjectSO plateKitchenObjSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        if (plateKitchenObjSO == recipeKitchenObjSO)
                        {
                            //Ingredient matches
                            ingredientFound = true;
                            break;
                        }
                    }
                    if (!ingredientFound)
                    {
                        //This recipe ingredient is not in the plate
                        plateContentMatchesRecipe = false;
                    }
                }
                if (plateContentMatchesRecipe)
                {
                    DeliverCorrectRecipeServerRpc(i);
                    return;
                }
            }
        }
        //No matches found
        DeliverIncorrectRecipeServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrectRecipeServerRpc(int watingRecipeSOIndex)
    {
        DeliverCorrectRecipeClientRpc(watingRecipeSOIndex);
    }
    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int watingRecipeSOIndex)
    {
        successfulRecipeAmount++;
        OnRecipeCompleted?.Invoke(this, new RecipeCompleteEventArgs(watingRecipeSOIndex));
        OnRecipeSuccess?.Invoke(this, new RecipeSuccessEventArgs(waitingRecipeSOList[watingRecipeSOIndex].value));
        waitingRecipeSOList.RemoveAt(watingRecipeSOIndex);
        orderList.RemoveAt(watingRecipeSOIndex);
    }
    [ServerRpc(RequireOwnership =false)]
    private void DeliverIncorrectRecipeServerRpc()
    {
        DeliverIncorrectRecipeClientRpc();
    }
    [ClientRpc]
    private void DeliverIncorrectRecipeClientRpc()
    {
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }
    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return waitingRecipeSOList;
    }
    public int GetSuccessfulRecipeAmount()
    {
        return successfulRecipeAmount;
    }
    public int GetFailedRecipeAmount()
    {
        return failedRecipeAmount;
    }
}
public class Order
{
    public Order(RecipeSO recipe, float remainingTime)
    {
        this.recipe = recipe;
        this.remainingTime = remainingTime;
    }
    public RecipeSO recipe;
    public float remainingTime;
}
