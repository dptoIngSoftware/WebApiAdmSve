using Microsoft.EntityFrameworkCore;
using WebApiVotacionElectronica.Context;
using WebApiVotacionElectronica.Models.DataHolder;
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

        public List<CandidatoxVoto_DataHolder> Candidatos(int top, int VotacionID)
        {
            var topCandidatos = context.SVE_Votos
                .Include(v => v.CandidatoInfo)
                .ThenInclude(c => c.Estado_Candidato)
                .Where(v => v.Candidato != null
                    && v.Votacion_ID == VotacionID)
                .GroupBy(v => v.CandidatoInfo)
                .Select(g => new CandidatoxVoto_DataHolder
                {
                    candidatoid = g.Key.Id,
                    Total = g.Count(),
                    Candidato = g.Key,
                    Ganador = null
                })
                .Where(x => x.Total > 0)
                .OrderByDescending(x => x.Total)
                .Take(top)
                .ToList();

            return topCandidatos;
        }

        public List<CandidatoxVoto_DataHolder> CandidatosAceptados(int top, int VotacionID)
        {
            var topCandidatos = context.SVE_Votos
                .Include(v => v.CandidatoInfo)
                .ThenInclude(c => c.Estado_Candidato)
                .Where(v => v.Candidato != null
                    && v.Votacion_ID == VotacionID
                    && v.CandidatoInfo.Estado_Candidato.Descripcion == "Aceptado")
                .GroupBy(v => v.CandidatoInfo)
                .Select(g => new CandidatoxVoto_DataHolder
                {
                    candidatoid = g.Key.Id,
                    Total = g.Count(),
                    Candidato = g.Key,
                    Ganador = true
                })
                .OrderByDescending(x => x.Total)
                .Take(top)
                .ToList();

            return topCandidatos;
        }

        public List<CandidatoxVoto_DataHolder> CandidatosDisponibles(int top, int VotacionID)
        {
            var topCandidatos = context.SVE_Votos
                .Include(v => v.CandidatoInfo)
                .ThenInclude(c => c.Estado_Candidato)
                .Where(v => v.Candidato != null
                    && v.Votacion_ID == VotacionID
                    && v.CandidatoInfo.Estado_Candidato.Descripcion == "Disponible")
                .GroupBy(v => v.CandidatoInfo)
                .Select(g => new CandidatoxVoto_DataHolder
                {
                    candidatoid = g.Key.Id,
                    Total = g.Count(),
                    Candidato = g.Key,
                    Ganador = null
                })
                .Where(x => x.Total > 0)
                .OrderByDescending(x => x.Total)
                .Take(top)
                .ToList();

            return topCandidatos;
        }

        public List<CandidatoxVoto_DataHolder> CandidatosSeleccionados(int top, int VotacionID)
        {
            var topCandidatos = context.SVE_Votos
                .Include(v => v.CandidatoInfo)                     
                .ThenInclude(c => c.Estado_Candidato)             
                .Where(v => v.Candidato != null
                    && v.Votacion_ID == VotacionID
                    && v.CandidatoInfo.Estado_Candidato.Descripcion == "Seleccionado")
                .GroupBy(v => v.CandidatoInfo)                     
                .Select(g => new CandidatoxVoto_DataHolder
                {
                    candidatoid = g.Key.Id,
                    Total = g.Count(),
                    Candidato = g.Key,                             
                    Ganador = null
                })
                .OrderByDescending(x => x.Total)
                .Take(top)
                .ToList();

            return topCandidatos;
        }

        public bool CreateAll(List<Voto> Votos)
        {
            context.SVE_Votos.AddRange(Votos);
            return SaveAll(Votos.Count);
        }

        public List<Voto> GetByVotacion(int VotacionID)
        {
            return context.SVE_Votos.Where(v => v.Votacion_ID == VotacionID).AsNoTracking().ToList();
        }

        public bool SaveAll(int Length)
        {
            return context.SaveChanges() == Length;
        }

        public List<int> TopCandidatos(int top, int VotacionID)
        {
            var Top = context.SVE_Votos.Where(v => v.Candidato != null && v.Votacion_ID == VotacionID)
                                        .GroupBy(v => v.Candidato)            
                                        .Select(g => new
                                        {
                                            CandidatoId = g.Key,
                                            TotalVotos = g.Count()
                                        })
                                        .Where(x => x.TotalVotos > 0)
                                        .OrderByDescending(x => x.TotalVotos)    
                                        .Take(top)                              
                                        .Select(x => x.CandidatoId.Value)              
                                        .ToList();

            return Top;
        }

        public int TotalNulosByIDVotacion(int ID_Votacion)
        {
            return context.SVE_Votos.Where(x => x.Votacion_ID == ID_Votacion && x.Candidato == null).Count();
        }

        public int TotalVotosByIDVotacion(int ID_Votacion)
        {
            return context.SVE_Votos.Where(x => x.Votacion_ID == ID_Votacion).Count();
        }

        public int votosByIDcandidato(int IDC)
        {
            return context.SVE_Votos.Where(x => x.Candidato == IDC).Count();
        }
    }
}
