using Microsoft.AspNetCore.Mvc;

namespace RoyalVillaApi.Controllers;

[ApiController]
[Route("api/villa")]
[ApiExplorerSettings(GroupName = "v1")]
public class VillaController : ControllerBase
{
    [HttpGet]
    public string GetVillas()
    {
        return "Get Villas";
    }
    
    [HttpGet("{id:int}")]
    public string GetVillaById(int id)
    {
        return "Get Villas: " + id;
    }
}
