using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static CuttingCounter;

public class TrashCounter : BaseCounter
{
    public static event EventHandler OnAnyObjectTrash;
    public static void ResetTrashStaticData()
    {
        OnAnyObjectTrash = null;
    }
    public override void Interact(Player player)
    {
        base.Interact(player);
        if (player.HasKitchenObj())
        {
            KitchenObject.DestroyKitchenObject(player.GetKitchenObj());
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

        OnAnyObjectTrash?.Invoke(this, EventArgs.Empty);
    }
}
