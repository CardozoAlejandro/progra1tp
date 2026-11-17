using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Cine_TP.Domain
{
    [Table("clasificaciones")]
    public class Clasificacion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_clasificacion")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(10)]
        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [StringLength(200)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [JsonIgnore]
        public virtual ICollection<Pelicula> Peliculas { get; set; } = new List<Pelicula>();
    }
}