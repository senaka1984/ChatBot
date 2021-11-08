using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Newtonsoft.Json;
using ChatBot.Common.EventBus.Extensions;
using Serilog;
using Serilog.Enrichers.Sensitive;

namespace ChatBot.Common.Pipeline.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger _logger;
        public LoggingBehavior(ILogger logger) => _logger = logger;

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            using (_logger.EnterSensitiveArea())
            {
                _logger.Information("----- Handling command {CommandName} ({@Command})", request.GetGenericTypeName(), JsonConvert.SerializeObject(request));
                var response = await next();
                _logger.Information("----- Command {CommandName} handled - response: {@Response}", request.GetGenericTypeName(), JsonConvert.SerializeObject(response));
                return response;
            }
        }
    }
}