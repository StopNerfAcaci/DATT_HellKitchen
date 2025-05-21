using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseCounter : NetworkBehaviour, IKitchenObjParent
{
    public static event EventHandler OnAnyObjectPlaceHere;
    protected bool canInteract = true;
    public static void ResetStaticData()
    {
        OnAnyObjectPlaceHere = null;
    }
    [SerializeField] protected Transform counterTopPoint;
    private KitchenObject kitchenObj;
    public virtual void Interact(Player player)
    {
    }
    public virtual void InteractAlt(Player player)
    {
    }
    public Transform GetKitchenObjFollowTransform()
    {
        return counterTopPoint;
    }
    public void SetKitchenObj(KitchenObject kitchen)
    {
        this.kitchenObj = kitchen;
        if (kitchenObj != null)
        {
            OnAnyObjectPlaceHere?.Invoke(this, EventArgs.Empty);
        }
    }
    public KitchenObject GetKitchenObj()
    {
        return kitchenObj;
    }
    public void ClearKitchenObj()
    {
        kitchenObj = null;
    }
    public bool HasKitchenObj()
    {
        return kitchenObj != null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
