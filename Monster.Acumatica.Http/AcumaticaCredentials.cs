using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Push.Foundation.Utilities.Json;

namespace Monster.Acumatica.Http
{
    public class AcumaticaCredentials
    {
        public string Branch = "MYCOMPANY";
        public string CompanyName = "MyCompany";
        public string Username = "admin";
        public string Password = "l0c4lInstance";

        public string AuthenticationContent
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

        public string InstanceUrl = "http://localhost/AcuInst2";
        public string EndpointPrefix = "/entity/Default/17.200.001/";

        public string LoginUrl => $"{InstanceUrl}/entity/auth/login";
        public string ServiceBaseUrl => $"{InstanceUrl}{EndpointPrefix}";

        public string ServiceMethodUrl(string path)
        {
            return $"{ServiceBaseUrl}{path}";
        }
    }
}
