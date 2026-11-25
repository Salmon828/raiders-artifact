using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "Cooking/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public IngredientType[] steps;
}
