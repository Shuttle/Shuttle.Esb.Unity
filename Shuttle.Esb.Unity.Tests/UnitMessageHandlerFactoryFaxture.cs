using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Shuttle.Esb.Unity.Tests.Duplicate;

namespace Shuttle.Esb.Unity.Tests
{
    [TestFixture]
    public class UnitMessageHandlerFactoryFaxture
    {
        [Test]
        public void Should_be_able_to_find_message_handlers()
        {
            var container = new UnityContainer();

            var factory = new UnityMessageHandlerFactory(container);

            factory.RegisterHandlers(GetType().Assembly);

            Assert.IsTrue(factory.MessageTypesHandled.Contains(typeof(SimpleCommand)));
            Assert.IsTrue(factory.MessageTypesHandled.Contains(typeof(SimpleEvent)));
            Assert.IsNotNull(factory.CreateHandler(new SimpleCommand()));
            Assert.IsNotNull(factory.CreateHandler(new SimpleEvent()));
        }


        [Test]
        public void Should_fail_when_attempting_to_register_duplicate_handlers()
        {
            var container = new UnityContainer();

            var factory = new UnityMessageHandlerFactory(container);

            Assert.Throws<InvalidOperationException>(() => factory.RegisterHandlers(typeof(DuplicateCommand).Assembly));
        }
    }
}
