using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FryRecipe", menuName = "KitchenObject/New Frying Obj")]
public class FryingRecipeSO : ScriptableObject
{
    public KitchenObjectSO input;
    public KitchenObjectSO output;
    public int fryingTimeMax;
}
