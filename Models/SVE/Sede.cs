using System.ComponentModel.DataAnnotations;

namespace WebApiVotacionElectronica.Models.SVE
{
    public class Sede
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(100)]
        public string Codigo { get; set; }
        [MaxLength(200)]
        public string Descripcion { get; set; }
    }
}
