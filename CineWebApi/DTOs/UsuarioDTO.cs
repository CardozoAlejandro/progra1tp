namespace CineWebApi.DTOs
{
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = "";
        public string Email { get; set; } = "";
        public string Rol { get; set; } = "";
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
    }

}
