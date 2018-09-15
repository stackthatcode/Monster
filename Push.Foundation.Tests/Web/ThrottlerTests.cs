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
            var testHost = "ThrottlerCausesDelayForSameHost";

            // Act
            Throttler.Process(testHost, timeBetweenCallsMs:1000);
            var startTime = DateTime.UtcNow;
            Throttler.Process(testHost, timeBetweenCallsMs: 1000);
            var endTime = DateTime.UtcNow;
            
            // Assert
            var actualTimeBetween = (endTime - startTime).TotalMilliseconds;
            Assert.IsTrue(actualTimeBetween > 1000);
        }

    }
}
