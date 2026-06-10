namespace Coevaluacion.Models
{
    public class Criterio
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal Peso { get; set; }
        public bool Activo { get; set; }
    }
}
