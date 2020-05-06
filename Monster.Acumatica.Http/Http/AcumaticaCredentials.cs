using Monster.Acumatica.Config;
using Push.Foundation.Utilities.Json;

namespace Monster.Acumatica.Http
{
    public class AcumaticaCredentials
    {
        public string Branch { get; set; }
        public string CompanyName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string InstanceUrl { get; set; }

        public AcumaticaCredentials()
        {
        }

        public AcumaticaCredentials(AcumaticaCredentialsConfig config)
        {
            Branch = config.Branch;
            CompanyName = config.CompanyName;
            Username = config.Username;
            Password = config.Password;
            InstanceUrl = config.InstanceUrl;
        }
        
        // Payload gets sent to Acumatica
        public string AuthenticationJson
        {
            get
            {
                var content = new
                {
                    branch = Branch,
                    company = CompanyName,
                    name = Username,
                    password = Password,
                };

                return content.SerializeToJson();
            }
        }
    }
}

