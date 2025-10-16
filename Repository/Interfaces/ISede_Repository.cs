using WebApiVotacionElectronica.Models.SVE;

namespace WebApiVotacionElectronica.Repository.Interfaces
{
    public interface ISede_Repository
    {
        Sede GetByCod(string Cod);
        List<Sede> GetAll();
    }
}
