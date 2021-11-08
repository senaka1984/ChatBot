namespace ChatBot.Common.Consul
{
    public class ConsulOptions
    {
        public bool Enabled { get; set; }
        public string Url { get; set; }
        //Service Name
        public string Service { get; set; }
        //Address (Url) to Service
        public string Address { get; set; }
        public int Port { get; set; }
        public int GrpcPort { get; set; } = 0;
        public bool PingEnabled { get; set; } = true;
        public string PingEndpoint { get; set; }
        public int PingInterval { get; set; }
        public int RemoveAfterInterval { get; set; }
        public int RequestRetries { get; set; }
    }
}