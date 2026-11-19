using System.ComponentModel.DataAnnotations;

namespace CineWebApi.DTOs
{
    public class LoginDTO
    {
        [Required] public string NombreUsuario { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }
}
