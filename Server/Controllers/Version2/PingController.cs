using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers.Version2;

[ApiController]
[Route("api/v2")]
public class PingController : ControllerBase
{
    [HttpGet("/ping")]
    public ActionResult<string> Ping() =>
        Ok("pong");
}