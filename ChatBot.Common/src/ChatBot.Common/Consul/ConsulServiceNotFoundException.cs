using System;

namespace ChatBot.Common.Consul
{
    public class ConsulServiceNotFoundException : Exception
    {
        public string ServiceName { get; set; }

        public ConsulServiceNotFoundException(string serviceName) : this(string.Empty, serviceName)
        {
        }

        public ConsulServiceNotFoundException(string message, string serviceName) : base(message)
        {
            ServiceName = serviceName;
        }

        public ConsulServiceNotFoundException() : base()
        {
        }

        public ConsulServiceNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}