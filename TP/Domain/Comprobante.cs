using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cine_TP.Domain
{
    [Table("comprobantes")]
    public class Comprobante
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_comprobante")]
        public int Id { get; set; }

        [Column("id_cliente")]
        public int? IdCliente { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Column("fecha_hora_creacion")]
        public DateTime FechaHoraCreacion { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [Column("fecha_limite_confirmacion")]
        public DateTime? FechaLimiteConfirmacion { get; set; }

        [DataType(DataType.DateTime)]
        [Column("fecha_hora_emision")]
        public DateTime? FechaHoraEmision { get; set; }

        [Required]
        [StringLength(30)]
        [Column("estado")]
        public string Estado { get; set; } = EstadoComprobante.Pendiente; // Ej: "Pendiente", "Confirmada", "Cancelada", "Vencida"

        [Required]
        [Column("monto_total", TypeName = "decimal(10,2)")]
        public decimal MontoTotal { get; set; }

        [Required]
        [Column("monto_final", TypeName = "decimal(10,2)")]
        public decimal MontoFinal { get; set; }

        [Column("id_promocion")]
        public int? IdPromocion { get; set; }

        [Column("id_forma_pago")]
        public int? IdFormaPago { get; set; }

        // Relación con Cliente
        [ForeignKey(nameof(IdCliente))]
        public virtual Cliente? Cliente { get; set; }

        [ForeignKey(nameof(IdPromocion))]
        public virtual Promocion? Promocion { get; set; }

        [ForeignKey(nameof(IdFormaPago))]
        public virtual FormaPago? FormaPago { get; set; }

        // Relación Maestro -> Detalle
        public virtual ICollection<ComprobanteDetalle> ComprobanteDetalles { get; set; } = new List<ComprobanteDetalle>();
    }
}