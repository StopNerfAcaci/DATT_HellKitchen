using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    [Serializable]
    public struct KitchenObjSO_GameObject
    {
        public KitchenObjectSO kitchenObjectSO;
        public GameObject obj;
    }
    [SerializeField] private PlateKitchenObject plateKitchenObject;
    [SerializeField] private List<KitchenObjSO_GameObject> kitchenObjSOGOList;
    private void Start()
    {
        plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;
        foreach (KitchenObjSO_GameObject k in kitchenObjSOGOList)
        {
            k.obj.SetActive(false);
        }
    }

    private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        foreach(KitchenObjSO_GameObject k in kitchenObjSOGOList)
        {
            if(k.kitchenObjectSO == e.kitchenObjSO)
            {
                k.obj.SetActive(true);
            }
        }
    }
}
