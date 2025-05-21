using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliverCounter : BaseCounter
{
    public static DeliverCounter Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    public override void Interact(Player player)
    {
        base.Interact(player);
        if (player.HasKitchenObj())
        {
            if(player.GetKitchenObj().TryGetPlate(out PlateKitchenObject plate)){
                //Only accept recipe plates
                DeliveryManager.Instance.DeliveryRecipe(plate);
                KitchenObject.DestroyKitchenObject(player.GetKitchenObj());
            }
        }
    }
}
