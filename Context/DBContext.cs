using Microsoft.EntityFrameworkCore;
using WebApiVotacionElectronica.Models.SVE;

namespace WebApiVotacionElectronica.Context
{
    public class DBContext:DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
                
        }

        //Tables
        public DbSet<Candidato> SVE_Candidatos { get; set; }
        public DbSet<Sede> SVE_Sedes { get; set; }
        public DbSet<Votacion> SVE_Votaciones { get; set; }
        public DbSet<Voto> SVE_Votos { get; set; }
        public DbSet<Estado_Candidato> SVE_Estados_Candidato { get; set; }
        public DbSet<Estado_Votacion> SVE_Estados_Votacion { get; set; }
        public DbSet<Votante> SVE_Votantes { get; set; }

    }
}
