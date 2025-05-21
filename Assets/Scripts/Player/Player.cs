using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : NetworkBehaviour, IKitchenObjParent
{
    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyPickSomething;
    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
    }
    public static Player LocalInstance { get; set; }

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs: EventArgs
    {
        public BaseCounter selectedCounter;
    }

    public event EventHandler OnPickSomething;
    [SerializeField] private float speed = 7f;
    [SerializeField] private Transform pickUpPoint;
    [SerializeField] private LayerMask counterMask;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private List<Vector3> playerSpawnList;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private GameObject lightGO;

    [HideInInspector] public bool isPouring = false;
    private bool isWalking;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObj;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }
        transform.position = playerSpawnList[KitchenGameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)];
        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
        SetSelectedCounter(null);
        if (SceneManager.GetActiveScene().name == "Level_6")
        {
            lightGO.SetActive(true);
        }
        else
        {
            lightGO.SetActive(false);
        }
    }
    public override void OnNetworkDespawn()
    {
        if (selectedCounter != null)
        {
            selectedCounter = null;
        }
    }
    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if(clientId == OwnerClientId && HasKitchenObj())
        {
            KitchenObject.DestroyKitchenObject(GetKitchenObj());
        }
    }

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAltAction += GameInput_OnInteractAltAction;

        PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerColor(KitchenGameMultiplayer.Instance.GetPlayerColor(playerData.colorId));
    }

    private void GameInput_OnInteractAltAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying())
        {
            return;
        }
        if (selectedCounter != null)
        {
            selectedCounter.InteractAlt(this);
        }
    }
    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {

        if (!KitchenGameManager.Instance.IsGamePlaying())
        {
            return;
        }
        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (KitchenGameManager.Instance.IsGameOver())
        {
            isWalking = false;
            return;
        }
        if(isPouring) return;
        HandleInteraction();
        HandleMovement();
    }
    public bool IsWalking()
    {
        return isWalking;
    }
    private void HandleInteraction()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVector();
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);
        if (moveDir != Vector3.zero)
        {
            lastInteractDir = moveDir.normalized;
        }
        float interactMovement = 1.2f;
        
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycast, interactMovement, counterMask))
        {
            if (raycast.transform.TryGetComponent(out BaseCounter clearCounter))
            {
                if (clearCounter != selectedCounter)
                {
                    SetSelectedCounter(clearCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }
    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVector();
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);

       // float playerHeight = 2f;
        float playerRadius = .6f;
        float moveDistance = speed * Time.deltaTime;
        bool canMove =  !Physics.BoxCast(transform.position, Vector3.one* playerRadius, moveDir, Quaternion.identity, moveDistance, collisionMask);

        if (!canMove)
        {
            //Attempt only X movement
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = moveDir.x != 0 && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirX, Quaternion.identity, moveDistance, collisionMask);
            if (canMove)
            {
                moveDir = moveDirX;
            }
            else
            {
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = moveDir.z != 0 && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirZ, Quaternion.identity, moveDistance, collisionMask);
                if (canMove)
                {
                    moveDir = moveDirZ;
                }
            }
        }
        if (canMove)
        {
            transform.position += moveDir * speed * Time.deltaTime;
        }
        isWalking = moveDir != Vector3.zero;
        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, rotateSpeed * Time.deltaTime);
    }
    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = selectedCounter,
        });
    }
    public Transform GetKitchenObjFollowTransform()
    {
        return pickUpPoint;
    }
    public void SetKitchenObj(KitchenObject kitchen)
    {
        this.kitchenObj = kitchen;
        if (kitchen != null)
        {
            OnPickSomething?.Invoke(this, EventArgs.Empty);
            OnAnyPickSomething?.Invoke(this, EventArgs.Empty);
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
