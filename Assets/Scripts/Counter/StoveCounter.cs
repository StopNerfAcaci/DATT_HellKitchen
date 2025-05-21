using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static CuttingCounter;

public class StoveCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangeEventArgs> OnProgressChanged;
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;

    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }
    public enum State
    {
        Idle, Frying, Fried, Burned
    }
    // Start is called before the first frame update
    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;
    private NetworkVariable<State> state = new NetworkVariable<State>(State.Idle);
    private NetworkVariable<float> fryingTime = new NetworkVariable<float>(0f);
    private NetworkVariable<float> burningTime = new NetworkVariable<float>(0f);
    FryingRecipeSO fryingRecipeSO;
    BurningRecipeSO burningRecipeSO;

    public override void OnNetworkSpawn()
    {
        fryingTime.OnValueChanged += OnFryingTimeChanged;
        burningTime.OnValueChanged += OnBurningTimeChanged;
        state.OnValueChanged += State_OnValueChanged;
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
        {
            state = state.Value,
        });
        if (state.Value == State.Burned || state.Value == State.Idle)
        {
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
            {
                progressNormalize = 0f

            });
        }
    }

    private void OnBurningTimeChanged(float previousValue, float newValue)
    {
        float burningTimerMax = burningRecipeSO != null ? burningRecipeSO.burningTimeMax : 1f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
        {
            progressNormalize = (float)burningTime.Value / burningTimerMax

        });
    }

    private void OnFryingTimeChanged(float previousValue, float newValue)
    {
        float fryingTimerMax = fryingRecipeSO!= null? fryingRecipeSO.fryingTimeMax:1f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangeEventArgs
        {
            progressNormalize = (float)fryingTime.Value / fryingTimerMax

        });
    }
    private void Start()
    {
        SetStateIdleServerRpc();
    }
    private void Update()
    {
        if (!IsServer) return;
        if (HasKitchenObj())
        {
            switch (state.Value)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    fryingTime.Value += Time.deltaTime;

                    if (fryingTime.Value > fryingRecipeSO.fryingTimeMax)
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObj());
                        KitchenObject.SpawnKitchenObj(fryingRecipeSO.output, this);

                        state.Value = State.Fried;
                        burningTime.Value = 0;
                        SetBurningRecipeSOClientRpc(KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(GetKitchenObj().GetKitchenObjectSO()));

                    }
                    break;
                case State.Fried:
                    burningTime.Value += Time.deltaTime;
                    if (burningTime.Value > burningRecipeSO.burningTimeMax)
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObj());
                        KitchenObject.SpawnKitchenObj(burningRecipeSO.output, this);
                        state.Value = State.Burned;

                    }

                    break;
                case State.Burned:
                    break;
            }
        }
    }
    public override void Interact(Player player)
    {
        base.Interact(player);
        Debug.Log("Interact with stove");
        if (!HasKitchenObj())
        {
            if (player.HasKitchenObj())
            {
                if (HasRecipeWithInput(player.GetKitchenObj().GetKitchenObjectSO()))
                {
                    KitchenObject kitchenObject = player.GetKitchenObj();
                    kitchenObject.SetKitchenObjParent(this);
                    InteractLogicPlaceObjectOnCounterServerRpc(KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObject.GetKitchenObjectSO()));
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
                if (player.GetKitchenObj().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    if (plateKitchenObject.TryAddIngredients(GetKitchenObj().GetKitchenObjectSO()))
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObj());
                        SetStateIdleServerRpc();
                    }
                }
            }
            else
            {
                GetKitchenObj().SetKitchenObjParent(player);
                SetStateIdleServerRpc();
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetStateIdleServerRpc()
    {
        state.Value = State.Idle;
    }
    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenObjectSOIndex)
    {

        fryingTime.Value = 0;
        state.Value = State.Frying;
        SetFryingRecipeSOClientRpc(kitchenObjectSOIndex);
    }
    [ClientRpc]
    private void SetFryingRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        fryingRecipeSO = GetFryingRecipeSOWithInput(kitchenObjectSO);

    }
    [ClientRpc]
    private void SetBurningRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        burningRecipeSO = GetBurningRecipeSOWithInput(kitchenObjectSO);

    }
    private bool HasRecipeWithInput(KitchenObjectSO input)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(input);
        return fryingRecipeSO != null;
    }
    private KitchenObjectSO GetOutputForInput(KitchenObjectSO input)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(input);
        if (fryingRecipeSO != null)
        {
            return fryingRecipeSO.output;
        }
        return null;
    }
    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO input)
    {
        foreach (FryingRecipeSO fryingRecipeSO in fryingRecipeSOArray)
        {
            if (fryingRecipeSO.input == input)
            {
                return fryingRecipeSO;
            }
        }
        return null;
    }
    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO input)
    {
        foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOArray)
        {
            if (burningRecipeSO.input == input)
            {
                return burningRecipeSO;
            }
        }
        return null;
    }
}
