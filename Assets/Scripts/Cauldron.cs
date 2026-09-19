using UnityEngine;

// Requires a sequence of items to make contact with its collider to pass and activate the next room
public class Cauldron : MonoBehaviour
{

    [SerializeField] private GameObject finishCutscene; // Cutscene to activate when completed correctly
    [SerializeField] private Recipe recipe;

    private int currentStep = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (finishCutscene == null)
        {
            throw new System.Exception("The cauldron has no finish object to activate!!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check for ingredient component
        Ingredient ingredient = other.GetComponent<Ingredient>();
        if (ingredient == null) return; 

        // Is it the ingredient the recipe wants next?
        if (ingredient.type == recipe.steps[currentStep])
        {
            currentStep++;
            Debug.Log($"Correct! Progress: {currentStep}/{recipe.steps.Length}");

            Destroy(other.gameObject); // consume the ingredient

            if (currentStep >= recipe.steps.Length)
            {
                OnRecipeComplete();
            }
        }
        else
        {
            Debug.Log("Wrong ingredient!");
            currentStep = 0;
        }
    }

    private void OnRecipeComplete()
    {
        Debug.Log($"{recipe.recipeName} complete!");
        finishCutscene.SetActive(true);
    }
}
