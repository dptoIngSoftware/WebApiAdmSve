using WebApiVotacionElectronica.Models.SVE;

namespace WebApiVotacionElectronica.Repository.Interfaces
{
    public interface IEstado_Votacion_Repository
    {
        Estado_Votacion GetEstadoByDescr(string descr);
    }
}
