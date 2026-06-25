using System;
using System.Collections.Generic;
using FeastGame.Data;
using UnityEngine;

namespace FeastGame.Systems
{
    public class TableSystem : MonoBehaviour
    {
        [Tooltip("Shall be 5 distinct table objects")]
        [SerializeField] private List<TableData> tableConfigs = new List<TableData>();

        public List<Table> Tables { get; private set; } = new List<Table>();

        // Fired whenever any table's seat is vacated. listens if it needs to react to freed seats.
        public event Action<Table> OnAnySeatVacated;

        private void Awake()
        {
            Tables.Clear();
            foreach (var config in tableConfigs)
            {
                var table = new Table(config);
                table.OnSeatVacated += () => OnAnySeatVacated?.Invoke(table);
                Tables.Add(table);
            }
        }

        //random initial tables for start of the game
        public void RollInitialTables(int count = 2)
        {
            for (int i = 0; i < count && i < Tables.Count; i++)
            {
                Tables[i].RollMenu();
            }
        }


        //reroll a table's menu. Only if table and seats are empty
        internal bool TryRerollTable(Table table)
        {
            if (table == null || !Tables.Contains(table))
                return false;

            if (!table.CanReroll)
                return false;

            table.RollMenu();
            return true;
        }

        internal bool CanSeatGuestAt(Guest guest, Table table)
        {
            return table != null && Tables.Contains(table) && table.HasOpenSeat && table.usesRemaining > 0;
        }

        // Seats a guest at the given table. Returns false if seating failed
        internal bool SeatGuest(Guest guest, Table table)
        {
            return table != null && table.TrySeatAndServe(guest);
        }

        public void VacateGuest(Guest guest)
        {
            guest.seatedAt?.VacateSeat(guest);
        }
    }
}
