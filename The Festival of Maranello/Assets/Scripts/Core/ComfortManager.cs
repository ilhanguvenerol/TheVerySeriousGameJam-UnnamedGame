using System;
using System.Collections.Generic;
using FeastGame.Systems;
using UnityEngine;

namespace FeastGame.Core
{
    public class ComfortManager : MonoBehaviour
    {
        [SerializeField] private GuestSystem guestSystem;

        [Tooltip("instant loss condition: singular comfort")]
        [SerializeField] private float minAverageComfort = 20f;

        [Tooltip("instant loss condition: average comfort")]
        [SerializeField] private float minSingleGuestComfort = 0f;

        /// <summary>
        /// All guests that have ever arrived, used for the running average.
        /// Per design doc: "Comfort is the average comfort of every guest that arrived so far" -
        /// this persists even after a guest departs, unlike GuestSystem.ActiveGuests.
        /// </summary>
        private readonly List<Guest> allArrivedGuests = new List<Guest>();

        public float CurrentAverageComfort { get; private set; } = 100f;

        public event Action OnInstantLose_LowAverageComfort;
        public event Action<Guest> OnInstantLose_SingleGuestZero;
        public event Action OnInstantLose_NoSeatAvailable;

        private void OnEnable()
        {
            if (guestSystem != null)
            {
                guestSystem.OnGuestSeated += HandleGuestSeated;
                guestSystem.OnGuestHasNoSeat += HandleGuestHasNoSeat;
            }
        }

        private void OnDisable()
        {
            if (guestSystem != null)
            {
                guestSystem.OnGuestSeated -= HandleGuestSeated;
                guestSystem.OnGuestHasNoSeat -= HandleGuestHasNoSeat;
            }
        }

        /// <summary>
        /// Registers a newly arrived guest into the running average pool.
        /// Call this from GuestSystem.SpawnGuest (or hook it via an OnGuestSpawned event)
        /// so comfort tracking starts as soon as they exist, per design doc.
        /// </summary>
        public void RegisterArrival(Guest guest)
        {
            allArrivedGuests.Add(guest);
            RecalculateAverage();
        }

        private void HandleGuestSeated(Guest guest)
        {
            // Comfort changed instantly on sitting (per design doc) - recalc now.
            RecalculateAverage();
            CheckSingleGuestLoss(guest);
        }

        private void HandleGuestHasNoSeat(Guest guest)
        {
            OnInstantLose_NoSeatAvailable?.Invoke();
        }

        private void RecalculateAverage()
        {
            if (allArrivedGuests.Count == 0)
            {
                CurrentAverageComfort = 100f;
                return;
            }

            float sum = 0f;
            foreach (var g in allArrivedGuests)
                sum += g.comfort;

            CurrentAverageComfort = sum / allArrivedGuests.Count;

            if (CurrentAverageComfort <= minAverageComfort)
                OnInstantLose_LowAverageComfort?.Invoke();
        }

        private void CheckSingleGuestLoss(Guest guest)
        {
            if (guest.comfort <= minSingleGuestComfort)
                OnInstantLose_SingleGuestZero?.Invoke(guest);
        }
    }
}
