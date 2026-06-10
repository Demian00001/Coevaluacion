namespace Coevaluacion.Models
{
    public class Integrante
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Apellido { get; set; }
        public required string Correo { get; set; }
        public required string Matricula { get; set; }

        public int EquipoId { get; set; }
        public Equipo? Equipo { get; set; }
    }
}
