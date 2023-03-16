using Microsoft.AspNetCore.Mvc;

namespace TFlic.Controllers.Version2;

[ApiController]
[Route("api/v2")]
public class PingController : ControllerBase
{
    [HttpGet("/ping")]
    public ActionResult<string> Ping() =>
        Ok("pong");
}