namespace CineWebApi.DTOs
{
    public class PeliculaDetalleDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public int DuracionMinutos { get; set; }
        public string? Sinopsis { get; set; }
        public DateTime? FechaEstreno { get; set; }
        public string? Genero { get; set; }
        public string? Clasificacion { get; set; }
    }
}
