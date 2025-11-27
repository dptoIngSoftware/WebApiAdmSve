namespace WebApiVotacionElectronica.Models.Tools
{
    public class EmailWorkItem
    {
        public string Destinatario { get; set; }
        public string Nombre { get; set; }
        public string Asunto { get; set; }
        public string Link { get; set; }
        public int VotanteId { get; set; }
        public string DESC { get; set; }
        public string FI { get; set; }
        public string FT { get; set; }
    }
}
