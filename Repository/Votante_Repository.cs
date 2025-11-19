using Microsoft.EntityFrameworkCore;
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

        public async Task ActualizarEnvioCorreoAsync(int idUsuario, bool envioExitoso)
        {
            Votante votante = context.SVE_Votantes.Where(x => x.Id == idUsuario).FirstOrDefault();
            if (votante == null)
            {
                return;
            }

            votante.Correo_Enviado = envioExitoso;
            await context.SaveChangesAsync(); 
        }

        public int CantidadporVotacionID(int ID_Votacion)
        {
            return context.SVE_Votantes.Count(v => v.Votacion_ID == ID_Votacion);

        }

        public bool CreateAll(List<Votante> Votantes)
        {
            context.SVE_Votantes.AddRange(Votantes);
            return SaveAll(Votantes.Count);
        }

        public async Task<bool> DeleteAllByIDVotacion(int ID)
        {
            int filasEliminadas = await context.SVE_Votantes.Where(c => c.Votacion_ID == ID).ExecuteDeleteAsync();
            return filasEliminadas > 0;
        }

        public bool Exists(int ID_Votante, int ID_Votacion)
        {
            return context.SVE_Votantes.Any(v => v.Id == ID_Votante && v.Votacion_ID == ID_Votacion);
        }

        public List<Votante> GetAllByVotacionID(int ID)
        {
            return context.SVE_Votantes.Where(v => v.Votacion_ID == ID).ToList();
        }

        public List<Votante> GetAllByVotacionIDPendientes(int ID)
        {
            return context.SVE_Votantes.Where(v => v.Votacion_ID == ID && v.Ha_Votado == false).ToList();
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
