using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BasinCounter : BaseCounter,IHasProgress
{
    public event EventHandler OnPlateAddedToSink;
    public event EventHandler OnPlateAddedToClean;
    public event EventHandler OnPlateRemoved;
    public event EventHandler OnWash;
    public event EventHandler<IHasProgress.OnProgressChangeEventArgs> OnProgressChanged;

    [SerializeField] protected Transform cleanCounterTopPoint;
    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;
    private int washingProgress;
    private int dirtyPlateAmount = 0;
    private int cleanPlateAmount = 0;
    public override void Interact(Player player)
    {
        base.Interact(player);
        if (player.HasKitchenObj())
        {
            if (player.GetKitchenObj().TryGetPlate(out PlateKitchenObject plateKO))
            {
                if (plateKO.isDirty)
                {
                    //washingProgress = 0;
                    InteractLogicPlaceObjectOnCounterServerRpc();

                    KitchenObject.DestroyKitchenObject(player.GetKitchenObj());
                    AddPlateToSinkServerRpc();
                }
            }
        }
        else
        {
            if (cleanPlateAmount > 0)
            {
                
                KitchenObject.SpawnKitchenObj(plateKitchenObjectSO, player);
                RemoveCleanPlateServerRpc();
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
        washingProgress = 0;


        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
        {
            progressNormalize = 0f
        });
    }
    [ServerRpc(RequireOwnership = false)]
    private void AddPlateToSinkServerRpc()
    {
        AddPlateToSinkClientRpc();
    }
    [ClientRpc]
    private void AddPlateToSinkClientRpc()
    {
        dirtyPlateAmount++;
        OnPlateAddedToSink?.Invoke(this, EventArgs.Empty);
    }
    [ServerRpc(RequireOwnership = false)]
    private void RemoveCleanPlateServerRpc()
    {
        RemoveCleanPlateClientRpc();
    }
    [ClientRpc]
    private void RemoveCleanPlateClientRpc()
    {
        cleanPlateAmount--;
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }
    public override void InteractAlt(Player player)
    {
        base.InteractAlt(player);
        if (dirtyPlateAmount>0)
        {
            WashPlateServerRpc();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void WashPlateServerRpc()
    {
        WashPlateClientRpc();
        WashingDoneServerRpc();
    }
    [ClientRpc]
    private void WashPlateClientRpc()
    {
        washingProgress++;
        OnWash?.Invoke(this, EventArgs.Empty);
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
        {
            progressNormalize = (float)washingProgress / 3
        });

    }
    [ServerRpc(RequireOwnership = false)]
    private void WashingDoneServerRpc()
    {
        if (washingProgress >= 3)
        {
            WashingDoneClientRpc();

        }
    }
    [ClientRpc]
    private void WashingDoneClientRpc()
    {
        OnPlateAddedToClean?.Invoke(this, EventArgs.Empty);
        dirtyPlateAmount--;
        cleanPlateAmount++;
        washingProgress = 0;
    }
}
