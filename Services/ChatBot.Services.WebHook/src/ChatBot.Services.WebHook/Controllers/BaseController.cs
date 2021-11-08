using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace ChatBot.Services.WebHook.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BaseController : ControllerBase
    {
        private readonly IMediator _dispatcher;

        public BaseController(IMediator dispatcher)
        {
            _dispatcher = dispatcher;
        }

        protected async Task<TResult> QueryAsync<TResult>(IRequest<TResult> query)
             => await _dispatcher.Send<TResult>(query);

        protected async Task<TResult> SendAsync<TResult>(IRequest<TResult> command)
             => await _dispatcher.Send<TResult>(command);

        protected async Task<ActionResult> SendAsync(IRequest<ActionResult> command)
           => await _dispatcher.Send(command);

        protected async Task SendAsync(object command) => await _dispatcher.Publish(command);

        protected ActionResult<T> Single<T>(T data)
        {
            if (data == null)
            {
                return NotFound();
            }

            return Ok(data);
        }

       

      

        protected Guid UserId
            => string.IsNullOrWhiteSpace(User?.Identity?.Name) ?
                Guid.Empty :
                Guid.Parse(User.Identity.Name);
    }
}
