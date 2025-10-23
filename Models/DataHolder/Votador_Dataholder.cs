using WebApiVotacionElectronica.Models.SVE;

namespace WebApiVotacionElectronica.Models.DataHolder
{
    public class Votador_Dataholder
    {
        public Votacion Votacion { get; set; }

        public int IdVotante { get; set; }

        public bool Has_Voted { get; set; }

        public List<Candidato> Candidatos { get; set; }

        public string TKN { get; set; }
    }
}
