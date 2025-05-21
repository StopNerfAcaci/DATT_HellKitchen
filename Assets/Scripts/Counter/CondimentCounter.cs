using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class CondimentCounter : BaseCounter, IHasProgress
{
    [SerializeField] private KitchenObjectSO kitchenObjSO;
    public event EventHandler OnPlayerPourObj;
    public event EventHandler<IHasProgress.OnProgressChangeEventArgs> OnProgressChanged;

    private NetworkVariable<float> pouringTime = new NetworkVariable<float>(0f);
    private float pouringTimeMax = 1f;

    public override void OnNetworkSpawn()
    {
        pouringTime.OnValueChanged += Pouring_OnValueChanged;
    }

    private void Pouring_OnValueChanged(float previousValue, float newValue)
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
        {
            progressNormalize = (float)pouringTime.Value / pouringTimeMax

        });
    }

    public override void Interact(Player player)
    {
        base.Interact(player);
        if (!canInteract) return;
        if (!HasKitchenObj())
        {
            if (player.HasKitchenObj())
            {
                if (player.GetKitchenObj().TryGetPlate(out PlateKitchenObject plateKitchenObject)) {
                    player.GetKitchenObj().SetKitchenObjParent(this);
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
                if(GetKitchenObj().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    if (plateKitchenObject.TryAddIngredients(GetKitchenObj().GetKitchenObjectSO())){
                        KitchenObject.DestroyKitchenObject(player.GetKitchenObj());
                    }
                }
            }
            else
            {
                GetKitchenObj().SetKitchenObjParent(player);
            }
        }
    }
    public override void InteractAlt(Player player)
    {
        base.InteractAlt(player);
        if (!canInteract) return;
        
        if (HasKitchenObj() && GetKitchenObj().TryGetPlate(out PlateKitchenObject plateKO))
        {
            if (plateKO.GetKitchenObjectSOList().Contains(kitchenObjSO))
            {
                Debug.Log("Don't watse ingredients");
                return;
            }
            
            StartCoroutine(AddCondimentAfterVisual(plateKO));
        }
        
    }
    IEnumerator AddCondimentAfterVisual(PlateKitchenObject plateKO)
    {
        canInteract = false;
        Player.LocalInstance.isPouring = true;
        pouringTime.Value = 0;
        PouringSauceServerRpc();
        yield return new WaitForSeconds(pouringTimeMax);
        plateKO.TryAddIngredients(kitchenObjSO);
        // Explicitly set to exactly 1 to ensure the UI updates correctly
        pouringTime.Value = 1f;

        // Additional delay to ensure UI has time to process
        yield return new WaitForEndOfFrame();
        canInteract = true;
        Player.LocalInstance.isPouring = false;
    }
    [ServerRpc(RequireOwnership = false)]
    private void PouringSauceServerRpc()
    {
        PouringSauceClientRpc();
    }
    [ClientRpc]
    private void PouringSauceClientRpc()
    {
        OnPlayerPourObj?.Invoke(this, EventArgs.Empty);
    }
    private void Update()
    {
        if (!IsServer) return;
        if (!canInteract)
        {
            pouringTime.Value += Time.deltaTime;
        }
    }
}
