using System;
using NUnit.Framework;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Misc;
using Rhino.Mocks;

namespace Push.Foundation.Tests.Web
{
    [TestFixture]
    public class ThrottlerTests
    {

        [Test]
        [Repeat(3)]
        public void ThrottlerCausesDelayForSameHost()
        {
            // Assemble
            var logger = MockRepository.GenerateStub<IPushLogger>();
            var sut = new Throttler(logger);
            sut.TimeBetweenCallsMs = 1000;
            var testHost = "ThrottlerCausesDelayForSameHost";

            // Act
            sut.Process(testHost);
            var startTime = DateTime.UtcNow;
            sut.Process(testHost);
            var endTime = DateTime.UtcNow;
            
            // Assert
            var actualTimeBetween = (endTime - startTime).TotalMilliseconds;
            Assert.IsTrue(actualTimeBetween > sut.TimeBetweenCallsMs);
        }

    }
}
