using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ChatBot.Services.WebHook.ViewModel.ChatBot.Commands;
using ChatBot.Services.WebHook.Services.Interfaces;

namespace ChatBot.Services.WebHook.Handlers.CommandHandlers
{
    public class ChatBotCommandHandler : IRequestHandler<ChatBotCommand,string>
    {
        private readonly ChatBotService _chatBotService;

        public ChatBotCommandHandler(ChatBotService service)
        {
            _chatBotService = service;
        }

        public async Task<string> Handle(ChatBotCommand command, CancellationToken cancellationToken)
        {
            await _chatBotService.EchoAsync(command);
            return "Success";
        }

    }
}
