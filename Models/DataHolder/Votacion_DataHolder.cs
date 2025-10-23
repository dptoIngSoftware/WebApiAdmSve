using System.ComponentModel.DataAnnotations;
using WebApiVotacionElectronica.Models.SVE;

namespace WebApiVotacionElectronica.Models.DataHolder
{
    public class Votacion_DataHolder
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaTermino { get; set; }
        public int CandidatosXvoto { get; set; }
        public string Sede { get; set; }

        public IFormFile Candidatos_doc { get; set; }
        public IFormFile Votantes_doc { get; set; }


        public List<Persona_DataHolder>? Candidatos { get; set; }
        public List<Persona_DataHolder>? Votantes { get; set; }

    }
}
