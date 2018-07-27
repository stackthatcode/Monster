using System.Net;
using NUnit.Framework;
using Push.Foundation.Web.HttpClient;

namespace Push.Foundation.Tests.Web
{
    [TestFixture]
    public class ResponseEnvelopeTests
    {
        [Test]
        public void Http200ReturnsSelf()
        {
            // Assemble
            var sut = new ResponseEnvelope();
            sut.StatusCode = HttpStatusCode.Accepted;
            
            // Act
            var result = sut.ProcessStatusCodes();

            // Assert
            Assert.AreSame(result, sut);            
        }

        [Test]
        public void Http404ReturnsSelf()
        {
            // Assemble
            var sut = new ResponseEnvelope();
            sut.StatusCode = HttpStatusCode.NotFound;

            // Act
            var result = sut.ProcessStatusCodes();

            // Assert
            Assert.AreSame(result, sut);
        }

        [Test]
        public void Http403ThrowsError()
        {
            // Assemble
            var sut = new ResponseEnvelope();
            sut.StatusCode = HttpStatusCode.Forbidden;

            // Act & Assert
            Assert.Throws<BadStatusCodeException>(
                () =>
                {
                    sut.ProcessStatusCodes();
                });
        }

        [Test]
        public void Http401ThrowsError()
        {
            // Assemble
            var sut = new ResponseEnvelope();
            sut.StatusCode = HttpStatusCode.Unauthorized;

            // Act & Assert
            Assert.Throws<BadStatusCodeException>(
                () =>
                {
                    sut.ProcessStatusCodes();
                });
        }

        [Test]
        public void Http500ThrowsError()
        {
            // Assemble
            var sut = new ResponseEnvelope();
            sut.StatusCode = HttpStatusCode.InternalServerError;

            // Act & Assert
            Assert.Throws<BadStatusCodeException>(
                () =>
                {
                    sut.ProcessStatusCodes();
                });
        }
    }
}
