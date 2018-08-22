using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Push.Foundation.Web.HttpClient;

namespace Monster.Acumatica.Http
{
    public class SpikeRepository
    {
        private readonly HttpFacade _clientFacade;
        private readonly AcumaticaRequestBuilder _requestBuilder;
        private readonly AcumaticaCredentials _credentials;

        public SpikeRepository(HttpFacade clientFacade)
        {
            _clientFacade = clientFacade;


            // TODO: wire these into the DI architecture
            var settings = new AcumaticaHttpSettings();
           _credentials = new AcumaticaCredentials();
            _requestBuilder = new AcumaticaRequestBuilder(settings);
        }


        public void RetrieveSession()
        {
            var path = _credentials.LoginUrl;
            var content = _credentials.AuthenticationContent;
            var request = _requestBuilder.HttpPost(path, content);
            var response = _clientFacade.ExecuteRequest(request);
        }

        public string RetrieveItemClass()
        {
            var path = _credentials.ServiceMethodUrl("ItemClass");
            var request = _requestBuilder.HttpGet(path);
            var response = _clientFacade.ExecuteRequest(request);
            return response.Body;
        }
    }
}
