using System;
using FeastGame.Systems;
using UnityEngine;

namespace FeastGame.Core
{
    public enum GamePhase
    {
        WelcomingVisitors,
        PreparingForNext
    }

    public class GameManager : MonoBehaviour
    {
        [Header("System references")]
        [SerializeField] private GuestSystem guestSystem;
        [SerializeField] private TableSystem tableSystem;
        [SerializeField] private ComfortManager comfortManager;
        [SerializeField] private KingEndgameResolver kingResolver;

        [Header("Config")]
        [Tooltip("How many guests arrive per batch during Welcoming Visitors.")] //add randomization later
        [SerializeField] private int guestsPerBatch = 3;

        public GamePhase CurrentPhase { get; private set; } = GamePhase.WelcomingVisitors;

        public event Action<GamePhase> OnPhaseChanged;
        public event Action OnGameLost;
        public event Action<bool> OnGameWon; // true = king satisfied

        private float simulatedTime = 19f; // 7pm start, cosmetic only - TimeManager is non-essential

        private void Start()
        {
            // "Start by randomizing two tables" per design doc.
            tableSystem.RollInitialTables(2);
            EnterPhase(GamePhase.WelcomingVisitors);
        }

        private void OnEnable()
        {
            if (comfortManager != null)
            {
                comfortManager.OnInstantLose_LowAverageComfort += HandleLoss;
                comfortManager.OnInstantLose_SingleGuestZero += (_) => HandleLoss();
                comfortManager.OnInstantLose_NoSeatAvailable += HandleLoss;
            }
        }

        private void OnDisable()
        {
            if (comfortManager != null)
            {
                comfortManager.OnInstantLose_LowAverageComfort -= HandleLoss;
                comfortManager.OnInstantLose_NoSeatAvailable -= HandleLoss;
            }
        }

        /// <summary>
        /// Player-triggered switch between the two phases. Hook this up to a UI button.
        /// Switching into Preparing is blocked while any guest is still queued - per
        /// design, the queue must be fully emptied first. If the player is stuck
        /// unable to empty it, some other instant-lose condition (no seat available,
        /// comfort threshold) will already have fired before this matters.
        /// </summary>
        public bool TogglePhase()
        {
            if (CurrentPhase == GamePhase.WelcomingVisitors && guestSystem.QueueLength > 0)
                return false;

            var next = CurrentPhase == GamePhase.WelcomingVisitors
                ? GamePhase.PreparingForNext
                : GamePhase.WelcomingVisitors;
            EnterPhase(next);
            return true;
        }

        private void EnterPhase(GamePhase phase)
        {
            CurrentPhase = phase;

            if (phase == GamePhase.WelcomingVisitors)
            {
                EnterWelcomingPhase();
            }
            else
            {
                EnterPreparingPhase();
            }

            OnPhaseChanged?.Invoke(CurrentPhase);
        }

        private void EnterWelcomingPhase()
        {
            // Spawn a new batch - no rejection mechanism, every guest is accepted.
            // They join GuestSystem's FIFO queue; the player resolves them strictly
            // in order via RequestSeatCurrentGuest below (e.g. clicking a table after
            // checking the current guest's traits against menus/encyclopedia/cookbook).
            var batch = guestSystem.SpawnBatch(guestsPerBatch, simulatedTime);
            foreach (var guest in batch)
            {
                comfortManager.RegisterArrival(guest);
            }
        }

        private void EnterPreparingPhase()
        {
            // TogglePhase already guarantees the queue was empty before we got here,
            // so this should be a no-op. Kept as a defensive assertion in case some
            // other code path reaches EnterPhase(PreparingForNext) directly.
            guestSystem.CheckForUnplacedGuests();

            // Automatic departures and menu rerolls happen here only.
            guestSystem.ProcessDepartures(simulatedTime);
            simulatedTime += 1f; // cosmetic hour tick, optional
        }

        /// <summary>
        /// UI-facing entry point for the player choosing a table for the current guest.
        /// Call this from Hall view when the player clicks a table to seat whoever is
        /// currently at the front of the queue. There is no overload taking a specific
        /// guest - per design, guests must be handled in strict arrival order, so the
        /// only guest that can ever be seated is GuestSystem.CurrentGuest.
        /// </summary>
        public bool RequestSeatCurrentGuest(Table table)
        {
            return guestSystem.SeatCurrentGuest(table);
        }

        /// <summary>
        /// Read-only check for whether a table is currently a valid seat for the
        /// current guest - use this from Hall view to highlight valid tables before
        /// the player clicks, without actually committing the seating.
        /// </summary>
        public bool CanSeatCurrentGuestAt(Table table)
        {
            return guestSystem.CanSeatCurrentGuestAt(table);
        }

        /// <summary>
        /// UI-facing entry point for rerolling a table. Gates TableSystem.TryRerollTable
        /// behind the current phase - only valid during Preparing for Next Visitors.
        /// UI code should always call this, never TableSystem.TryRerollTable directly,
        /// otherwise the phase restriction can be bypassed.
        /// </summary>
        public bool RequestTableReroll(Table table)
        {
            if (CurrentPhase != GamePhase.PreparingForNext)
                return false;

            return tableSystem.TryRerollTable(table);
        }

        /// <summary>
        /// Call when it's time for the king to arrive (e.g. a "Call the King" UI button
        /// during Preparing phase, since TimeManager / the 12am clock is non-essential for now).
        /// </summary>
        public void TriggerKingsArrival()
        {
            bool success = kingResolver.ResolveKingsRequest();
            OnGameWon?.Invoke(success);
        }

        private void HandleLoss()
        {
            OnGameLost?.Invoke();
        }
    }
}
