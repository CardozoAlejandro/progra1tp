namespace Cine_TP.Domain
{
    public static class EstadoPelicula
    {
        public const string EnCartelera = "En cartelera";
        public const string FueraDeCartelera = "Fuera de cartelera";

        public static readonly string[] Validos = { EnCartelera, FueraDeCartelera };
    }
}