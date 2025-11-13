using Microsoft.EntityFrameworkCore;
using WebApiVotacionElectronica.Context;
using WebApiVotacionElectronica.Models.SVE;
using WebApiVotacionElectronica.Repository.Interfaces;

namespace WebApiVotacionElectronica.Repository
{
    public class Candidato_Repository:ICandidato_Repository
    {
        private readonly DBContext context;

        public Candidato_Repository(DBContext context)
        {
            this.context = context;
        }

        public bool CreateAll(List<Candidato> Candidatos)
        {
            context.SVE_Candidatos.AddRange(Candidatos);
            return SaveAll(Candidatos.Count);
        }

        public List<Candidato> GetAllByVotacionID(int ID)
        {
           return context.SVE_Candidatos.Where(c => c.Votacion_ID == ID).ToList();
        }

        public bool Update(Candidato Candidato)
        {
            context.SVE_Candidatos.Update(Candidato);
            return Save();
        }

        public bool Save()
        {
            return context.SaveChanges() > 0;   
        }

        public bool SaveAll(int Length)
        {
            return context.SaveChanges() == Length;
        }

        public Candidato GetById(int ID)
        {
            return context.SVE_Candidatos.Where(c => c.Id == ID).Include(x => x.Estado_Candidato).FirstOrDefault();
        }

        public bool UpdateAll(List<Candidato> Candidatos)
        {
            context.SVE_Candidatos.UpdateRange(Candidatos);
            return SaveAll(Candidatos.Count);
        }

        public Candidato GetByIdVotacionGanador(int IDVE)
        {
            return context.SVE_Candidatos.Where(x => x.Votacion_ID == IDVE && x.Estado_Candidato.Descripcion == "Aceptado").FirstOrDefault();
        }

        public List<Candidato> GetAllSelecByVotacionID(int ID)
        {
            return context.SVE_Candidatos.Where(c => c.Votacion_ID == ID && c.Estado_Candidato.Descripcion == "Seleccionado").Include(c => c.Estado_Candidato).ToList();
        }

        public List<Candidato> GetAllSelected(int IDVE)
        {
            return context.SVE_Candidatos.Where(x => x.Votacion_ID == IDVE && x.Estado_Candidato.Descripcion == "Aceptado").ToList();
        }

        public Task<bool> DeleteALlByIDVotacion(int ID)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteAllByIDVotacion(int ID)
        {
            
            int filasEliminadas = await context.SVE_Candidatos.Where(c => c.Votacion_ID == ID).ExecuteDeleteAsync();
            return filasEliminadas > 0;

        }
    }
}
