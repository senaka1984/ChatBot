using System;

namespace ChatBot.Common.BaseException
{
#pragma warning disable IDE1006
    public class ChatBotException : Exception
#pragma warning restore IDE1006
    {
        public string Code { get; }

        public ChatBotException()
        {
        }

        public ChatBotException(string code)
        {
            Code = code;
        }

        public ChatBotException(string message, params object[] args)
            : this(string.Empty, message, args)
        {
        }

        public ChatBotException(string code, string message, params object[] args)
            : this(null, code, message, args)
        {
        }

        public ChatBotException(Exception innerException, string message, params object[] args)
            : this(innerException, string.Empty, message, args)
        {
        }

        public ChatBotException(Exception innerException, string code, string message, params object[] args)
            : base(string.Format(message, args), innerException)
        {
            Code = code;
        }

        public ChatBotException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}