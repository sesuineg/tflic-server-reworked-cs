using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers;

[ApiController]
public class PingController : ControllerBase
{
    [HttpGet("/Ping")]
    public ActionResult<string> Ping() =>
        Ok("Pong");
}