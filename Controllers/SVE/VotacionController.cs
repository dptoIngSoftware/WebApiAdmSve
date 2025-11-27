using DinkToPdf;
using DinkToPdf.Contracts;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SimpleBase;
using System.Buffers.Text;
using System.Data;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using WebApiVotacionElectronica.Helper;
using WebApiVotacionElectronica.Models.DataHolder;
using WebApiVotacionElectronica.Models.SVE;
using WebApiVotacionElectronica.Models.Tools;
using WebApiVotacionElectronica.Repository;
using WebApiVotacionElectronica.Repository.Interfaces;
using WebApiVotacionElectronica.Services;

namespace WebApiVotacionElectronica.Controllers.SVE
{
    [Route("api/[controller]")]
    [ApiController]
    public class VotacionController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IConfiguration configuration;
        private readonly IBackgroundEmailQueue emailQueue;
        private readonly ILogger<VotacionController> logger;
        private readonly IVoto_Repository voto_Repository;
        private readonly ICandidato_Repository candidato_Repository;
        private readonly IVotante_Repository votante_Repository;
        private readonly IEstado_Candidato_Repository estado_Candidato_Repository;
        private readonly IVotacion_Repository votacion_Repository;
        private readonly ISede_Repository sede_Repository;
        private readonly IEstado_Votacion_Repository estado_Votacion_Repository;

        public VotacionController(IServiceProvider serviceProvider, IConfiguration configuration, IBackgroundEmailQueue emailQueue, ILogger<VotacionController> logger, IVoto_Repository voto_Repository, ICandidato_Repository candidato_Repository, IVotante_Repository votante_Repository, IEstado_Candidato_Repository estado_Candidato_Repository, IVotacion_Repository votacion_Repository, ISede_Repository sede_Repository, IEstado_Votacion_Repository estado_Votacion_Repository)
        {
            this.serviceProvider = serviceProvider;
            this.configuration = configuration;
            this.emailQueue = emailQueue;
            this.logger = logger;
            this.voto_Repository = voto_Repository;
            this.candidato_Repository = candidato_Repository;
            this.votante_Repository = votante_Repository;
            this.estado_Candidato_Repository = estado_Candidato_Repository;
            this.votacion_Repository = votacion_Repository;
            this.sede_Repository = sede_Repository;
            this.estado_Votacion_Repository = estado_Votacion_Repository;
        }


        [Authorize]
        [HttpPost("GetAll")]
        public IActionResult GetAllVotaciones([FromBody] Filtro_DataHolder Filtro)
        {
            var votaciones = votacion_Repository.GetAll(Filtro);
            return Ok(votaciones);
        }

