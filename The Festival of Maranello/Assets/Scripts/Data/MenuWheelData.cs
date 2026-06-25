using System.Collections.Generic;
using UnityEngine;

namespace FeastGame.Data
{
    [CreateAssetMenu(fileName = "MenuWheelPool", menuName = "FeastGame/Menu Wheel Pool")]
    public class MenuWheelData : ScriptableObject
    {
        [Tooltip("All soups eligible to be spun.")]
        public List<DishData> soups = new List<DishData>();

        [Tooltip("All main meals eligible to be spun.")]
        public List<DishData> mains = new List<DishData>();

        [Tooltip("All desserts eligible to be spun.")]
        public List<DishData> desserts = new List<DishData>();

        // random pick from a list of dishes
        public DishData SpinLayer(List<DishData> pool)
        {
            if (pool == null || pool.Count == 0)
                return null;

            float total = 0f;
            foreach (var d in pool)
                total += Mathf.Max(0f, d.wheelWeight);

            if (total <= 0f)
                return pool[Random.Range(0, pool.Count)];

            float roll = Random.Range(0f, total);
            float cumulative = 0f;
            foreach (var d in pool)
            {
                cumulative += Mathf.Max(0f, d.wheelWeight);
                if (roll <= cumulative)
                    return d;
            }
            return pool[pool.Count - 1];
        }

        public (DishData soup, DishData main, DishData dessert) SpinFullMenu()
        {
            return (SpinLayer(soups), SpinLayer(mains), SpinLayer(desserts));
        }
    }
}
