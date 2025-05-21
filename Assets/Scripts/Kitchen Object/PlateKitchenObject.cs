using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public class OnIngredientAddedEventArgs: EventArgs
    {
        public KitchenObjectSO kitchenObjSO;
    }
    [SerializeField] private List<KitchenObjectSO> validKitchenObjSOList;
    [SerializeField] GameObject dirtyVisual;
    public bool isDirty;
    private List<KitchenObjectSO> kitchenObjSOList;

    protected override void Awake()
    {
        base.Awake();
        kitchenObjSOList = new List<KitchenObjectSO>();
    }
    private void Start()
    {
        dirtyVisual.SetActive(isDirty);
    }
    public bool TryAddIngredients(KitchenObjectSO kitchenObjectSO)
    {
        if (isDirty)
        {
            Debug.Log("Wash it man");
            return false;
        }
        if(!validKitchenObjSOList.Contains(kitchenObjectSO)) return false;
        if (kitchenObjSOList.Contains(kitchenObjectSO)) return false;
        else
        {
            AddIngredientServerRpc(
                KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObjectSO) //Get the index of the kitchen object so
                );
            return true;
        }

    }
    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientServerRpc(int kitchenObjectSOIndex)
    {
        AddIngredientClientRpc(kitchenObjectSOIndex);
    }
    [ClientRpc]
    private void AddIngredientClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        kitchenObjSOList.Add(kitchenObjectSO);
        OnIngredientAdded(this, new OnIngredientAddedEventArgs
        {
            kitchenObjSO = kitchenObjectSO
        });
    }   
    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjSOList;
    }
}
