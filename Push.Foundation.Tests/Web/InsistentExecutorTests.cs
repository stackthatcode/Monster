using System;
using Push.Foundation.Utilities.Logging;
using Push.Foundation.Web.Misc;
using NUnit.Framework;
using Rhino.Mocks;

namespace Push.Foundation.Tests.Web
{
    [TestFixture]
    public class InsistentExecutorTests
    {
        public class MockClass
        {
            public virtual bool DoSomething()
            {
                return true;
            }
        }
        
        [Test]
        public void ExecutesOnceOnSuccess()
        {
            // Assemble
            var context = new DurableExecContext()
            {
                NumberOfAttempts = 3
            };

            var mockClass = MockRepository.GenerateMock<MockClass>();
            mockClass.Expect(x => x.DoSomething()).Return(true);

            // Act
            var result = 
                DurableExec.Do(() => mockClass.DoSomething(), context);

            // Assert
            Assert.IsTrue(result);
            mockClass.VerifyAllExpectations();
        }

        [Test]
        public void ExecutesTwiceOnFailSuccess()
        {
            // Assemble
            var context = new DurableExecContext()
            {
                NumberOfAttempts = 3
            };

            var mockClass = MockRepository.GenerateMock<MockClass>();
            mockClass
                .Expect(x => x.DoSomething())
                .Throw(new Exception("Aha! Fail!"))
                .Repeat.Once();

            mockClass
                .Expect(x => x.DoSomething())
                .Return(true);

            // Act
            var result =
                DurableExec.Do(() => mockClass.DoSomething(), context);

            // Assert
            Assert.IsTrue(result);
            mockClass.VerifyAllExpectations();
        }
        
        [Test]
        public void ExecutesThriceOnFailFailSuccess()
        {
            // Assemble
            var context = new DurableExecContext()
            {
                NumberOfAttempts = 3
            };

            var mockClass = MockRepository.GenerateMock<MockClass>();
            mockClass
                .Expect(x => x.DoSomething())
                .Throw(new Exception("Aha! Fail!"))
                .Repeat.Times(2);

            mockClass
                .Expect(x => x.DoSomething())
                .Return(true);

            // Act
            var result =
                DurableExec.Do(() => mockClass.DoSomething(), context);

            // Assert
            Assert.IsTrue(result);
            mockClass.VerifyAllExpectations();
        }

        [Test]
        public void ExecutesThriceOnFailFailFailAndThrowException()
        {
            // Assemble
            var context = new DurableExecContext()
            {
                NumberOfAttempts = 3
            };

            var mockClass = MockRepository.GenerateMock<MockClass>();
            mockClass
                .Expect(x => x.DoSomething())
                .Throw(new Exception("Aha! Fail!"))
                .Repeat.Times(3);

            // Act
            Assert.Throws<Exception>(
                () =>
                {
                    var result =
                        DurableExec.Do(() => mockClass.DoSomething(), context);
                });
        }
    }
}
