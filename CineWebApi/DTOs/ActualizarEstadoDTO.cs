using System.ComponentModel.DataAnnotations;

namespace CineWebApi.DTOs
{
    public class ActualizarEstadoDto
    {
        [Required(ErrorMessage = "El nuevo estado es obligatorio.")]
        [StringLength(50, ErrorMessage = "El estado no puede superar los 50 caracteres.")]
        public string NuevoEstado { get; set; } = string.Empty;
    }
}
