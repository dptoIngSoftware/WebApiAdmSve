using WebApiVotacionElectronica.Models.SVE;

namespace WebApiVotacionElectronica.Repository.Interfaces
{
    public interface IVotante_Repository
    {
        List<Votante> GetAllByVotacionID(int ID);
        Votante GetByID(int ID);
        bool Ha_Votado(int ID_Votante, int ID_Votacion);
        bool CreateAll(List<Votante> Votantes);
        bool Update(Votante Votante);
        bool SaveAll(int Length);
        bool Save();
    }
}
