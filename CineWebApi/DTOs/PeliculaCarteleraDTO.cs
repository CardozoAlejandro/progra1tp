namespace CineWebApi.DTOs
{
    public class PeliculaCarteleraDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public int DuracionMinutos { get; set; }
        public DateTime? FechaEstreno { get; set; }
        public string? Genero { get; set; }
        public string? Clasificacion { get; set; }
    }
}