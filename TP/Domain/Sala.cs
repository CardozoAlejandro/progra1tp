using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Cine_TP.Domain
{
    [Table("salas")]
    public class Sala
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_sala")]
        public int Id { get; set; }

        [Required]
        [Column("numero_sala")]
        public int NumeroSala { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000, ErrorMessage = "La capacidad debe ser mayor a 0.")]
        [Column("capacidad_total")]
        public int CapacidadTotal { get; set; }

        [Column("id_tipo_sala")]
        public int? IdTipoSala { get; set; }

        [ForeignKey(nameof(IdTipoSala))]
        public virtual TipoSala? TipoSala { get; set; }

        public virtual ICollection<Butaca> Butacas { get; set; } = new List<Butaca>();
        public virtual ICollection<Funcion> Funciones { get; set; } = new List<Funcion>();
    }
}