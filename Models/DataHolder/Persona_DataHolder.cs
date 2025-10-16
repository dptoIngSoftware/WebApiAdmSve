using System.ComponentModel.DataAnnotations;

namespace WebApiVotacionElectronica.Models.DataHolder
{
    public class Persona_DataHolder
    {
        public string Rut { get; set; }
        public string Nombre_Completo { get; set; }
        public string Email { get; set; }
        public string Cargo { get; set; }
        public string Unidad { get; set; }
    }
}
