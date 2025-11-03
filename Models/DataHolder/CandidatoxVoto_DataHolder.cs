using WebApiVotacionElectronica.Models.SVE;

namespace WebApiVotacionElectronica.Models.DataHolder
{
    public class CandidatoxVoto_DataHolder
    {
        public Candidato? Candidato { get; set; }

        public int Total { get; set; }

        public int candidatoid { get; set; }

        public bool? Ganador { get; set; }
    }
}
