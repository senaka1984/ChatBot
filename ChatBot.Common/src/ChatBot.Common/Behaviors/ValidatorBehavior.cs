using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FluentValidation;
using MediatR;
using ChatBot.Common.EventBus.Extensions;
using System.Runtime.Serialization;
using System;

namespace ChatBot.Common.Pipeline.Behaviors
{
    public class ValidatorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<ValidatorBehavior<TRequest, TResponse>> _logger;
        private readonly IValidator<TRequest>[] _validators;

        public ValidatorBehavior(IValidator<TRequest>[] validators, ILogger<ValidatorBehavior<TRequest, TResponse>> logger)
        {
            _validators = validators;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var typeName = request.GetGenericTypeName();

            _logger.LogInformation("----- Validating command {CommandType}", typeName);

            var failures = _validators
                .Select(v => v.Validate(request))
                .SelectMany(result => result.Errors)
                .Where(error => error != null)
                .ToList();

            if (failures.Count > 0)
            {
                _logger.LogWarning("Validation errors - {CommandType} - Command: {@Command} - Errors: {@ValidationErrors}", typeName, request, failures);

                throw new Exception(
                      $"Command Validation Errors for type {typeof(TRequest).Name} Validation exception '{string.Join(",", failures)}'");
            }

            return await next();
        }
    }

    [System.Serializable]
    internal class ChatBotValidationException : System.Exception
    {
        public ChatBotValidationException()
        {
        }

        public ChatBotValidationException(string message) : base(message)
        {
        }

        public ChatBotValidationException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        protected ChatBotValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}