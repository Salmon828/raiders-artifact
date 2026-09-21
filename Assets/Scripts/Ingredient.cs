using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public IngredientType type;
    public Color color;
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

