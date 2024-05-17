using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent
{

    public event EventHandler OnPickedSomething;
    public event EventHandler <OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs {
        public BaseCounter selectedCounter;

    }

    public static Player Instance {get; private set;}

    [SerializeField] private float moveSpeed = 7f; // SerializeField so you can change it from the unity interface
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Transform kitchenObjectHoldPoint;

    private bool isWalking;
    private Vector3 lastInteractedDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;


    private void Awake() {
        if (Instance != null) {
            Debug.LogError("This should not happen");
        }
        Instance = this;
    }

    private void Start() {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if (!GameManager.Instance.isGamePlaying()) return;
        
        if (selectedCounter != null) {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (!GameManager.Instance.isGamePlaying()) return;

        if (selectedCounter != null) {
            selectedCounter.Interact(this);
        }
    }

    // Get input first then process the logic after
    private void Update() {
        HandleMovement();
        HandleInteractions();
        

    }

    public bool IsWalking() {
        return isWalking;

    }

    public void HandleInteractions() {
        Vector2 inputVector = gameInput.getMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        float interactDistance = 2f;

        if (moveDir != Vector3.zero) {
            lastInteractedDir = moveDir;
        }
        

        if (Physics.Raycast(transform.position, lastInteractedDir, out RaycastHit rayCastHit, interactDistance)) {
            if (rayCastHit.transform.TryGetComponent(out BaseCounter baseCounter)) {
                if (baseCounter != selectedCounter) {
                    SetSelectedCounter(baseCounter);
                }
            }
            else {
                SetSelectedCounter(null);
            }
        }
        else {
            SetSelectedCounter(null);
        }
    }

    public void HandleMovement() {
        Vector2 inputVector = gameInput.getMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        
        isWalking = moveDir != Vector3.zero; // Define when it is walking and when it is walking
        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, -moveDir, Time.deltaTime * rotateSpeed); // for player to look in the direction he is moving in

        // Collisions
        float playerRadius = .7f;
        float playerHeight = .2f;
        float moveDistance = moveSpeed * Time.deltaTime;


        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

        if (!canMove) {
            
            // Attempt only X direction if cannot move in forward direction
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            bool canMoveX = moveDir.x !=0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);

            if (canMoveX) {
                transform.position += moveDirX * Time.deltaTime * moveSpeed;
            }

            else {
                // Attempt only Z direction if cannot move in X direction
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                bool canMoveZ = moveDir.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);
                if (canMoveZ) {
                    transform.position += moveDirZ * Time.deltaTime * moveSpeed;
            }
            }
        }

        if (canMove) {
            transform.position += moveDir * Time.deltaTime * moveSpeed; 
        }
    }

    private void SetSelectedCounter(BaseCounter selectedCounter) {
        this.selectedCounter = selectedCounter;
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs {
            selectedCounter = selectedCounter
        });

    }

    public Transform GetKitchenObjectFollowTransform() {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject) {
        this.kitchenObject = kitchenObject;
        
        if (kitchenObject != null) {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject() {
        return kitchenObject;
    }

    public void ClearKitchenObject() {
        kitchenObject = null;
    }

    public bool HasKitchenObject() {
        if (kitchenObject == null) {
            return false;   
        }
        else {
            return true;
        }
    }
}
