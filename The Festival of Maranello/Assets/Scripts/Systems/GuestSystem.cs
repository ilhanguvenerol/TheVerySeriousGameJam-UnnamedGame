using System;
using System.Collections.Generic;
using FeastGame.Data;
using UnityEngine;

namespace FeastGame.Systems
{
    public class GuestSystem : MonoBehaviour
    {
        [SerializeField] private List<RaceData> availableRaces = new List<RaceData>();
        [SerializeField] private Vector2 departureDelayRange = new Vector2(2f, 5f);

        [SerializeField] private TableSystem tableSystem;

        public List<Guest> ActiveGuests { get; private set; } = new List<Guest>();


        public event Action<Guest> OnGuestHasNoSeat;//instant lose condition

        public event Action<Guest> OnGuestSeated;
        public event Action<Guest> OnGuestDeparted;


        // Spawns a single new guest with a randomly assigned race and a departure time
        public Guest SpawnGuest(float currentTime)
        {
            if (availableRaces.Count == 0)
            {
                Debug.LogWarning("GuestSystem has no races assigned.");
                return null;
            }

            var race = availableRaces[UnityEngine.Random.Range(0, availableRaces.Count)];
            float delay = UnityEngine.Random.Range(departureDelayRange.x, departureDelayRange.y);
            var guest = new Guest(race, currentTime + delay);

            ActiveGuests.Add(guest);
            return guest;
        }

        // Spawns a bulk of guests.
        public List<Guest> SpawnBatch(int count, float currentTime)
        {
            var batch = new List<Guest>();
            for (int i = 0; i < count; i++)
            {
                var guest = SpawnGuest(currentTime);
                if (guest != null) batch.Add(guest);
            }
            return batch;
        }

        public bool TrySeatGuest(Guest guest)
        {
            var table = tableSystem.FindAvailableTableFor(guest);
            if (table == null)
            {
                OnGuestHasNoSeat?.Invoke(guest);
                return false;
            }

            bool seated = tableSystem.SeatGuest(guest, table);
            if (seated)
                OnGuestSeated?.Invoke(guest);
            else
                OnGuestHasNoSeat?.Invoke(guest);

            return seated;
        }

        //handle guest departures during in-between time
        public void ProcessDepartures(float currentTime)
        {
            for (int i = ActiveGuests.Count - 1; i >= 0; i--)
            {
                var guest = ActiveGuests[i];
                if (currentTime >= guest.departureTime)
                {
                    tableSystem.VacateGuest(guest);
                    ActiveGuests.RemoveAt(i);
                    OnGuestDeparted?.Invoke(guest);
                }
            }
        }
    }
}
