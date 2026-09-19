using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public IngredientType type;
}

public enum IngredientType
{
    RedPot,
    GreenPot,
    BluePot,
    Skull,
    Branch,
    Jar
}

