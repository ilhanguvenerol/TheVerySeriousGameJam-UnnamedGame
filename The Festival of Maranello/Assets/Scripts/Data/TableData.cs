using UnityEngine;

namespace FeastGame.Data
{
    [CreateAssetMenu(fileName = "Table_", menuName = "FeastGame/Table Data")]
    public class TableData : ScriptableObject
    {
        public string tableName;

        [Tooltip("Which wheel pool this table spins from. Tables can share a pool or each have their own.")]
        public MenuWheelData wheelPool;

        [Tooltip("How many guest servings the menu can provide before it must be rerolled.")]
        public int totalUses = 6;

        [Tooltip("How many guests can sit at this table at once.")]
        public int seatCount = 6;
    }
}
