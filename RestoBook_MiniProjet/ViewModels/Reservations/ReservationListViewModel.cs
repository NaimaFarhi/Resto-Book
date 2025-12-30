namespace RestoBook_MiniProjet.ViewModels.Reservations
{
    public class ReservationListViewModel
    {
        public int Id { get; set; }
        public DateTime ReservationDate { get; set; }
        public int NumberOfGuests { get; set; }
        public string Status { get; set; }

        public int TableNumber { get; set; }
        public string CustomerName { get; set; }
        public string? SpecialRequest { get; set; }
    }
}
