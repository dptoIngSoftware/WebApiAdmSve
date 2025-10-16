using WebApiVotacionElectronica.Context;
using WebApiVotacionElectronica.Models.SVE;
using WebApiVotacionElectronica.Repository.Interfaces;

namespace WebApiVotacionElectronica.Repository
{
    public class Estado_Candidato_Repository : IEstado_Candidato_Repository
    {
        private readonly DBContext context;

        public Estado_Candidato_Repository(DBContext context)
        {
            this.context = context;
        }

        public Estado_Candidato GetEstadoByDescr(string descr)
        {
           return context.SVE_Estados_Candidato.Where(e => e.Descripcion == descr).FirstOrDefault();
        }
    }
}
