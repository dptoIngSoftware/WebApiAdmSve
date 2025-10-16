using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiVotacionElectronica.Models.DataHolder;
using WebApiVotacionElectronica.Models.SVE;
using WebApiVotacionElectronica.Repository;
using WebApiVotacionElectronica.Repository.Interfaces;

namespace WebApiVotacionElectronica.Controllers.SVE
{
    [Route("api/[controller]")]
    [ApiController]
    public class VotacionController : ControllerBase
    {
        private readonly Voto_Repository voto_Repository;
        private readonly ICandidato_Repository candidato_Repository;
        private readonly IVotante_Repository votante_Repository;
        private readonly IEstado_Candidato_Repository estado_Candidato_Repository;
        private readonly IVotacion_Repository votacion_Repository;
        private readonly ISede_Repository sede_Repository;
        private readonly IEstado_Votacion_Repository estado_Votacion_Repository;

        public VotacionController(Voto_Repository voto_Repository,ICandidato_Repository candidato_Repository, IVotante_Repository votante_Repository, IEstado_Candidato_Repository estado_Candidato_Repository, IVotacion_Repository votacion_Repository, ISede_Repository sede_Repository, IEstado_Votacion_Repository estado_Votacion_Repository)
        {
            this.voto_Repository = voto_Repository;
            this.candidato_Repository = candidato_Repository;
            this.votante_Repository = votante_Repository;
            this.estado_Candidato_Repository = estado_Candidato_Repository;
            this.votacion_Repository = votacion_Repository;
            this.sede_Repository = sede_Repository;
            this.estado_Votacion_Repository = estado_Votacion_Repository;
        }


        [Authorize]
        [HttpGet("GetAll")]
        public IActionResult GetAllVotaciones([FromBody] Filtro_DataHolder Filtro)
        {
            var votaciones = votacion_Repository.GetAll(Filtro);
            return Ok(votaciones);
        }

        [Authorize]
        [HttpPost("Crear")]
        public IActionResult CrearVotacion([FromBody] Votacion_DataHolder Nueva_Votacion)
        {

            // Lógica para crear una nueva votación
            //1.Se crea la votacion
            Votacion NV = new()
            {
                Nombre = Nueva_Votacion.Nombre,
                Descripcion = Nueva_Votacion.Descripcion,
                FechaInicio = Nueva_Votacion.FechaInicio,
                FechaTermino = Nueva_Votacion.FechaTermino,
                Activa = false,
                CandidatosXvoto = Nueva_Votacion.CandidatosXvoto,
                FechaCreacion = DateTime.Now,
                Estado_Votacion = estado_Votacion_Repository.GetEstadoByDescr("Creada"),
                Sede = sede_Repository.GetByCod(Nueva_Votacion.Sede)
            };

            if (!votacion_Repository.Create(NV))
            {
                return BadRequest("No se pudo crear la votacion");
            }

            //2.Se Crean los candidatos
            List<Candidato> Candidatos = new();
            Candidato NC = new();
            foreach (var candidato in Nueva_Votacion.Candidatos)
            {
                string[] partes = candidato.Rut.Split('-');
                NC = new()
                {
                    Votacion_ID = NV.Id,
                    Rut = partes[0],
                    DV = partes[1],
                    Nombre_Completo = candidato.Nombre_Completo,
                    Email = candidato.Email,
                    Cargo = candidato.Cargo,
                    Unidad = candidato.Unidad,
                    Estado_Candidato = estado_Candidato_Repository.GetEstadoByDescr("Disponible")

                };
                Candidatos.Add(NC);
            }

            if (!candidato_Repository.CreateAll(Candidatos))
            {
                return BadRequest("No se pudieron crear los candidatos");
            }

            //3.Se crean los votantes

            List<Votante> Votantes = new();
            Votante NVot = new();
            foreach (var votante in Nueva_Votacion.Votantes)
            {
                string[] partes = votante.Rut.Split('-');
                NVot = new()
                {
                    Votacion_ID = NV.Id,
                    Rut = partes[0],
                    DV = partes[1],
                    Nombre_Completo = votante.Nombre_Completo,
                    Email = votante.Email,
                    Cargo = votante.Cargo,
                    Unidad = votante.Unidad,
                    Ha_Votado = false
                };
                Votantes.Add(NVot);
            }

            if (!votante_Repository.CreateAll(Votantes))
            {
                return BadRequest("No se pudieron crear los votantes");
            }

            return Ok("Votacion creada exitosamente");

        }


        [Authorize]
        [HttpPut]
        [Route("Activar/{ID}")]
        public IActionResult ActivarVotacion(int ID)
        {
            Votacion votacion = votacion_Repository.GetById(ID);
            votacion.Activa = true;
            votacion.Estado_Votacion = estado_Votacion_Repository.GetEstadoByDescr("Activa");

            if (!votacion_Repository.Update(votacion))
            {
                return BadRequest("No se pudo activar la votacion");
            }

            return Ok("Votacion activada correctamente");
        }


        //Votacion

        [Authorize]
        [HttpPost("Votar/{ID_Votacion}/{ID_Votante}")]
        public IActionResult Votar(int ID_Votacion, int ID_Votante, [FromBody] List<int> Candidatos_ID)
        {
            //1.Revisar que el votante no haya votado
            if (votante_Repository.Ha_Votado(ID_Votante, ID_Votacion))
            {
                return BadRequest("El votante ya ha votado en esta votacion");
            }

            //2.Revisar que la votacion este activa
            Votacion votacion = votacion_Repository.GetById(ID_Votacion);
            if (votacion == null || !votacion.Activa)
            {
                return BadRequest("La votacion no esta activa");
            }

            //3.Revisar que la cantidad de candidatos seleccionados no supere el limite
            if (Candidatos_ID.Count > votacion.CandidatosXvoto)
            {
                return BadRequest($"No se pueden seleccionar mas de {votacion.CandidatosXvoto} candidatos");
            }




            List<Voto> Votos = new();
            Voto NV = new();
            Votante votante = votante_Repository.GetByID(ID_Votante);
            //4.Registrar el voto
            if (Candidatos_ID.Count == 1 && Candidatos_ID[0] == 0)
            {
                //Voto en blanco se ingresa como nulo
                NV = new()
                {
                    Votante = votante,
                    Candidato = null,
                    FechaVoto = DateTime.Now,
                };
                Votos.Add(NV);

            }
            else
            {
                foreach (var IDC in Candidatos_ID)
                {
                    NV = new()
                    {
                        Votante = votante,
                        Candidato = candidato_Repository.GetById(IDC),
                        FechaVoto = DateTime.Now,
                    };
                    Votos.Add(NV);
                }
            }

            if(!voto_Repository.CreateAll(Votos)) 
            {
                if (Votos.Count == 1)
                {
                    return BadRequest("No se pudo registrar el voto.");
                }
                else
                {
                    return BadRequest("No se pudieron registrar los votos");
                }
            }

            //5.Marcar al votante como que ya voto
            votante.Ha_Votado = true;
            if (!votante_Repository.Update(votante))
            {
                return BadRequest("No se pudo actualizar el estado del votante");
            }

            return Ok("Voto registrado correctamente");

        }

    }
}
