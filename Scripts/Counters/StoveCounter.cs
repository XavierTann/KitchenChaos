using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress {
    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    // Progress Bar Event
    public event EventHandler <IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs {
        public State state;
    }

    public enum State {
        Idle,
        Frying,
        Fried,
        Burned,
    }
    
    private State state;
    private float fryingTimer;
    private float burningTimer;
    private FryingRecipeSO fryingRecipeSO;
    private BurningRecipeSO burningRecipeSO;

    private void Start() {
        state = State.Idle;
    }
    
    private void Update() {
        if (HasKitchenObject()) {
            switch (state) {
            case State.Idle:
                break;
            case State.Frying:
                fryingTimer += Time.deltaTime;

                // Invoke triggers the event
                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = fryingTimer/ fryingRecipeSO.fryingTimerMax
                    });

                if (fryingRecipeSO.fryingTimerMax < fryingTimer) {
                    GetKitchenObject().DestroySelf();

                    KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this); 

                    state = State.Fried;
                    burningTimer = 0f;
                    burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
                    
                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
                        state = state
                    });

                }
                break;
            
            case State.Fried:
                burningTimer += Time.deltaTime;

                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = burningTimer/ burningRecipeSO.burningTimerMax
                    });

                if (burningRecipeSO.burningTimerMax < burningTimer) {
                    GetKitchenObject().DestroySelf();

                    KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this); 


                    state = State.Burned;
                    
                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
                        state = state
                    });
                    
                    // Make sure that timer hides itself after it is burnt
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = 0f
                    });
                }
        
                break;
            case State.Burned:
                break;
            }
        }
    }

    public override void Interact(Player player)
    { 
        if (!HasKitchenObject()) {
            // No Kitchen Object on counter
            if (player.HasKitchenObject()) {
                // PLayer has kitchen Object
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) {
                    // Player is carrying something that can be fried
                     player.GetKitchenObject().SetKitchenObjectParent(this);

                    fryingRecipeSO = GetFryingRecipeSOWithInput (GetKitchenObject().GetKitchenObjectSO());

                    state = State.Frying;
                    fryingTimer = 0f;
                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
                        state = state
                    });

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = fryingTimer/ fryingRecipeSO.fryingTimerMax
                    });


                     
                }
            } else {
                //Player is not carryihng anything
            }
        }
        else {
            // There is a KitchenObject on the counter
            if (player.HasKitchenObject()) {
                // If player is holding a plate 
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)) { 
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) {
                        GetKitchenObject().DestroySelf();
                        // Set state back to idle
                    state = State.Idle;
                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
                         state = state
                    });
                
                    // Make sure that timer hides itself after it is burnt
                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                            progressNormalized = 0f
                        });
                        }
                    }
            } else {
                // Player is not carrying anything, pick up object.

                GetKitchenObject().SetKitchenObjectParent(player);

                // Set state back to idle
                state = State.Idle;
                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
                        state = state
                    });
                
                // Make sure that timer hides itself after it is burnt
                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = 0f
                    });
            }
        }

    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO) {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        return fryingRecipeSO != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO) {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        if (fryingRecipeSO != null) {
            return fryingRecipeSO.output; 
        }
        else{return null;} 
    }

    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO) {
        foreach (FryingRecipeSO fryingRecipeSO in fryingRecipeSOArray) {
            if (fryingRecipeSO.input == inputKitchenObjectSO) {
                return fryingRecipeSO;
            }
        }
    return null; 
    }

    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO) {
        foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOArray) {
            if (burningRecipeSO.input == inputKitchenObjectSO) {
                return burningRecipeSO;
            }
        }
    return null; 
    }
    
}
