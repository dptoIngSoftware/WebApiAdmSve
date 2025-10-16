using System.ComponentModel.DataAnnotations;

namespace WebApiVotacionElectronica.Models.SVE
{
    public class Votante
    {
        [Key]
        public int Id { get; set; }
        public int Votacion_ID { get; set; }
        [MaxLength(20)]
        public string Rut { get; set; }
        [MaxLength(1)]
        public string DV { get; set; }
        [MaxLength(200)]
        public string Nombre_Completo { get; set; }
        [MaxLength(100)]
        public string Email { get; set; }
        [MaxLength(50)]
        public string Cargo { get; set; }
        [MaxLength(50)]
        public string Unidad { get; set; }
        public bool Ha_Votado { get; set; }


    }
}
