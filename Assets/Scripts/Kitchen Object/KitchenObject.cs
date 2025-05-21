using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    private FollowTransform followTransform;
    private IKitchenObjParent parent;
    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO; 
    }
    protected virtual void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
    }
    public void SetKitchenObjParent(IKitchenObjParent kparent)
    {
        SetKitchenObjectParentServerRpc(kparent.GetNetworkObject());
    }
    [ServerRpc(RequireOwnership =false)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference parentNetwork)
    {
        SetKitchenObjectParentClientRpc(parentNetwork);
    }
    [ClientRpc]
    private void SetKitchenObjectParentClientRpc(NetworkObjectReference parentNetwork)
    {
        parentNetwork.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
        IKitchenObjParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjParent>();
        if (this.parent != null)
        {
            this.parent.ClearKitchenObj();
        }
        this.parent = kitchenObjectParent;
        if (kitchenObjectParent.HasKitchenObj())
        {
            Debug.LogError("Already has kitchen obj");
        }
        kitchenObjectParent.SetKitchenObj(this);
        followTransform.SetTargetTransform(kitchenObjectParent.GetKitchenObjFollowTransform());
    }
    public IKitchenObjParent GetKitchenObjParent()
    {
        return this.parent;
    }
    public void DestroyItself()
    {
        Destroy(this.gameObject);
    }

    public void ClearKitchenObjectOnParent()
    {
        parent.ClearKitchenObj();
    }
    public static void SpawnKitchenObj(KitchenObjectSO kitchenObjectSO, IKitchenObjParent parent, bool isDirty = false)
    {
        KitchenGameMultiplayer.Instance.SpawnKitchenObj(kitchenObjectSO, parent, isDirty);
    }
    public static void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        KitchenGameMultiplayer.Instance.DestroyKitchenObject(kitchenObject);
    }
    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if (this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        else
        {
            plateKitchenObject = null;
            return false;
        }

    }

}
