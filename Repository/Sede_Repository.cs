using WebApiVotacionElectronica.Context;
using WebApiVotacionElectronica.Models.SVE;
using WebApiVotacionElectronica.Repository.Interfaces;

namespace WebApiVotacionElectronica.Repository
{
    public class Sede_Repository:ISede_Repository
    {
        private readonly DBContext context;

        public Sede_Repository(DBContext context)
        {
            this.context = context;
        }

        public List<Sede> GetAll()
        {
            return context.SVE_Sedes.ToList();
        }

        public Sede GetByCod(string Cod)
        {
           return context.SVE_Sedes.Where(x => x.Codigo == Cod).FirstOrDefault();
        }
    }
}
