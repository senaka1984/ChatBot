using ChatBot.Services.Api.ViewModel.ChatBot.Response;
using MediatR;
using Telegram.Bot.Types;

namespace ChatBot.Services.Api.ViewModel.ChatBot.Commands
{
    public class ChatBotCommand : Update, IRequest<ChatBotResponse> // , Update
    {
      
    }
}
