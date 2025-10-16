using WebApiVotacionElectronica.Models.SVE;

namespace WebApiVotacionElectronica.Repository.Interfaces
{
    public interface IEstado_Candidato_Repository
    {
        Estado_Candidato GetEstadoByDescr(string descr);
    }
}
