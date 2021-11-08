using System.Collections.Generic;

namespace ChatBot.Services.WebHook
{    

    public class RestEaseSettings
    {
        public List<ServicesSettings> Services { get; set; }
    }

    public class ServicesSettings
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public string Scheme { get; set; }
        public string Port { get; set; }
    }

}
