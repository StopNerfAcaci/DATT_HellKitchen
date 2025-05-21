using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BurnRecipe", menuName = "KitchenObject/New Burning Obj")]
public class BurningRecipeSO : ScriptableObject
{
    public KitchenObjectSO input;
    public KitchenObjectSO output;
    public int burningTimeMax;
}
