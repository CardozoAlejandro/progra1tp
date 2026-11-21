namespace CineWebApi.DTOs
{
    public class MiComprobanteDTO
    {
        public int IdComprobante { get; set; }
        public string Estado { get; set; } = "";
        public DateTime FechaHoraCreacion { get; set; }
        public DateTime? FechaHoraEmision { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoFinal { get; set; }
        public string? FormaPago { get; set; }
        public string? Promocion { get; set; }
        public List<MiEntradaDTO> Entradas { get; set; } = new();
    }
}
