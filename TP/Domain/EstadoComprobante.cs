namespace Cine_TP.Domain
{
    public static class EstadoComprobante
    {
        public const string Confirmado = "Confirmado";
        public const string Pendiente = "Pendiente";
        public const string Cancelado = "Cancelado";

        public static readonly string[] Validos = { Confirmado, Pendiente, Cancelado };
    }
}