using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;


    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;
    public bool isDirtyPlate;
    private float spawnPlatesTimer;
    public float spawnPlateTimerMax = 8f;
    private int plateSpawnedAmount;
    private int plateSpawnedAmountMax = 4;
    public override void Interact(Player player)
    {
        base.Interact(player);
        if (!player.HasKitchenObj())
        {
            if(plateSpawnedAmount >0)
            {
                KitchenObject.SpawnKitchenObj(plateKitchenObjectSO,player, isDirtyPlate);
                InteractLogicServerRpc();
            }
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
        plateSpawnedAmount--;
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }
    private void Update()
    {
        if (!IsServer) return;
        spawnPlatesTimer += Time.deltaTime;
        if(spawnPlatesTimer > spawnPlateTimerMax)
        {
            spawnPlatesTimer = 0;
            if (plateSpawnedAmount < plateSpawnedAmountMax)
            {
                SpawnPlateServerRpc();
            }

        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlateServerRpc()
    {
        SpawnPlateClientRpc();
    }
    [ClientRpc]
    private void SpawnPlateClientRpc()
    {
        plateSpawnedAmount++;
        OnPlateSpawned?.Invoke(this, EventArgs.Empty);
    }
}
