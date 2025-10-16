using WebApiVotacionElectronica.Models.SVE;

namespace WebApiVotacionElectronica.Repository.Interfaces
{
    public interface IVoto_Repository
    {
        List<Voto> GetByVotacion(int VotacionID);
        bool CreateAll(List<Voto> Votos);
        bool SaveAll(int Length);
    }
}
