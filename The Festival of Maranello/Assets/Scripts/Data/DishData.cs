using System.Collections.Generic;
using UnityEngine;

namespace FeastGame.Data
{
    public enum CourseLayer
    {
        Soup,
        Main,
        Dessert
    }

    [CreateAssetMenu(fileName = "Dish_", menuName = "FeastGame/Dish Data")]
    public class DishData : ScriptableObject
    {
        public string dishName;
        public CourseLayer course;
        [TextArea] public string description;
        public Sprite icon;

        [Tooltip("Ingredients contained in this dish. Used to look up race reactions.")]
        public List<IngredientData> ingredients = new List<IngredientData>();

        [Tooltip("Relative weight for this dish's wheel slice within its course layer. Higher = more likely.")]
        public float wheelWeight = 1f;
    }
}
