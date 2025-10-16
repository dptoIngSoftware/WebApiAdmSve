using Microsoft.EntityFrameworkCore;
using WebApiVotacionElectronica.Context;
using WebApiVotacionElectronica.Models.SVE;
using WebApiVotacionElectronica.Repository.Interfaces;

namespace WebApiVotacionElectronica.Repository
{
    public class Voto_Repository:IVoto_Repository
    {
        private readonly DBContext context;

        public Voto_Repository(DBContext context)
        {
            this.context = context;
        }

        public bool CreateAll(List<Voto> Votos)
        {
            context.SVE_Votos.AddRange(Votos);
            return SaveAll(Votos.Count);
        }

        public List<Voto> GetByVotacion(int VotacionID)
        {
            return context.SVE_Votos.Where(v => v.Votante.Votacion_ID == VotacionID).AsNoTracking().ToList();
        }

        public bool SaveAll(int Length)
        {
            return context.SaveChanges() == Length;
        }
    }
}
