namespace CineWebApi.DTOs
{
    public class MiEntradaDTO
    {
        public int IdFuncion { get; set; }
        public string Pelicula { get; set; } = "";
        public DateTime FechaHoraInicio { get; set; }
        public int? IdSala { get; set; }
        public string Butaca { get; set; } = "";
        public decimal Precio { get; set; }
    }
}
