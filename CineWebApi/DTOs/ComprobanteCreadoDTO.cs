namespace CineWebApi.DTOs
{
    public class ComprobanteCreadoDTO
    {
        public int IdComprobante { get; set; }
        public string Estado { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal MontoTotal { get; set; }
        public DateTime? FechaLimiteConfirmacion { get; set; }
    }
}
