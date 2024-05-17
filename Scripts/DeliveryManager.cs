using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;

    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;

    //Singleton
    public static DeliveryManager Instance {get; private set; }

    [SerializeField] private RecipeListSO recipeListSO;

    private List<RecipeSO> waitingRecipeList;
    private int waitingRecipesMax = 4;
    private int successfulRecipesAmount;

    private float spawnRecipeTimer;
    private float recipeTimerMax = 4f;


    private void Awake() {
        Instance = this; 

        waitingRecipeList = new List<RecipeSO>();
    }
    private void Update() {
        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer < 0f) {
            spawnRecipeTimer = recipeTimerMax;

            if (waitingRecipeList.Count < waitingRecipesMax) {
                RecipeSO waitingRecipeSO = recipeListSO.RecipeSOList[UnityEngine.Random.Range(0, recipeListSO.RecipeSOList.Count)];
                
                waitingRecipeList.Add(waitingRecipeSO);

                OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
            }   
        }
    }

    public void DeliverRecipe (PlateKitchenObject plateKitchenObject) {
        for (int i = 0; i < waitingRecipeList.Count ; i++) {
            RecipeSO waitingRecipeSO = waitingRecipeList[i];

            // Check if number of ingredients are the same
            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count) {
                bool ingredientFound = true;
                foreach (KitchenObjectSO kitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList()) {
                    if (!waitingRecipeSO.kitchenObjectSOList.Contains(kitchenObjectSO)) {
                        ingredientFound = false;
                        break;
                    }
                }
                if (ingredientFound) {
                    // Correct Recipe!
                    successfulRecipesAmount += 1;
                    waitingRecipeList.RemoveAt(i);

                    OnRecipeCompleted?.Invoke(this, EventArgs.Empty);

                    OnRecipeSuccess?.Invoke(this,EventArgs.Empty);
                    
                    return;
                }
                
            }
            OnRecipeFailed?.Invoke(this,EventArgs.Empty);
            
        }
    }

    public List<RecipeSO> GetWaitingRecipeSOList() {
        return waitingRecipeList; 
    }

    public int GetSuccessfulRecipesAmount() {
        return successfulRecipesAmount;
    }



}
