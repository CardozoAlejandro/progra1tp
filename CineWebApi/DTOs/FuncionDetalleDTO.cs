namespace CineWebApi.DTOs
{
    public class FuncionDetalleDTO
    {
        public int IdFuncion { get; set; }
        public int? IdPelicula { get; set; }
        public string Titulo { get; set; } = "";
        public int DuracionMinutos { get; set; }
        public string? Genero { get; set; }
        public string? Clasificacion { get; set; }
        public int? IdSala { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public decimal PrecioBase { get; set; }
        public int Capacidad { get; set; }
        public int Disponibles { get; set; }
    }
}
