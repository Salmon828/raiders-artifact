using UnityEngine;

// Requires a sequence of items to make contact with its collider to pass and activate the next room
public class Cauldron : MonoBehaviour
{

    [SerializeField] private GameObject finishCutscene; // Cutscene to activate when completed correctly
    [SerializeField] private Recipe recipe;
    [SerializeField] private ParticleSystem brewParticles; // Recolored to match each ingredient added
    [Tooltip("How much darker the particle color gets by the end of its lifetime.")]
    [SerializeField, Range(0f, 1f)] private float endColorDarken = 0.3f;

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

            SetParticleColor(ingredient.color);

            Destroy(other.gameObject); // consume the ingredient

            if (currentStep >= recipe.steps.Length)
            {
                OnRecipeComplete();
            }
        }
        else
        {
            Debug.Log("Wrong ingredient!");
            SetParticleColor(ingredient.color);
            Destroy(other.gameObject); // consume the ingredient
            currentStep = 0;
        }
    }

    // Swaps the color keys of the particles' Color over Lifetime gradient, keeping its existing alpha fade
    private void SetParticleColor(Color color)
    {
        if (brewParticles == null) return;

        var colorOverLifetime = brewParticles.colorOverLifetime;
        Gradient gradient = colorOverLifetime.color.gradient;
        if (gradient == null)
        {
            gradient = new Gradient();
        }

        color.a = 1f; // alpha comes from the gradient's alpha keys, not the ingredient color
        Color endColor = Color.Lerp(color, Color.black, endColorDarken);

        gradient.SetKeys(
            new[] { new GradientColorKey(color, 0f), new GradientColorKey(endColor, 1f) },
            gradient.alphaKeys
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
    }

    private void OnRecipeComplete()
    {
        Debug.Log($"{recipe.recipeName} complete!");
        finishCutscene.SetActive(true);
    }
}
