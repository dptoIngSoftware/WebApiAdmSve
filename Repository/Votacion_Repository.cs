using Microsoft.EntityFrameworkCore;
using WebApiVotacionElectronica.Context;
using WebApiVotacionElectronica.Models.DataHolder;
using WebApiVotacionElectronica.Models.SVE;
using WebApiVotacionElectronica.Repository.Interfaces;

namespace WebApiVotacionElectronica.Repository
{
    public class Votacion_Repository:IVotacion_Repository
    {
        private readonly DBContext context;

        public Votacion_Repository(DBContext context)
        {
            this.context = context;
        }

        public bool Create(Votacion votacion)
        {
            context.SVE_Votaciones.Add(votacion);
            return Save();
        }

        public bool Update(Votacion votacion)
        {
            context.SVE_Votaciones.Update(votacion);
            return Save();
        }

        public bool Save()
        {
           return context.SaveChanges() > 0;
        }

        public Votacion GetById(int id)
        {
            return context.SVE_Votaciones.Where(v => v.Id == id).Include(x => x.Sede).Include(x => x.Estado_Votacion).FirstOrDefault();
        }

        public List<Votacion> GetAll(Filtro_DataHolder Filtro)
        {
            var query = context.SVE_Votaciones.AsQueryable();

            if (!string.IsNullOrEmpty(Filtro.Estado))
            {
                query = query.Where(v => v.Estado_Votacion.Descripcion == Filtro.Estado);
            }

            if (Filtro.FechaInicio.HasValue)
            {
                query = query.Where(v => v.FechaInicio >= Filtro.FechaInicio.Value);
            }

            if (Filtro.FechaTermino.HasValue)
            {
                query = query.Where(v => v.FechaTermino <= Filtro.FechaTermino.Value);
            }

            if (!string.IsNullOrEmpty(Filtro.Sede))
            {
                query = query.Where(v => v.Sede.Codigo == Filtro.Sede);
            }

            if (Filtro.FechaCreacion.HasValue)
            {
                query = query.Where(v => v.FechaCreacion.Date == Filtro.FechaCreacion.Value.Date);
            }

            if (!string.IsNullOrEmpty(Filtro.Nombre))
            {
                query = query.Where(v => v.Nombre.Contains(Filtro.Nombre));
            }

            query = query.OrderByDescending(x => x.FechaCreacion);

            query = query.Include(x => x.Sede).Include(x => x.Estado_Votacion).Skip((Filtro.CurrentPage - 1) * Filtro.PerPage).Take(Filtro.PerPage).AsNoTracking();

            return query.ToList();
        }

        public Votacion GetByIdNoTrack(int id)
        {
            return context.SVE_Votaciones.Where(v => v.Id == id).Include(x => x.Sede).Include(x => x.Estado_Votacion).AsNoTracking().FirstOrDefault();
        }

        public bool HayActivaSede(int ID)
        {
            return context.SVE_Votaciones.Any(v => v.Sede.Id == ID && v.Estado_Votacion.Descripcion == "Activada");
        }
    }
}
