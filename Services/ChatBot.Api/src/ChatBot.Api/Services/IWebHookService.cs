using ChatBot.Services.Api.ViewModel.ChatBot.Commands;
using ChatBot.Services.Api.ViewModel.ChatBot.Response;
using RestEase;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ChatBot.Api.Services
{
    [SerializationMethods(Query = QuerySerializationMethod.Serialized)]
    public interface IWebHookService
    {
        #region COMMANDS

        [AllowAnyStatusCode]
        [Post("webhook")]
        Task TriggerCommand([Body] Update update);

        #endregion

        #region QUERY


        #endregion
    }
}
