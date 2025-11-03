using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace WebApiVotacionElectronica.Models.SVE
{
    public class Voto
    {
        [Key]
        public int Id { get; set; }
        public int Votante { get; set; }
        public int? Candidato { get; set; }
        public int Votacion_ID { get; set; }
        public DateTime FechaVoto { get; set; }
    }
}
