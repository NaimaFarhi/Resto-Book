using System.ComponentModel.DataAnnotations;

namespace RestoBook_MiniProjet.ViewModels.Reservations
{
    public class ReservationStatusViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Status { get; set; }
    }
}
