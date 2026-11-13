using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cine_TP.Domain
{
    [Table("butacas")]
    public class Butaca
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_butaca")]
        public int Id { get; set; }

        [Column("id_sala")]
        public int? IdSala { get; set; }

        [Required(ErrorMessage = "La fila es obligatoria.")]
        [StringLength(5)]
        [Column("fila")]
        public string Fila { get; set; } = string.Empty;

        [Required]
        [Column("numero")]
        public int Numero { get; set; }

        [StringLength(20)]
        [Column("estado")]
        public string? Estado { get; set; } // Ej: "Habilitada"

        [ForeignKey(nameof(IdSala))]
        public virtual Sala? Sala { get; set; }

        public virtual ICollection<ComprobanteDetalle> ComprobanteDetalles { get; set; } = new List<ComprobanteDetalle>();
    }
}