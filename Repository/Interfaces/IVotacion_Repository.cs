using WebApiVotacionElectronica.Models.DataHolder;
using WebApiVotacionElectronica.Models.SVE;

namespace WebApiVotacionElectronica.Repository.Interfaces
{
    public interface IVotacion_Repository
    {
        List<Votacion> GetAll(Filtro_DataHolder Filtro);
        Votacion GetById(int id);
        Votacion GetByIdNoTrack(int id);
        bool Create(Votacion votacion);
        bool Update(Votacion votacion);
        bool Save();
    }
}
