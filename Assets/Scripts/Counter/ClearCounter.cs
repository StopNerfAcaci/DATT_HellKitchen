using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    //[SerializeField] private KitchenObjectSO kitchenObjSO;

    public override void Interact(Player player)
    {
        base.Interact(player);
        if (!HasKitchenObj())
        {
            if (player.HasKitchenObj())
            {
                player.GetKitchenObj().SetKitchenObjParent(this);
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
                if(player.GetKitchenObj().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    if (plateKitchenObject.TryAddIngredients(GetKitchenObj().GetKitchenObjectSO()))
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObj());
                    }
                }
                else
                {
                    if (GetKitchenObj().TryGetPlate(out PlateKitchenObject plate))
                    {
                        //Counter is holding a plate
                        if (plate.TryAddIngredients(player.GetKitchenObj().GetKitchenObjectSO()))
                        {
                            KitchenObject.DestroyKitchenObject(player.GetKitchenObj());
                        }
                    }
                }
            }
            else
            {
                GetKitchenObj().SetKitchenObjParent(player);
            }
        }
    }
}
