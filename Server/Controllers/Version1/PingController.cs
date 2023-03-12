using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers.Version1;

[ApiController]
public class PingController : ControllerBase
{
    [HttpGet("/Ping")]
    public ActionResult<string> Ping() =>
        Ok("Pong");
}