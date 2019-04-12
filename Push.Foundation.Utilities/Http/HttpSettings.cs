namespace Push.Foundation.Utilities.Http
{
    public class HttpSettings
    {
        public int MaxAttempts { get; set; }
        public int Timeout { get; set; }
        public int ThrottlingDelay { get; set; }
        public bool RetriesEnabled => MaxAttempts > 0;
        

        public HttpSettings()
        {
            MaxAttempts = 0;
            Timeout = 60000;
            ThrottlingDelay = 0;
        }
    }
}

