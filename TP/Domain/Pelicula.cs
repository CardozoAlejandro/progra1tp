using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cine_TP.Domain
{
    [Table("peliculas")]
    public class Pelicula
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_pelicula")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(150)]
        [Column("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [Range(1, 600, ErrorMessage = "La duración debe estar entre 1 y 600 minutos.")]
        [Column("duracion_minutos")]
        public int DuracionMinutos { get; set; }

        [Column("sinopsis", TypeName = "varchar(max)")]
        public string? Sinopsis { get; set; }

        [DataType(DataType.Date)]
        [Column("fecha_estreno", TypeName = "date")]
        public DateTime? FechaEstreno { get; set; }

        [Column("id_genero")]
        public int? IdGenero { get; set; }

        [Required]
        [StringLength(20)]
        [Column("estado")]
        public string Estado { get; set; } = EstadoPelicula.EnCartelera;

        [Column("id_clasificacion")]
        public int? IdClasificacion { get; set; }

        [ForeignKey(nameof(IdGenero))]
        public virtual Genero? Genero { get; set; }

        [ForeignKey(nameof(IdClasificacion))]
        public virtual Clasificacion? Clasificacion { get; set; }

        public virtual ICollection<Funcion> Funciones { get; set; } = new List<Funcion>();
    }
}