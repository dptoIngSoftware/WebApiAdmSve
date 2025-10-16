using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiVotacionElectronica.Repository.Interfaces;

namespace WebApiVotacionElectronica.Controllers.SVE
{
    [Route("api/[controller]")]
    [ApiController]
    public class VotoController : ControllerBase
    {
        private readonly IVoto_Repository voto_Repository;

        public VotoController(IVoto_Repository voto_Repository)
        {
            this.voto_Repository = voto_Repository;
        }

        [Authorize]
        [HttpGet("VotosByVotacion/{ID}")]
        public IActionResult VotosByVotacion(int ID)
        {
            var votos = voto_Repository.GetByVotacion(ID);
            return Ok(votos);
        }
    }
}
