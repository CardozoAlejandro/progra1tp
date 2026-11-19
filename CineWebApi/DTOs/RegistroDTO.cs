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

        [Required, StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [StringLength(15, MinimumLength = 7)]
        [RegularExpression(@"^\d+$", ErrorMessage = "El DNI solo puede contener números.")]
        public string Dni { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }
    }
}