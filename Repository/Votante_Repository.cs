using WebApiVotacionElectronica.Context;
using WebApiVotacionElectronica.Models.SVE;
using WebApiVotacionElectronica.Repository.Interfaces;

namespace WebApiVotacionElectronica.Repository
{
    public class Votante_Repository:IVotante_Repository
    {
        private readonly DBContext context;

        public Votante_Repository(DBContext context)
        {
            this.context = context;
        }

        public bool CreateAll(List<Votante> Votantes)
        {
            context.SVE_Votantes.AddRange(Votantes);
            return SaveAll(Votantes.Count);
        }

        public List<Votante> GetAllByVotacionID(int ID)
        {
            return context.SVE_Votantes.Where(v => v.Votacion_ID == ID).ToList();
        }

        public Votante GetByID(int ID)
        {
            return context.SVE_Votantes.Where(v => v.Id == ID).FirstOrDefault();
        }

        public bool Ha_Votado(int ID_Votante, int ID_Votacion)
        {
            return context.SVE_Votantes.Any(v => v.Id == ID_Votante && v.Votacion_ID == ID_Votacion && v.Ha_Votado);
        }

        public bool Save()
        {
            return context.SaveChanges() > 0;
        }

        public bool SaveAll(int Length)
        {
            return context.SaveChanges() == Length;
        }

        public bool Update(Votante Votante)
        {
            context.SVE_Votantes.Update(Votante);
            return Save();
        }
    }
}
