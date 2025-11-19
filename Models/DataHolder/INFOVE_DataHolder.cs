using WebApiVotacionElectronica.Models.SVE;

namespace WebApiVotacionElectronica.Models.DataHolder
{
    public class INFOVE_DataHolder
    {
        public int TotalVotos { get; set; }
        public int Totalnulos { get; set; }

        public int VotantesQuevotaron { get; set; }

        public int TotalVotantes { get; set; }

        public Votacion? Votacion { get; set; }

        public List<CandidatoxVoto_DataHolder> Candidatos_Top { get; set; }

    }
}
