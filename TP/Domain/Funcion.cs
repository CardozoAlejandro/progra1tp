using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cine_TP.Domain
{
    [Table("funciones")]
    public class Funcion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_funcion")]
        public int Id { get; set; }

        [Column("id_pelicula")]
        public int? IdPelicula { get; set; }

        [Column("id_sala")]
        public int? IdSala { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Column("fecha_hora_inicio")]
        public DateTime FechaHoraInicio { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Column("fecha_hora_fin")]
        public DateTime FechaHoraFin { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo.")]
        [Column("precio_base", TypeName = "decimal(10,2)")]
        public decimal PrecioBase { get; set; }

        [ForeignKey(nameof(IdPelicula))]
        public virtual Pelicula? Pelicula { get; set; }

        [ForeignKey(nameof(IdSala))]
        public virtual Sala? Sala { get; set; }

        public virtual ICollection<ComprobanteDetalle> ComprobanteDetalles { get; set; } = new List<ComprobanteDetalle>();
    }
}