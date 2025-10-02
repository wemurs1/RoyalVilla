using Microsoft.AspNetCore.Mvc;

namespace RoyalVilla_API.Controllers
{
    [ApiController]
    public class VillaController : ControllerBase
    {
        [HttpGet]
        [Route("/villas")]
        public string GetVillas()
        {
            return "Get all Villas";
        }


    }
}
