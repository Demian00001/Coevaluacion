namespace Coevaluacion.Models
{
    public class Periodo
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activo { get; set; }
    }
}
