namespace CineWebApi.DTOs
{
    public class ClienteDTO
    {
        public int Id { get; set; }
        public string Dni { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public int? IdTipoPublico { get; set; }
    }
}