using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cine_TP.Domain
{
    [Table("clientes")]
    public class Cliente
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_cliente")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [StringLength(15)]
        [Column("dni")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50)]
        [Column("apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "El email no es válido.")]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        [Column("telefono")]
        public string? Telefono { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Column("fecha_nacimiento", TypeName = "date")]
        public DateTime FechaNacimiento { get; set; }

        [Column("id_tipo_publico")]
        public int? IdTipoPublico { get; set; }

        [Column("id_usuario")]
        public int? IdUsuario { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public virtual Usuario? Usuario { get; set; }

        [ForeignKey(nameof(IdTipoPublico))]
        public virtual TipoPublico? TipoPublico { get; set; }

        // Relación Cliente -> Comprobantes
        public virtual ICollection<Comprobante> Comprobantes { get; set; } = new List<Comprobante>();
    }
}