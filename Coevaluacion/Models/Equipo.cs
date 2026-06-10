namespace Coevaluacion.Models
{
    public class Equipo
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public string? Descripcion { get; set; }

        public ICollection<Integrante> Integrantes { get; set; } = new List<Integrante>();
    }
}
