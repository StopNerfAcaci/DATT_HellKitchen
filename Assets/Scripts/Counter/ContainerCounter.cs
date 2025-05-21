using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class ContainerCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjSO;

    public event EventHandler OnPlayerGrabObj;

    public override void Interact(Player player)
    {
        base.Interact(player);
        if (!player.HasKitchenObj())
        {
            KitchenObject.SpawnKitchenObj(kitchenObjSO,player);
            InteractLogicServerRpc();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();
    }
    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        OnPlayerGrabObj?.Invoke(this, EventArgs.Empty);
    }
}
