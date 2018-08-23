﻿using System;
using System.Net;
using NUnit.Framework;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.HttpClient;
using Push.Foundation.Web.Misc;
using Rhino.Mocks;

namespace Push.Foundation.Tests.Web
{
    [TestFixture]
    public class ClientFacadeTests
    {

        [Test]
        public void ExecuteInvokesFactory()
        {
            // Arrange
            var request = new RequestEnvelope("GET", "http://google.com");

            var testWebRequest = (HttpWebRequest)WebRequest.Create(request.Url);

            var factory = MockRepository.GenerateMock<IRequestBuilder>();
            factory
                .Expect(x => x.Make(request))
                .Return(testWebRequest);

            var logger = MockRepository.GenerateStub<IPushLogger>();
            var insistor = MockRepository.GenerateStub<InsistentExecutor>();

            var response = MockRepository.GenerateMock<ResponseEnvelope>();
            response.Expect(x => x.ProcessStatusCodes());

            var requestProcessor = MockRepository.GenerateMock<HttpWebRequestProcessor>();
            requestProcessor
                .Expect(x => x.Execute(testWebRequest))
                .Return(response);

            var throttler = MockRepository.GenerateMock<Throttler>();
            throttler
                .Expect(x => x.Process(null))
                .IgnoreArguments();

            var configuration = new HttpSettings();

            var sut = 
                new HttpFacade(
                    factory, configuration, requestProcessor, 
                    throttler, insistor, logger);


            // Act
            sut.ExecuteRequest(request);

            // Assert
            factory.VerifyAllExpectations();
            requestProcessor.VerifyAllExpectations();
            throttler.VerifyAllExpectations();
            response.VerifyAllExpectations();
        }
        
    }
}
