using System.ComponentModel.DataAnnotations;

namespace Coevaluacion.Models
{
    public class Integrante
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public required string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es requerido.")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres.")]
        public required string Apellido { get; set; }

        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo válido.")]
        public required string Correo { get; set; }

        [Required(ErrorMessage = "La matrícula es requerida.")]
        [StringLength(20, ErrorMessage = "La matrícula no puede exceder los 20 caracteres.")]
        public required string Matricula { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un equipo.")]
        public int EquipoId { get; set; }
        public Equipo? Equipo { get; set; }
    }
}
