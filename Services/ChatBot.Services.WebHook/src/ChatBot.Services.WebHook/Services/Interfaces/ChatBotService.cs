using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChatBot.Services.WebHook.ViewModel.ChatBot.Commands;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;


namespace ChatBot.Services.WebHook.Services.Interfaces
{
    public class ChatBotService : IChatBotService
    {

        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<ChatBotService> _logger;

        public ChatBotService(ITelegramBotClient botClient, ILogger<ChatBotService> logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        public async Task EchoAsync(ChatBotCommand update)
        {
            var handler = update.Type switch
            {              
                UpdateType.Message => BotOnMessageReceived(update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery),
                UpdateType.InlineQuery => BotOnInlineQueryReceived(update.InlineQuery),
                UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(update.ChosenInlineResult),
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(exception);
            }
        }

        private static readonly string[] Quoted = new[]
       {
            "God knows I couldn’t love them more, but even the Kennedys didn’t get together this often. —Jay Pritchett in Modern Family",
             "No need to seize the last word, Lord Baelish. I’ll assume it was something clever.” —Sansa Stark in Game of Thrones",
            "I don’t have anything against education—as long as it doesn’t interfere with your thinking.” —Ben Cartwright in Bonanza",
            "Do I remember Adrian Monk? That’s like asking the Titanic if it remembers the iceberg.” -Ralph “Father” Roberts in Monk"
        };

        private async Task BotOnMessageReceived(Message message)
        {
            _logger.LogInformation($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;

            var action = message.Text.Split(' ').First() switch
            {
                "/help" => Usage(_botClient, message),
                "/quotes" => Quotes(_botClient, message),
                "/education" => SendInlineKeyboard(_botClient, message),
                "/age" => SendReplyKeyboard(_botClient, message),               
                _ => Help(_botClient, message)
            };
            var sentMessage = await action;
            _logger.LogInformation($"The message was sent with id: {sentMessage.MessageId}");

            // Send inline keyboard
            // You can process responses in BotOnCallbackQueryReceived handler
            static async Task<Message> SendInlineKeyboard(ITelegramBotClient bot, Message message)
            {
                await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                // Simulate longer running task
                await Task.Delay(500);

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Diploma", "Diploma"),
                        InlineKeyboardButton.WithCallbackData("Degree", "Degree"),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Masters", "Masters"),
                        InlineKeyboardButton.WithCallbackData("Phd", "Phd"),
                    },
                });

                return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: "Choose Your Highest Education",
                                                      replyMarkup: inlineKeyboard);
            }

            static async Task<Message> SendReplyKeyboard(ITelegramBotClient bot, Message message)
            {
                var replyKeyboardMarkup = new ReplyKeyboardMarkup(
                    new KeyboardButton[][]
                    {
                        new KeyboardButton[] { "10-20", "20-30" },
                        new KeyboardButton[] { "30-40", "40-50" },
                    })
                {
                    ResizeKeyboard = true
                };

                return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: "Choose Your Age Group",
                                                      replyMarkup: replyKeyboardMarkup);
            }

            static async Task<Message> RemoveKeyboard(ITelegramBotClient bot, Message message)
            {
                return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: "Removing keyboard",
                                                      replyMarkup: new ReplyKeyboardRemove());
            }

            static async Task<Message> SendFile(ITelegramBotClient bot, Message message)
            {
                await bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                const string filePath = @"Files/tux.png";
                using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

                return await bot.SendPhotoAsync(chatId: message.Chat.Id,
                                                photo: new InputOnlineFile(fileStream, fileName),
                                                caption: "Nice Picture");
            }

            static async Task<Message> RequestContactAndLocation(ITelegramBotClient bot, Message message)
            {
                var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
                {
                    KeyboardButton.WithRequestLocation("Location"),
                    KeyboardButton.WithRequestContact("Contact"),
                });

                return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: "Who or Where are you?",
                                                      replyMarkup: RequestReplyKeyboard);
            }

            static async Task<Message> Help(ITelegramBotClient bot, Message message)
            {
                const string usage = "Enter /help to list all commands";
                                  

                return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: usage,
                                                      replyMarkup: new ReplyKeyboardRemove());
            }


            static async Task<Message> Quotes(ITelegramBotClient bot, Message message)
            {

                var rng = new Random();
               var content = Enumerable.Range(1, 1).Select(index => new QuoteSummary
                {
                  Content = Quoted[rng.Next(Quoted.Length)]
                })
                .ToArray();


                return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text:content[0].Content,
                                                      replyMarkup: new ReplyKeyboardRemove());
            }

            


        static async Task<Message> Usage(ITelegramBotClient bot, Message message)
            {
                const string usage = "Please enter a command:\n" +
                                     "/quotes   - list random quote\n" +
                                     "/education   - Whats your highest education\n" +
                                     "/age - Whats your age group";

                return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                      text: usage,
                                                      replyMarkup: new ReplyKeyboardRemove());
            }
        }

        // Process Inline Keyboard callback data
        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            await _botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"Received {callbackQuery.Data}");

            await _botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: $"Received {callbackQuery.Data}");
        }

        #region Inline Mode

        private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery)
        {
            _logger.LogInformation($"Received inline query from: {inlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                // displayed result
                new InlineQueryResultArticle(
                    id: "3",
                    title: "TgBots",
                    inputMessageContent: new InputTextMessageContent(
                        "hello"
                    )
                )
            };

            await _botClient.AnswerInlineQueryAsync(inlineQueryId: inlineQuery.Id,
                                                    results: results,
                                                    isPersonal: true,
                                                    cacheTime: 0);
        }

        private Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult)
        {
            _logger.LogInformation($"Received inline result: {chosenInlineResult.ResultId}");
            return Task.CompletedTask;
        }

        #endregion

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            _logger.LogInformation($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }

        public Task HandleErrorAsync(Exception exception)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogInformation(ErrorMessage);
            return Task.CompletedTask;
        }

       
    }

    public class QuoteSummary
    {      

        public string Content { get; set; }
    }
}
