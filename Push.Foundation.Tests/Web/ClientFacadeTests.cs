using System;
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
        public void ExecuteInvokesInsistentExecutorWithNonZeroRetries()
        {
            // Arrange
            var logger = MockRepository.GenerateStub<IPushLogger>();
            var insistor = MockRepository.GenerateStub<InsistentExecutor>();
            var requestProcessor = MockRepository.GenerateMock<HttpWebRequestProcessor>();
            var throttler = MockRepository.GenerateStub<Throttler>();
            var configuration = new HttpSettings();
            configuration.RetryLimit = 3;

            insistor.Expect(x => x.Execute((Func<ResponseEnvelope>) null)).IgnoreArguments();

            var sut = 
                new HttpFacade(
                    requestProcessor, configuration, throttler, insistor, logger);

            // Act
            var request = (HttpWebRequest)WebRequest.Create("http://test.com/a/b");
            sut.ExecuteRequest(request);

            // Assert
            insistor.VerifyAllExpectations();
        }

        [Test]
        public void ExecuteInvokesThrottler()
        {
            // Arrange
            var logger = MockRepository.GenerateStub<IPushLogger>();
            var insistor = MockRepository.GenerateStub<InsistentExecutor>();
            var requestProcessor = MockRepository.GenerateStub<HttpWebRequestProcessor>();
            requestProcessor
                .Expect(x => x.Execute(null))
                .IgnoreArguments()
                .Return(new ResponseEnvelope());


            var throttler = MockRepository.GenerateMock<Throttler>();
            var configuration = new HttpSettings();
            configuration.RetryLimit = 0;

            var request = (HttpWebRequest)WebRequest.Create("http://test.com/a/b");
            throttler.Expect(x => x.Process(String.Empty)).IgnoreArguments();

            var sut =
                new HttpFacade(
                    requestProcessor, configuration, throttler, insistor, logger);

            // Act
            sut.ExecuteRequest(request);

            // Assert
            throttler.VerifyAllExpectations();
        }

        [Test]
        public void ExecuteInvokesRequestProcessor()
        {
            // Arrange
            var configuration = new HttpSettings();
            configuration.RetryLimit = 0;

            var logger = MockRepository.GenerateStub<IPushLogger>();
            var insistor = MockRepository.GenerateStub<InsistentExecutor>();
            var throttler = MockRepository.GenerateStub<Throttler>();

            var request = (HttpWebRequest)WebRequest.Create("http://test.com/a/b");
            var requestProcessor = MockRepository.GenerateStub<HttpWebRequestProcessor>();
            requestProcessor
                .Expect(x => x.Execute(null))
                .IgnoreArguments()
                .Return(new ResponseEnvelope());

            var sut =
                new HttpFacade(
                    requestProcessor, configuration, throttler, insistor, logger);

            // Act
            sut.ExecuteRequest(request);

            // Assert
            requestProcessor.VerifyAllExpectations();
        }

    }
}
