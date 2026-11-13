using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cine_TP.Domain
{
    [Table("formas_pago")]
    public class FormaPago
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_forma_pago")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        public virtual ICollection<Comprobante> Comprobantes { get; set; } = new List<Comprobante>();
    }
}