using ChatBot.Services.WebHook.ViewModel.ChatBot.Commands;
using RestEase;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ChatBot.Services.WebHook.Services.Interfaces
{
    public interface IChatBotService
    {
         Task EchoAsync(ChatBotCommand update);
     
    }
}
