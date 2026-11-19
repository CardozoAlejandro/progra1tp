using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Cine_TP.Domain
{
    [Table("generos")]
    public class Genero
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_genero")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del género es obligatorio.")]
        [StringLength(50)]
        [Column("nombre_genero")]
        public string NombreGenero { get; set; } = string.Empty;

        [JsonIgnore]
        public virtual ICollection<Pelicula> Peliculas { get; set; } = new List<Pelicula>();
    }
}