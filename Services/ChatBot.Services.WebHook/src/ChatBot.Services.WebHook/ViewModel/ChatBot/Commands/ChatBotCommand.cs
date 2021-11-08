using ChatBot.Services.WebHook.ViewModel.ChatBot.Response;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ChatBot.Services.WebHook.ViewModel.ChatBot.Commands
{
    public class ChatBotCommand : Update, IRequest<string>
    {
      
    }
}
