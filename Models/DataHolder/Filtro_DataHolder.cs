namespace WebApiVotacionElectronica.Models.DataHolder
{
    public class Filtro_DataHolder
    {
        public string? Estado { get; set; }
        public string? Nombre { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaTermino { get; set; }
        public string Sede { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public int CurrentPage { get; set; }
        public int PerPage { get; set; }
    }
}
