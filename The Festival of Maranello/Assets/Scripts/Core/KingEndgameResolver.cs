using System;
using FeastGame.Core;
using UnityEngine;

namespace FeastGame.Systems
{
    public class KingEndgameResolver : MonoBehaviour
    {
        [SerializeField] private ComfortManager comfortManager;

        public event Action<bool, float> OnKingResultResolved;

        //Comfort% = Win%
        public bool ResolveKingsRequest()
        {
            float winChance = comfortManager != null ? comfortManager.CurrentAverageComfort : 0f;
            float roll = UnityEngine.Random.Range(0f, 100f);

            bool success = roll <= winChance;
            OnKingResultResolved?.Invoke(success, winChance);
            return success;
        }
    }
}
