using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface IKitchenObjParent
{
    public Transform GetKitchenObjFollowTransform();
    public void SetKitchenObj(KitchenObject kitchen);
    public KitchenObject GetKitchenObj();
    public void ClearKitchenObj();
    public bool HasKitchenObj();
    public NetworkObject GetNetworkObject();
}
