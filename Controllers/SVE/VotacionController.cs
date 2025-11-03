using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SimpleBase;
using System.Buffers.Text;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
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

        public VotacionController(IServiceProvider serviceProvider,IConfiguration configuration, IBackgroundEmailQueue emailQueue, ILogger<VotacionController> logger, IVoto_Repository voto_Repository, ICandidato_Repository candidato_Repository, IVotante_Repository votante_Repository, IEstado_Candidato_Repository estado_Candidato_Repository, IVotacion_Repository votacion_Repository, ISede_Repository sede_Repository, IEstado_Votacion_Repository estado_Votacion_Repository)
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


        //[Authorize]
        [HttpPost("GetAll")]
        public IActionResult GetAllVotaciones([FromBody] Filtro_DataHolder Filtro)
        {
            var votaciones = votacion_Repository.GetAll(Filtro);
            return Ok(votaciones);
        }

        //[Authorize]
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


        //[Authorize]
        [HttpPut]
        [Route("Activar/{ID}")]
        public IActionResult ActivarVotacion(int ID)
        {
            Votacion votacion = votacion_Repository.GetById(ID);
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

            List<Votante> Votantes = votante_Repository.GetAllByVotacionID(ID);            
            foreach (var votante in Votantes)
            {
                original = votante.Rut+"-"+votante.Id+-+votante.Votacion_ID;
                URL = BaseURL+Encrypt(original, claveSecreta);


                var workItem = new EmailWorkItem
                {
                    Destinatario = votante.Email,
                    Nombre = votante.Nombre_Completo,
                    Asunto = votacion.Nombre,
                    Link = URL,
                    VotanteId = votante.Id
                };

                emailQueue.Enqueue(workItem);

            }
          
            return Ok("Votación activada correctamente");
        }



        //[Authorize]
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

            List<int> CandidatosIDs = voto_Repository.TopCandidatos(votacion.CandidatosXvoto,ID);

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



        //[Authorize]
        [HttpPost]
        [Route("SGVE/{IDVE}/{IDC}")]
        public IActionResult SeleccionarGanador(int IDVE,int IDC) 
        {

            if (IDC == 0) 
            {
                Votacion votacionN = votacion_Repository.GetById(IDVE);
                votacionN.Estado_Votacion = estado_Votacion_Repository.GetEstadoByDescr("Nula");
                if (!votacion_Repository.Update(votacionN))
                {
                    return BadRequest("No se pudo Finalizar la Votación");
                }


                List<int> CandidatosIDsN = voto_Repository.TopCandidatos(votacionN.CandidatosXvoto, IDVE);

                Candidato candidatoN = new Candidato();
                List<Candidato> candidatosN = new();

                foreach (int id in CandidatosIDsN)
                {
                    candidatoN = new();
                    candidatoN = candidato_Repository.GetById(id);
                    candidatoN.Estado_Candidato = estado_Candidato_Repository.GetEstadoByDescr("Disponible");
                    
                    candidatosN.Add(candidatoN);

                }

                //se actulizan 
                if (!candidato_Repository.UpdateAll(candidatosN))
                {
                    return BadRequest("Error al Actulizar Candidatos");

                }

                return Ok("Votación Anulada Correctamente");

            }


            Votacion votacion = votacion_Repository.GetById(IDVE);
            votacion.Estado_Votacion = estado_Votacion_Repository.GetEstadoByDescr("Finalizada");

            if (!votacion_Repository.Update(votacion))
            {
                return BadRequest("No se pudo Finalizar la Votación");
            }

            // ahora se marcan los candidatos

            List<int> CandidatosIDs = voto_Repository.TopCandidatos(votacion.CandidatosXvoto, IDVE);

            Candidato candidato = new Candidato();
            List<Candidato> candidatos = new();

            foreach (int id in CandidatosIDs)
            {
                candidato = new();
                candidato = candidato_Repository.GetById(id);
                if (id == IDC)
                {
                    candidato.Estado_Candidato = estado_Candidato_Repository.GetEstadoByDescr("Aceptado");
                }
                else
                {
                    candidato.Estado_Candidato = estado_Candidato_Repository.GetEstadoByDescr("Disponible");
                }
                candidatos.Add(candidato);

            }

            //se actulizan 
            if (!candidato_Repository.UpdateAll(candidatos))
            {
                return BadRequest("Error al Actulizar Candidatos");

            }

            return Ok("Votación Finalizada Correctamente");
            
        }



        //envio correos
        //[Authorize]
        [HttpPost]
        [Route("Correos/{ID}")]
        public IActionResult CorreosVotacion(int ID)
        {
            Votacion votacion = votacion_Repository.GetById(ID);

            string original = "";
            string claveSecreta = "T5lne8%4#r7=x%09"; // 16 bytes

            string BaseURL = configuration["MySettings:WEBVEURL"];
            string URL = "";

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
                    VotanteId = votante.Id
                };

                emailQueue.Enqueue(workItem);

            }

            return Ok();
        }


        //[Authorize]
        [HttpGet]
        [Route("InfoVE/{ID}")]
        public IActionResult GetInfoVotacion(int ID) 
        {
            Votacion votacion = votacion_Repository.GetById(ID);
            List<CandidatoxVoto_DataHolder> Candidatos = new();
            if (votacion.Estado_Votacion.Descripcion == "Finalizada")
            {
                Candidato CG = candidato_Repository.GetByIdVotacionGanador(ID);
                CandidatoxVoto_DataHolder DVEDH = new()
                {
                    candidatoid = CG.Id,
                    Total = voto_Repository.votosByIDcandidato(CG.Id),
                    Candidato = CG,
                    Ganador = true
                };

                Candidatos.Add(DVEDH);
            }
            else
            {
                Candidatos = voto_Repository.Candidatos(votacion.CandidatosXvoto, ID);

                foreach (var item in Candidatos)
                {
                    item.Candidato = candidato_Repository.GetById(item.candidatoid);

                }
            }



            INFOVE_DataHolder INFO = new()
            {
                Totalnulos = voto_Repository.TotalNulosByIDVotacion(ID),
                TotalVotos = voto_Repository.TotalVotosByIDVotacion(ID),               
                Candidatos_Top = Candidatos
            };

            return Ok(INFO); 
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

    }
}
