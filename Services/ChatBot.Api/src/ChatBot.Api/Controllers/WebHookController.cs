using ChatBot.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ChatBot.Api.Controllers
{
   
    public class WebhookController : ControllerBase
    {
        private readonly IWebHookService _webHookService;

        public WebhookController(IWebHookService webhookService)
        {
            _webHookService = webhookService;
        }

        [HttpPost]      
        public async Task<IActionResult> Post([FromBody]  Update update)        
        {
            await _webHookService.TriggerCommand(update);
            return Ok();
        }
    }
}