        [Authorize]
        [HttpPost("Crear")]
        public async Task<IActionResult> CrearVotacion([FromForm] Votacion_DataHolder Nueva_Votacion)
        {
            try
            {
                Nueva_Votacion.Candidatos = await LeerExcelPersonasAsync(Nueva_Votacion.Candidatos_doc);
                Nueva_Votacion.Votantes = await LeerExcelPersonasAsync(Nueva_Votacion.Votantes_doc);
            }
            catch (Exception)
            {

                return BadRequest("XLSBAD");
            }



            //validaciones
            List<string> MensajeError = new();
            if (!ValidarRuts(Nueva_Votacion.Candidatos))
            {
                MensajeError.Add("RCINV"); // Ruts Candidatos Invalidos
            }
            if (!RutRepetidos(Nueva_Votacion.Candidatos))
            {
                MensajeError.Add("RCRPT"); // Ruts Candidatos Repetidos
            }
            if (!ValidarRuts(Nueva_Votacion.Votantes))
            {
                MensajeError.Add("RVINV"); // Ruts Votantes Invalidos
            }
            if (!RutRepetidos(Nueva_Votacion.Votantes))
            {
                MensajeError.Add("RVRPT"); // Ruts Votantes Repetidos
            }

            if (MensajeError.Count != 0)
            {
                return BadRequest(MensajeError);
            }

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
                return BadRequest("No se pudo crear la Votación");
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
                    Nombre_Completo = candidato.Nombre_Completo.ToUpperInvariant(),
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
                    Ha_Votado = false,
                    Correo_Enviado = null
                };
                Votantes.Add(NVot);
            }

            if (!votante_Repository.CreateAll(Votantes))
            {
                return BadRequest("No se pudieron crear los votantes");
            }

            return Ok("Votación creada exitosamente");

        }


        [Authorize]
        [HttpPost("Editar/{ID}")]
        public async Task<IActionResult> EditarVotacion([FromForm] Votacion_DataHolder Nueva_Votacion, int ID)
        {

            Votacion NV = votacion_Repository.GetById(ID);
            bool NVS = false;
            bool NCS = false;


            //se carga los candidatos nuevos
            if (Nueva_Votacion.Candidatos_doc != null)
            {
                try
                {

                    Nueva_Votacion.Candidatos = await LeerExcelPersonasAsync(Nueva_Votacion.Candidatos_doc);

                }
                catch (Exception)
                {

                    return BadRequest("XLSBAD");
                }
                finally
                {
                    NCS = true;
                }

            }
            //se carga los Votantes nuevos
            if (Nueva_Votacion.Votantes_doc != null)
            {
                try
                {

                    Nueva_Votacion.Votantes = await LeerExcelPersonasAsync(Nueva_Votacion.Votantes_doc);
                }
                catch (Exception)
                {

                    return BadRequest("XLSBAD");
                }
                finally
                {
                    NVS = true;
                }

            }


            //validaciones
            List<string> MensajeError = new();
            if (NCS)
            {
                if (!ValidarRuts(Nueva_Votacion.Candidatos))
                {
                    MensajeError.Add("RCINV"); // Ruts Candidatos Invalidos
                }
                if (!RutRepetidos(Nueva_Votacion.Candidatos))
                {
                    MensajeError.Add("RCRPT"); // Ruts Candidatos Repetidos
                }
            }

            if (NVS)
            {
                if (!ValidarRuts(Nueva_Votacion.Votantes))
                {
                    MensajeError.Add("RVINV"); // Ruts Votantes Invalidos
                }
                if (!RutRepetidos(Nueva_Votacion.Votantes))
                {
                    MensajeError.Add("RVRPT"); // Ruts Votantes Repetidos
                }
            }


            if (MensajeError.Count != 0)
            {
                return BadRequest(MensajeError);
            }



            if (NCS)
            {
                //2.Se Crean los candidatos
                if (!await candidato_Repository.DeleteAllByIDVotacion(NV.Id))
                {
                    return BadRequest("NDC");
                }
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
                        Nombre_Completo = candidato.Nombre_Completo.ToUpperInvariant(),
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
            }


            if (NVS)
            {
                //3.Se crean los votantes

                if (!await votante_Repository.DeleteAllByIDVotacion(NV.Id))
                {
                    return BadRequest("NDV");

                }

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
                        Ha_Votado = false,
                        Correo_Enviado = null
                    };
                    Votantes.Add(NVot);
                }

                if (!votante_Repository.CreateAll(Votantes))
                {
                    return BadRequest("No se pudieron crear los votantes");
                }
            }


            NV.Nombre = Nueva_Votacion.Nombre;
            NV.Descripcion = Nueva_Votacion.Descripcion;
            NV.FechaInicio = Nueva_Votacion.FechaInicio;
            NV.FechaTermino = Nueva_Votacion.FechaTermino;
            NV.CandidatosXvoto = Nueva_Votacion.CandidatosXvoto;

            if (!votacion_Repository.Update(NV))
            {
                return BadRequest("No se pudo actualizar la Votación");
            }

            return Ok("Votación actualizada exitosamente");

        }


        [Authorize]
        [HttpPut]
        [Route("Activar/{ID}")]
        public IActionResult ActivarVotacion(int ID)
        {

            Votacion votacion = votacion_Repository.GetById(ID);
            if (votacion_Repository.HayActivaSede(votacion.Sede.Id))
            {
                return BadRequest("YAPS");
            }


            votacion.Activa = true;
            votacion.Estado_Votacion = estado_Votacion_Repository.GetEstadoByDescr("Activada");

            if (!votacion_Repository.Update(votacion))
            {
                return BadRequest("No se pudo activar la Votación");
            }


            string original = "";
            string claveSecreta = "T5lne8%4#r7=x%09"; // 16 bytes

            string BaseURL = configuration["MySettings:WEBVEURL"];
            string URL = "";

            CultureInfo cultura = new CultureInfo("es-ES");

            List<Votante> Votantes = votante_Repository.GetAllByVotacionID(ID);
            foreach (var votante in Votantes)
            {
                original = votante.Rut + "-" + votante.Id + -+votante.Votacion_ID;
                URL = BaseURL + Encrypt(original, claveSecreta);


                var workItem = new EmailWorkItem
                {
                    Destinatario = votante.Email,
                    Nombre = votante.Nombre_Completo,
                    Asunto = votacion.Nombre,
                    Link = URL,
                    DESC = votacion.Descripcion,
                    FI = votacion.FechaInicio.ToString("dddd dd 'de' MMMM 'del' yyyy", cultura),
                    FT = votacion.FechaTermino.ToString("dddd dd 'de' MMMM 'del' yyyy", cultura),
                    VotanteId = votante.Id
                };

                emailQueue.Enqueue(workItem);

            }

            return Ok("Votación activada correctamente");
        }



        [Authorize]
        [HttpPut]
        [Route("CerrarVotacion/{ID}")]
        public IActionResult CerrarVotacion(int ID)
        {
            Votacion votacion = votacion_Repository.GetById(ID);
            votacion.Activa = false;
            votacion.Estado_Votacion = estado_Votacion_Repository.GetEstadoByDescr("Con Resultado");

            if (!votacion_Repository.Update(votacion))
            {
                return BadRequest("No se pudo Cerrar la Votación");
            }

            // ahora se marcan los candidatos

            List<int> CandidatosIDs = voto_Repository.TopCandidatos(votacion.CandidatosXvoto, ID);

            Candidato candidato = new Candidato();
            List<Candidato> candidatos = new();

            foreach (int id in CandidatosIDs)
            {
                candidato = new();
                candidato = candidato_Repository.GetById(id);
                candidato.Estado_Candidato = estado_Candidato_Repository.GetEstadoByDescr("Seleccionado");
                candidatos.Add(candidato);

            }

            //se actulizan 
            if (!candidato_Repository.UpdateAll(candidatos))
            {
                return BadRequest("Error al Buscar Candidatos");

            }

            return Ok("Periodo de Votación Cerrado Correctamente");
        }

        //Votacion

        [Authorize]
        [HttpPost("Votar/{ID_Votacion}/{ID_Votante}")]
        public IActionResult Votar(int ID_Votacion, int ID_Votante, [FromBody] List<int> Candidatos_ID)
        {
            //1.Revisar que el votante no haya votado
            if (votante_Repository.Ha_Votado(ID_Votante, ID_Votacion))
            {
                return BadRequest("El votante ya ha votado en esta Votación");
            }

            //2.Revisar que la votacion este activa
            Votacion votacion = votacion_Repository.GetById(ID_Votacion);
            if (votacion == null || !votacion.Activa)
            {
                return BadRequest("La Votación no esta activa");
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
                    Votante = ID_Votante,
                    Candidato = null,
                    FechaVoto = DateTime.Now,
                    Votacion_ID = ID_Votacion
                };
                Votos.Add(NV);

            }
            else
            {
                foreach (var IDC in Candidatos_ID)
                {
                    NV = new()
                    {
                        Votante = ID_Votante,
                        Candidato = IDC,
                        FechaVoto = DateTime.Now,
                        Votacion_ID = ID_Votacion
                    };
                    Votos.Add(NV);
                }
            }

            if (!voto_Repository.CreateAll(Votos))
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



        [Authorize]
        [HttpPost]
        [Route("SGVE/{IDVE}/{IDC}")]
        public IActionResult SeleccionarGanador(int IDVE, int IDC)
        {

            if (IDC == 0)
            {
                Votacion votacionN = votacion_Repository.GetById(IDVE);
                votacionN.Estado_Votacion = estado_Votacion_Repository.GetEstadoByDescr("Nula");
                if (!votacion_Repository.Update(votacionN))
                {
                    return BadRequest("No se pudo Finalizar la Votación");
                }

                return Ok("Votación Anulada Correctamente");

            }


            Votacion votacion = votacion_Repository.GetById(IDVE);
            votacion.Estado_Votacion = estado_Votacion_Repository.GetEstadoByDescr("Finalizada");

            if (!votacion_Repository.Update(votacion))
            {
                return BadRequest("No se pudo Finalizar la Votación");
            }



            List<Candidato> candidatos = candidato_Repository.GetAllSelecByVotacionID(IDVE);
            foreach (var item in candidatos)
            {
                item.Estado_Candidato = estado_Candidato_Repository.GetEstadoByDescr("Aceptado");
            }
            //se actulizan 
            if (!candidato_Repository.UpdateAll(candidatos))
            {
                return BadRequest("Error al Actulizar Candidatos");

            }

            return Ok("Votación Finalizada Correctamente");

        }


        [Authorize]
        [HttpGet]
        [Route("AllV/{IDVE}")]
        public IActionResult GetAllVotos(int IDVE)
        {
            List<CandidatoxVoto_DataHolder> Candidatos = voto_Repository.Candidatos(1000, IDVE);
            return Ok(Candidatos);
        }

        [Authorize]
        [HttpGet]
        [Route("GetData/{Valor}/{IDV}")]
        public IActionResult GetDataVotacion(int Valor, int IDV)
        {
            if (Valor == 1)
            {
                var Cs = candidato_Repository.GetAllByVotacionID(IDV);
                return Ok(Cs);
            }
            else
            {
                var Vs = votante_Repository.GetAllByVotacionID(IDV);
                return Ok(Vs);
            }
        }




        //envio correos
        [Authorize]
        [HttpPost]
        [Route("Correos/{ID}")]
        public IActionResult CorreosVotacion(int ID)
        {
            Votacion votacion = votacion_Repository.GetById(ID);

            string original = "";
            string claveSecreta = "T5lne8%4#r7=x%09"; // 16 bytes

            string BaseURL = configuration["MySettings:WEBVEURL"];
            string URL = "";

            CultureInfo cultura = new CultureInfo("es-ES");

            List<Votante> Votantes = votante_Repository.GetAllByVotacionIDPendientes(ID);
            foreach (var votante in Votantes)
            {
                original = votante.Rut + "-" + votante.Id + -+votante.Votacion_ID;
                URL = BaseURL + Encrypt(original, claveSecreta);


                var workItem = new EmailWorkItem
                {
                    Destinatario = votante.Email,
                    Nombre = votante.Nombre_Completo,
                    Asunto = votacion.Nombre,
                    Link = URL,
                    DESC = votacion.Descripcion,
                    FI = votacion.FechaInicio.ToString("dddd dd 'de' MMMM 'del' yyyy", cultura),
                    FT = votacion.FechaTermino.ToString("dddd dd 'de' MMMM 'del' yyyy", cultura),
                    VotanteId = votante.Id
                };

                emailQueue.Enqueue(workItem);

            }
            votacion.FechaReenvio = DateTime.Now;
            if (!votacion_Repository.Update(votacion))
            {
                return BadRequest("No se pudo actualizar la Votación");

            }

            return Ok();
        }


        [Authorize]
        [HttpGet]
        [Route("InfoVE/{ID}")]
        public IActionResult GetInfoVotacion(int ID)
        {
            Votacion votacion = votacion_Repository.GetById(ID);
            List<CandidatoxVoto_DataHolder> Candidatos = new();
            if (votacion.Estado_Votacion.Descripcion == "Finalizada")
            {
                Candidatos = voto_Repository.CandidatosAceptados(votacion.CandidatosXvoto, ID);
            }
            else
            {
                Candidatos = voto_Repository.CandidatosSeleccionados(votacion.CandidatosXvoto, ID);
            }



            INFOVE_DataHolder INFO = new()
            {
                Totalnulos = voto_Repository.TotalNulosByIDVotacion(ID),
                TotalVotos = voto_Repository.TotalVotosByIDVotacion(ID),
                VotantesQuevotaron = voto_Repository.VotantesqueVotaron(ID),
                TotalVotantes = votante_Repository.CantidadporVotacionID(ID),
                Candidatos_Top = Candidatos
            };

            return Ok(INFO);
        }


        [Authorize]
        [HttpPost]
        [Route("CGVE/{IDVE}/{IDC}")]
        public IActionResult CambiarGanador(int IDVE, int IDC)
        {
            Votacion votacion = votacion_Repository.GetById(IDVE);
            List<CandidatoxVoto_DataHolder> Candidatos = voto_Repository.CandidatosDisponibles(1, IDVE);
            Candidato CandidatoR = candidato_Repository.GetById(IDC);
            // no hay mas candidatos para dejar como ganador
            if (Candidatos == null || Candidatos.Count == 0)
            {
                return BadRequest("CD0");
            }

            //poner mensaje de que si en candidato tiene 0 votos retorne CD0V

            List<Candidato> CVEs = new();
            CandidatoR.Estado_Candidato = estado_Candidato_Repository.GetEstadoByDescr("Rechazado");
            Candidatos[0].Candidato.Estado_Candidato = estado_Candidato_Repository.GetEstadoByDescr("Seleccionado");

            CVEs.Add(CandidatoR);
            CVEs.Add(Candidatos[0].Candidato);

            if (!candidato_Repository.UpdateAll(CVEs))
            {
                return BadRequest("EUCD");
            }


            return Ok("Cambio Realizado con exito");
        }

        [Authorize]
        [HttpPost]
        [Route("QGVE/{IDC}")]
        public IActionResult QuitarGanador(int IDC)
        {

            Candidato CandidatoR = candidato_Repository.GetById(IDC);

            CandidatoR.Estado_Candidato = estado_Candidato_Repository.GetEstadoByDescr("Rechazado");

            if (!candidato_Repository.Update(CandidatoR))
            {
                return BadRequest("Error al Quitar Ganador");

            }

            return Ok("Cambio Realizado con exito");
        }


        [Authorize]
        [HttpGet]
        [Route("PDF/{id}")]
        public async Task<IActionResult> GenerarActa(int id) 
        {

            Votacion votacion = votacion_Repository.GetById(id);
            List<CandidatoxVoto_DataHolder> candidatos = voto_Repository.Candidatos(20, id);

            INFOVE_DataHolder INFO = new()
            {
                Totalnulos = voto_Repository.TotalNulosByIDVotacion(id),
                TotalVotos = voto_Repository.TotalVotosByIDVotacion(id),
                VotantesQuevotaron = voto_Repository.VotantesqueVotaron(id),
                TotalVotantes = votante_Repository.CantidadporVotacionID(id),
                Candidatos_Top = candidatos
            };

            var imagePath = Path.Combine(AppContext.BaseDirectory, "Imagenes", "logo-ucn.png");
            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            string base64Image = Convert.ToBase64String(imageBytes);

            string logoUrl = $"data:image/png;base64,{base64Image}";

            string html = GenerateHtml(votacion, INFO, logoUrl); 

            var converter = HttpContext.RequestServices.GetService<IConverter>();

            var doc = new HtmlToPdfDocument()
            {
               GlobalSettings = {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
               },
               Objects = {
                 new ObjectSettings() {
                    HtmlContent = html,
                    WebSettings = {
                        DefaultEncoding = "utf-8",
                        LoadImages = true
                    },
                    LoadSettings = {
                        BlockLocalFileAccess = false
                    }
                 }
               }
            };

            var file = converter.Convert(doc);

            return File(file, "application/pdf", "ActaVotacion.pdf");
        }

        private string GenerateHtml(Votacion votacion, INFOVE_DataHolder INFO, string logoUrl)
        {
            CultureInfo cultura = new CultureInfo("es-ES");
            string FI = votacion.FechaInicio.ToString("dddd dd 'de' MMMM 'del' yyyy", cultura);
            string FT = votacion.FechaTermino.ToString("dddd dd 'de' MMMM 'del' yyyy", cultura);

            string html = $@"
            <html>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        margin: 30px;
                    }}

                    .center {{
                        text-align: center;
                    }}

                    .title {{
                        font-size: 28px;
                        font-weight: bold;
                        margin-top:10px;
                    }}

                    table {{
                        width: 100%;
                        border-collapse: collapse;
                        margin-top: 20px;
                    }}

                    thead {{
                        display: table-header-group;
                        background-color: #2c3e50;
                        color: white;
                    }}

                    tfoot {{
                        display: table-footer-group;
                    }}

                    th {{
                           background-color: #2c3e50;
                           color: white;
                           padding: 8px;
                           text-align: left;
                           border: 1px solid #ddd;
                    }}

                    td {{
                        padding: 8px;
                        border: 1px solid #ddd;
                    }}

                    tr {{
                        page-break-inside: avoid;
                    }}

                    .section-title {{
                        margin-top: 25px;
                        font-size: 20px;
                        font-weight: bold;
                    }}
                </style>
            </head>

            <body>

                <div class='center'>
                    <img src='{logoUrl}' style='height:80px; margin-bottom:10px;' />
                    <div class='title'>Acta del Proceso de Votación</div>
                </div>

                <p><strong>Nombre:</strong> {votacion.Nombre}.</p>
                <p><strong>Descripción:</strong> {votacion.Descripcion}.</p>
                <p><strong>Periodo de Votación:</strong> {FI} hasta el {FT}.</p>

                <div class='section-title'>Resumen General</div>

                <table>
                    <tbody>
                        <tr><th>Total de votos válidos</th><td>{INFO.TotalVotos}</td></tr>
                        <tr><th>Total de votos nulos</th><td>{INFO.Totalnulos}</td></tr>
                        <tr><th>Personas que votaron</th><td>{INFO.VotantesQuevotaron}</td></tr>
                        <tr><th>Total de votantes</th><td>{INFO.TotalVotantes}</td></tr>
                    </tbody>
                </table>

                <div class='section-title'>Postulantes con votos registrados</div>

                <table>
                    <thead>
                        <tr>
                            <th>Nombre</th>
                            <th>RUT</th>
                            <th>Votos</th>
                        </tr>
                    </thead>
                    <tbody>";

                        // generar filas con zebra
                        bool zebra = false;

                        foreach (var c in INFO.Candidatos_Top)
                        {
                            string bg = zebra ? "#f5f5f5" : "#ffffff";
                            zebra = !zebra;

                            html += $@"
                            <tr style='background:{bg}; page-break-inside: avoid;'>
                            <td>{c.Candidato.Nombre_Completo}</td>
                            <td>{c.Candidato.Rut}-{c.Candidato.DV}</td>
                            <td>{c.Total}</td>
                            </tr>";
                        }

                        html += @"
                    </tbody>
                </table>
            </body>
            </html>";

            return html;
        }


        [HttpPost("GetVotar")]
        public IActionResult TEST2([FromBody] string CODE)
        {
            if (!IsValidBase58(CODE))
            {
                return BadRequest("Código inválido");
            }

            try
            {
                string claveSecreta = "T5lne8%4#r7=x%09"; // 16 bytes

                string desencriptado = Decrypt(CODE, claveSecreta);


                string[] partes = desencriptado.Split('-');

                if (!votante_Repository.Exists(int.Parse(partes[1]),int.Parse(partes[2])))
                {
                    return BadRequest("Votante no existe en la votacion");
                }

                Votante votante = votante_Repository.GetByID(int.Parse(partes[1]));

                if (votante.Ha_Votado)
                {
                    return BadRequest("El votante ya ha votado en esta Votación");
                }

                Votacion votacion = votacion_Repository.GetByIdNoTrack(int.Parse(partes[2]));

                List<Candidato> candidatos = candidato_Repository.GetAllByVotacionID(votacion.Id);

                Votador_Dataholder votador = new()
                {
                    Votacion = votacion,
                    IdVotante = votante.Id,
                    Has_Voted = votante.Ha_Votado,
                    Candidatos = candidatos,
                    TKN = crearToken()
                };
                
                return Ok(votador);


            }
            catch
            {
                return BadRequest("Código dañado");
            }

        }

        private static async Task<List<Persona_DataHolder>> LeerExcelPersonasAsync(IFormFile archivo)
        {
            var lista = new List<Persona_DataHolder>();

            using (var stream = new MemoryStream())
            {
                await archivo.CopyToAsync(stream);
                stream.Position = 0;

                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true // Usa la primera fila como encabezado
                        }
                    });

                    var tabla = dataSet.Tables[0]; // Primera hoja

                    foreach (DataRow fila in tabla.Rows)
                    {
                        var persona = new Persona_DataHolder
                        {
                            Rut = fila["Rut"]?.ToString()?.Trim(),
                            Nombre_Completo = fila["Nombre_Completo"]?.ToString()?.Trim(),
                            Email = fila["Email"]?.ToString()?.Trim(),
                            Cargo = fila["Cargo"]?.ToString()?.Trim(),
                            Unidad = fila["Unidad"]?.ToString()?.Trim()
                        };

                        lista.Add(persona);
                    }
                }
            }

            return lista;
        }


        private static string Encrypt(string plainText, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] iv = new byte[IvLength];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(iv, 0, RandomIvBytes);

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var ms = new MemoryStream())
                {
                    ms.Write(iv, 0, IvLength);
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                        sw.Write(plainText);

                    byte[] encrypted = ms.ToArray();
                    // Base58 limpio, sin = + /
                    return Base58.Bitcoin.Encode(encrypted);
                }
            }
        }

        private static string Decrypt(string cipherText, string key)
        {
            byte[] fullCipher = Base58.Bitcoin.Decode(cipherText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            byte[] iv = new byte[IvLength];
            Array.Copy(fullCipher, 0, iv, 0, IvLength);

            byte[] cipher = new byte[fullCipher.Length - IvLength];
            Array.Copy(fullCipher, IvLength, cipher, 0, cipher.Length);

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var ms = new MemoryStream(cipher))
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                    return sr.ReadToEnd();
            }
        }

        private static readonly int IvLength = 16; // AES requiere 16 bytes IV
        private static readonly int RandomIvBytes = 8; // Solo 8 aleatorios, 8 en cero

        private static bool IsValidBase58(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Rango razonable para evitar strings demasiado cortos o largos
            if (input.Length < 16 || input.Length > 200)
                return false;

            // Solo caracteres Base58 válidos
            return input.All(c => "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz".Contains(c));
        }

        private string crearToken()
        {
            try
            {
                string subject = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Jwt")["Subject"];
                string keySettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Jwt")["Key"];
                string issuer = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Jwt")["Issuer"];
                string audience = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Jwt")["Audience"];
                //string IdSis = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("MySettings")["idSis"];

                var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, subject),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),ClaimValueTypes.Integer64),
                    new Claim(ClaimTypes.Role,"ADM001")
                    //new Claim("Id", IdSis),

                   };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keySettings));

                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddHours(10), signingCredentials: signIn);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                // Info  
                System.Diagnostics.Debug.WriteLine(ex);
                throw ex;
            }
        }

        private bool ValidarRuts(List<Persona_DataHolder> personas)
        {
            return personas.All(p => Regex.IsMatch(p.Rut, @"^\d{7,8}-[\dkK]$"));
        }

        private bool RutRepetidos(List<Persona_DataHolder> personas)
        {
            return personas.Select(x => x.Rut.ToUpper()).Distinct().Count() == personas.Count;
        }
    }
}
