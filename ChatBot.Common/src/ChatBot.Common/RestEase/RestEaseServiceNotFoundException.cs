using System;

namespace ChatBot.Common.RestEase
{
    public class RestEaseServiceNotFoundException : Exception
    {
        public string ServiceName { get; set; }
        public RestEaseServiceNotFoundException()
        {
        }

        public RestEaseServiceNotFoundException(string serviceName) : this(string.Empty, serviceName)
        {
        }

        public RestEaseServiceNotFoundException(string message, string serviceName) : base(message)
        {
            ServiceName = serviceName;
        }

        public RestEaseServiceNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}