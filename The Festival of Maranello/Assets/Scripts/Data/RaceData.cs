using System;
using System.Collections.Generic;
using UnityEngine;

namespace FeastGame.Data
{
    public enum ReactionType
    {
        None,
        Allergic,      // severe comfort hit
        Intolerant,    // moderate comfort hit
        Intoxicated,   // unique effect, possibly leads to instant game over
        Loved           // bonus comfort
    }

    [Serializable]
    public struct IngredientReaction
    {
        public IngredientData ingredient;
        public ReactionType reaction;
        [Tooltip("Negative for bad reactions, positive for Loved.")]
        public float comfortDelta;
    }

    [CreateAssetMenu(fileName = "Race_", menuName = "FeastGame/Race Data")]
    public class RaceData : ScriptableObject
    {
        public string raceName;
        [TextArea] public string description;
        public Sprite portrait;

        [Tooltip("How this race reacts to specific ingredients. Anything not listed is treated as neutral.")]
        public List<IngredientReaction> ingredientReactions = new List<IngredientReaction>();

        public IngredientReaction? GetReaction(IngredientData ingredient)
        {
            foreach (var r in ingredientReactions)
            {
                if (r.ingredient == ingredient)
                    return r;
            }
            return null;
        }
    }
}
