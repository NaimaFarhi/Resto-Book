namespace RestoBook_MiniProjet.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        // Reservation Stats
        public int TotalReservations { get; set; }
        public int PendingReservations { get; set; }
        public int ConfirmedReservations { get; set; }
        public int CancelledReservations { get; set; }
        public int TodayReservations { get; set; }

        // Menu Stats
        public int TotalMenuItems { get; set; }
        public int TotalCategories { get; set; }

        // Add these:
        public int TotalTables { get; set; }
        public int TotalCustomers { get; set; }

        // Charts Data
        public List<string> WeeklyReservationsDates { get; set; } = new();
        public List<int> WeeklyReservationsCounts { get; set; } = new();
    }
}