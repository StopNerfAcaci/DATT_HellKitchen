using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgress
{
    public static event EventHandler OnAnyCut;
    public static void ResetCutStaticData()
    {
        OnAnyCut = null;
    }
    [SerializeField] private CuttingRecipeSO[] cutKitchenObjSOArray;
    public event EventHandler<IHasProgress.OnProgressChangeEventArgs> OnProgressChanged;
    public event EventHandler OnCut;
    private int cuttingProgress;
    public override void Interact(Player player)
    {
        base.Interact(player);
        if (!HasKitchenObj())
        {
            if (player.HasKitchenObj())
            {
                if (HasRecipeWithInput(player.GetKitchenObj().GetKitchenObjectSO()))
                {
                    KitchenObject kitchenObject = player.GetKitchenObj();
                    kitchenObject.SetKitchenObjParent(this);
                    InteractLogicPlaceObjectOnCounterServerRpc();
                }
            }
            else
            {
                //Player not carry anythg
            }
        }
        else
        {
            if (player.HasKitchenObj())
            {
                if (player.GetKitchenObj().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    if (plateKitchenObject.TryAddIngredients(GetKitchenObj().GetKitchenObjectSO()))
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObj());
                    }
                }
            }
            else
            {
                GetKitchenObj().SetKitchenObjParent(player);
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc()
    {
        InteractLogicPlaceObjectOnCounterClientRpc();
    }
    [ClientRpc]
    private void InteractLogicPlaceObjectOnCounterClientRpc()
    {
        cuttingProgress = 0;


        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
        {
            progressNormalize = 0f
        });
    }
    public override void InteractAlt(Player player)
    {
        base.InteractAlt(player);
        if (HasKitchenObj() && HasRecipeWithInput(GetKitchenObj().GetKitchenObjectSO()))
        {
            CutObjectServerRpc();
            CuttingProgressDoneServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc()
    {
        CutObjectClientRpc();
    }
    [ClientRpc]
    private void CutObjectClientRpc()
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObj().GetKitchenObjectSO());
        if(cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
        {
            Debug.LogError("Already cut");
            return;
        }
        cuttingProgress++;

        OnCut?.Invoke(this, EventArgs.Empty);
        OnAnyCut?.Invoke(this, EventArgs.Empty);

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
        {
            progressNormalize = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
        });

    }
    [ServerRpc(RequireOwnership = false)]
    private void CuttingProgressDoneServerRpc()
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObj().GetKitchenObjectSO());
        if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
        {
            KitchenObjectSO output = GetOutputForInput(GetKitchenObj().GetKitchenObjectSO());
            KitchenObject.DestroyKitchenObject(GetKitchenObj());
            KitchenObject.SpawnKitchenObj(output, this);

        }
    }
    private bool HasRecipeWithInput(KitchenObjectSO input)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(input);
        return cuttingRecipeSO != null;
    }
    private KitchenObjectSO GetOutputForInput(KitchenObjectSO input)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(input);
        if (cuttingRecipeSO != null)
        {
           return cuttingRecipeSO.output;
        }
        return null;
    }
    private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO input)
    {
        foreach (CuttingRecipeSO cuttingRecipeSO in cutKitchenObjSOArray)
        {
            if (cuttingRecipeSO.input == input)
            {
                return cuttingRecipeSO;
            }
        }
        return null;
    }
}
