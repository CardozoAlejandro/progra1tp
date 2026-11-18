using System.ComponentModel.DataAnnotations;

namespace CineWebApi.DTOs
{
    public class RegistroDTO
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string Dni { get; set; } = string.Empty;
    }
}