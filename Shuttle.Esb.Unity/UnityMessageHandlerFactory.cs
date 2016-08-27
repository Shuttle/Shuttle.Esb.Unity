using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.Unity;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb.Unity
{
    public class UnityMessageHandlerFactory : MessageHandlerFactory, IRequireInitialization
    {
        private readonly IUnityContainer _container;
        private static readonly Type MessageHandlerType = typeof(IMessageHandler<>);
        private readonly ILog _log;
        private readonly Dictionary<Type, Type> _messageHandlerTypes = new Dictionary<Type, Type>();
        private readonly ReflectionService _reflectionService = new ReflectionService();
        private static readonly InjectionMember[] EmptyInjectionMembers = { };

        public UnityMessageHandlerFactory(IUnityContainer container)
        {
            Guard.AgainstNull(container, "container");

            _container = container;

            _log = Log.For(this);
        }

        public override IMessageHandler CreateHandler(object message)
        {
            return (IMessageHandler)_container.Resolve(MessageHandlerType.MakeGenericType(message.GetType()));
        }

        public override IEnumerable<Type> MessageTypesHandled
        {
            get { return _messageHandlerTypes.Keys; }
        }

        public void Initialize(IServiceBus bus)
        {
            Guard.AgainstNull(bus, "bus");

            if (_container.Registrations.FirstOrDefault(item => item.RegisteredType == typeof(IServiceBus)) == null) {
                _container.RegisterInstance(bus);
            }
        }

        public override IMessageHandlerFactory RegisterHandlers(Assembly assembly)
        {
            try
            {
                foreach (var type in _reflectionService.GetTypes(MessageHandlerType, assembly))
                {
                    foreach (var @interface in type.GetInterfaces())
                    {
                        if (!@interface.IsAssignableTo(MessageHandlerType))
                        {
                            continue;
                        }

                        var messageType = @interface.GetGenericArguments()[0];

                        if (!_messageHandlerTypes.ContainsKey(messageType))
                        {
                            _messageHandlerTypes.Add(messageType, type);
                        }
                        else
                        {
                            _log.Warning(string.Format(UnityResources.DuplicateMessageHandlerIgnored, _messageHandlerTypes[messageType].FullName, messageType.FullName, type.FullName));
                        }

                        _container.RegisterType(MessageHandlerType.MakeGenericType(messageType), type, new ContainerControlledLifetimeManager(), EmptyInjectionMembers);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warning(string.Format(EsbResources.RegisterHandlersException, assembly.FullName,
                    ex.AllMessages()));
            }

            return this;
        }
    }
}