using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Cine_TP.Domain
{
    [Table("detalles_comprobantes")]
    public class ComprobanteDetalle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_detalle")]
        public int Id { get; set; }

        [Required]
        [Column("id_comprobante")]
        public int IdComprobante { get; set; }

        [Required]
        [Column("id_funcion")]
        public int IdFuncion { get; set; }

        [Required]
        [Column("id_butaca")]
        public int IdButaca { get; set; }

        [Column("id_tipo_publico")]
        public int? IdTipoPublico { get; set; }

        [Required]
        [Column("precio_base", TypeName = "decimal(10,2)")]
        public decimal PrecioBase { get; set; }

        [Required]
        [Column("descuento_unitario", TypeName = "decimal(10,2)")]
        public decimal DescuentoUnitario { get; set; }

        [Required]
        [Column("precio_unitario", TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Column("butaca_ocupada")]
        public bool ButacaOcupada { get; set; } = true;

        // Relación Detalle -> Maestro
        [JsonIgnore]
        [ForeignKey(nameof(IdComprobante))]
        public virtual Comprobante? Comprobante { get; set; }

        [ForeignKey(nameof(IdFuncion))]
        public virtual Funcion? Funcion { get; set; }

        [ForeignKey(nameof(IdButaca))]
        public virtual Butaca? Butaca { get; set; }

        [ForeignKey(nameof(IdTipoPublico))]
        public virtual TipoPublico? TipoPublico { get; set; }
    }
}