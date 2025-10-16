using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace WebApiVotacionElectronica.Models.SVE
{
    public class Voto
    {
        [Key]
        public int Id { get; set; }
        public Votante Votante { get; set; }
        public Candidato? Candidato { get; set; }
        public DateTime FechaVoto { get; set; }
    }
}
