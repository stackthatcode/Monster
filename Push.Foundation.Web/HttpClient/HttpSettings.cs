namespace Push.Foundation.Web.HttpClient
{
    public class HttpSettings
    {
        public int RetryLimit { get; set; }
        public int Timeout { get; set; }
        public int ThrottlingDelay { get; set; }
        public bool RetriesEnabled => RetryLimit > 0;
        

        public HttpSettings()
        {
            RetryLimit = 3;
            Timeout = 60000;
            ThrottlingDelay = 0;
        }
    }
}

