using WebApiVotacionElectronica.Context;
using WebApiVotacionElectronica.Models.SVE;
using WebApiVotacionElectronica.Repository.Interfaces;

namespace WebApiVotacionElectronica.Repository
{
    public class Estado_Votacion_Repository:IEstado_Votacion_Repository
    {
        private readonly DBContext context;

        public Estado_Votacion_Repository(DBContext context)
        {
            this.context = context;
        }

        public Estado_Votacion GetEstadoByDescr(string descr)
        {
            return context.SVE_Estados_Votacion.Where(e => e.Descripcion == descr).FirstOrDefault();
        }
    }
}
