using System;

namespace RestoBook_MiniProjet.ViewModels.Availability
{
    public class AvailabilityViewModel
    {
        public DateTime SelectedDate { get; set; }

        public int TableId { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }

        public bool IsAvailable { get; set; }
        public string? ReservationTime { get; set; }
        public string? ReservationPeriod { get; set; }
    }
}
