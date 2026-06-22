using UnityEngine;

namespace FeastGame.Data
{
    [CreateAssetMenu(fileName = "Ingredient_", menuName = "FeastGame/Ingredient Data")]
    public class IngredientData : ScriptableObject
    {
        public string ingredientName;
        [TextArea] public string description;
        public Sprite icon;
    }
}
