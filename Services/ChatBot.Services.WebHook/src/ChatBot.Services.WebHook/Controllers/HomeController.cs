
using Microsoft.AspNetCore.Mvc;

namespace ChatBot.Services.WebHook.Controllers
{
    [Route("")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("ChatBot WebHook Service");
    }
}