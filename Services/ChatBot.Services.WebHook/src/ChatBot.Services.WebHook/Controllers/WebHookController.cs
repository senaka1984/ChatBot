using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ChatBot.Services.WebHook.ViewModel.ChatBot.Commands;
namespace ChatBot.Services.WebHook.Controllers
{
    [Route("[controller]")]
    public class WebHookController : BaseController
    {
        public WebHookController(IMediator dispatcher) : base(dispatcher)
        { }

        #region COMMAND

        [HttpPost("")]
        public async Task TriggerCommand(ChatBotCommand command)
            =>  await SendAsync(command);
       

        #endregion

        #region QUERY


        #endregion
    }
}
