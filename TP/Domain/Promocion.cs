using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cine_TP.Domain
{
    [Table("promociones")]
    public class Promocion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_promocion")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        [Column("nombre_promo")]
        public string NombrePromo { get; set; } = string.Empty;

        [Required]
        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100.")]
        [Column("porcentaje_descuento", TypeName = "decimal(5,2)")]
        public decimal PorcentajeDescuento { get; set; }

        [StringLength(15)]
        [Column("dia_semana")]
        public string? DiaSemana { get; set; }

        [DataType(DataType.Time)]
        [Column("hora_inicio", TypeName = "time")]
        public TimeSpan? HoraInicio { get; set; }

        [DataType(DataType.Time)]
        [Column("hora_fin", TypeName = "time")]
        public TimeSpan? HoraFin { get; set; }

        [StringLength(50)]
        [Column("temporada")]
        public string? Temporada { get; set; }

        public virtual ICollection<Comprobante> Comprobantes { get; set; } = new List<Comprobante>();
    }
}