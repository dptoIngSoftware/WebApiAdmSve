using System.ComponentModel.DataAnnotations;

namespace WebApiVotacionElectronica.Models.SVE
{
    public class Estado_Candidato
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(100)]
        public string Descripcion { get; set; }
    }
}
