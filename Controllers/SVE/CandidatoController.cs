using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiVotacionElectronica.Repository.Interfaces;

namespace WebApiVotacionElectronica.Controllers.SVE
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidatoController : ControllerBase
    {
        private readonly ICandidato_Repository candidato_Repository;

        public CandidatoController(ICandidato_Repository candidato_Repository)
        {
            this.candidato_Repository = candidato_Repository;
        }

        [Authorize]
        [HttpGet("GetAllByVotacionID/{ID}")]
        public IActionResult GetAllByVotacionID(int ID)
        {
            var candidatos = candidato_Repository.GetAllByVotacionID(ID);
            return Ok(candidatos);
        }
    }
}
