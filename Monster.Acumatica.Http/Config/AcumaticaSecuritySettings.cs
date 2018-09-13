using Push.Foundation.Utilities.Json;

namespace Monster.Acumatica.Config
{
    public class AcumaticaSecuritySettings
    {
        public string Branch { get; set; }
        public string CompanyName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string InstanceUrl { get; set; }

        public AcumaticaSecuritySettings()
        {
        }

        public AcumaticaSecuritySettings(AcumaticaSecurityConfig config)
        {
            Branch = config.Branch;
            CompanyName = config.CompanyName;
            Username = config.Username;
            Password = config.Password;
            InstanceUrl = config.InstanceUrl;
        }


        public string AuthenticationJson
        {
            get
            {
                var content = new
                {
                    branch = Branch,
                    companyname = CompanyName,
                    name = Username,
                    password = Password,
                };

                return content.SerializeToJson();
            }
        }
    }
}

