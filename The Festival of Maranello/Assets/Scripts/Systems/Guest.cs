using FeastGame.Data;
using UnityEngine;

namespace FeastGame.Systems
{

    // Runtime representation of a single guest, need to add sprites later
    [System.Serializable]
    public class Guest
    {
        public string guestId;
        public RaceData race;

        [Tooltip("0-100. Starts at a neutral baseline and shifts based on meals served.")]
        public float comfort = 100f;

        [Tooltip("Decided at spawn time. The hour (or tick) at which this guest will automatically leave.")]
        public float departureTime;

        public Table seatedAt;

        public Guest(RaceData race, float departureTime)
        {
            this.guestId = System.Guid.NewGuid().ToString("N").Substring(0, 8);
            this.race = race;
            this.departureTime = departureTime;
            this.comfort = 100f;
        }

        // Apply comfort effect of served dish.
        public void ApplyDishEffect(DishData dish, CourseLayer course)
        {
            if (dish == null || race == null) return;

            float severityMultiplier = course == CourseLayer.Main ? 1.5f : 1f; //main meal effects harder

            foreach (var ingredient in dish.ingredients)
            {
                var reaction = race.GetReaction(ingredient);
                if (reaction.HasValue)
                {
                    comfort += reaction.Value.comfortDelta * severityMultiplier;
                }
            }

            comfort = Mathf.Clamp(comfort, 0f, 100f);
        }
    }
}
