namespace Cine_TP.Domain
{
    public static class EstadoComprobante
    {
        public const string Confirmada = "Confirmada";
        public const string Pendiente = "Pendiente";
        public const string Cancelado = "Cancelado";

        public static readonly string[] Validos = { Confirmada, Pendiente, Cancelado };
    }
}