using System.ComponentModel.DataAnnotations;

namespace WebApiVotacionElectronica.Models.SVE
{
    public class Votacion
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(200)]
        public string Nombre { get; set; }
        [MaxLength(500)]
        public string Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaTermino { get; set; }
        public int CandidatosXvoto { get; set; }
        public bool Activa { get; set; }
        public Sede Sede { get; set; }
        public Estado_Votacion Estado_Votacion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaReenvio { get; set; }

    }
}
