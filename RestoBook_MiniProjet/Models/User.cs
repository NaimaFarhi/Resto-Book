using System.ComponentModel.DataAnnotations;

namespace RestoBook_MiniProjet.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Customer";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Reservation> Reservations { get; set; }

    }
}
