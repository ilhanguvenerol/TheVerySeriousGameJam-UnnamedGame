using System;
using System.Collections.Generic;
using FeastGame.Data;
using UnityEngine;

namespace FeastGame.Systems
{
    [System.Serializable]
    public class Table
    {
        public TableData config;

        public DishData currentSoup;
        public DishData currentMain;
        public DishData currentDessert;
        public int usesRemaining;

        public List<Guest> seatedGuests = new List<Guest>();

        public event Action OnSeatVacated;

        public Table(TableData config)
        {
            this.config = config;
            usesRemaining = 0; // no menu rolled yet
        }

        public bool HasMenu => usesRemaining > 0;

        public bool IsEmpty => seatedGuests.Count == 0;

        public bool HasOpenSeat => seatedGuests.Count < config.seatCount;

        public bool CanReroll => IsEmpty && usesRemaining <= 0;

        // Spins a fresh menu for this table
        public void RollMenu()
        {
            if (config.wheelPool == null)
            {
                Debug.LogWarning($"Table '{config.tableName}' has no wheel pool assigned.");
                return;
            }

            var (soup, main, dessert) = config.wheelPool.SpinFullMenu();
            currentSoup = soup;
            currentMain = main;
            currentDessert = dessert;
            usesRemaining = config.totalUses;
        }

        /// <summary>
        /// Seats a guest at this table and immediately serves them, consuming one use.
        /// Returns false if there's no room or no menu available.
        /// </summary>
        public bool TrySeatAndServe(Guest guest)
        {
            if (!HasOpenSeat || usesRemaining <= 0)
                return false;

            seatedGuests.Add(guest);
            guest.seatedAt = this;

            guest.ApplyDishEffect(currentSoup, CourseLayer.Soup);
            guest.ApplyDishEffect(currentMain, CourseLayer.Main);
            guest.ApplyDishEffect(currentDessert, CourseLayer.Dessert);

            usesRemaining--;
            return true;
        }

        /// <summary>
        /// Removes a guest from this table (automatic departure, decided at spawn time).
        /// </summary>
        public void VacateSeat(Guest guest)
        {
            if (seatedGuests.Remove(guest))
            {
                guest.seatedAt = null;
                OnSeatVacated?.Invoke();
            }
        }
    }
}
