using Push.Foundation.Utilities.Json;

namespace Monster.Acumatica.Http
{
    public class AcumaticaCredentials
    {
        public string Branch = "MYCOMPANY";
        public string CompanyName = "MyCompany";
        public string Username = "admin";
        public string Password = "l0c4lInstance";

        public string InstanceUrl = "http://localhost/AcuInst2";

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

