using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Practices.Unity;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Esb.Unity
{
	public class UnityMessageHandlerFactory : MessageHandlerFactory, IRequireInitialization
	{
		private readonly IUnityContainer _container;
		private static readonly Type Generic = typeof (IMessageHandler<>);
		private static readonly InjectionMember[] EmptyInjectionMembers = {};
		private readonly Dictionary<Type, Type> _messageHandlerTypes = new Dictionary<Type, Type>();
		private readonly ILog _log;

		public UnityMessageHandlerFactory(IUnityContainer container)
		{
			Guard.AgainstNull(container, "container");

			_container = container;

			_log = Log.For(this);
		}

		public override IMessageHandler CreateHandler(object message)
		{
			return (IMessageHandler) _container.Resolve(Generic.MakeGenericType(message.GetType()));
		}

		public override IEnumerable<Type> MessageTypesHandled
		{
			get { return _messageHandlerTypes.Keys; }
		}

		public void Initialize(IServiceBus bus)
		{
			Guard.AgainstNull(bus, "bus");

		    _container.RegisterInstance(bus);

			RefreshHandledTypes();
		}

		private void RefreshHandledTypes()
		{
			foreach (var containerRegistration in _container.Registrations)
			{
				if (containerRegistration.RegisteredType.IsGenericType &&
				    containerRegistration.RegisteredType.GetGenericTypeDefinition() == Generic)
				{
					var messageType = containerRegistration.RegisteredType.GetGenericArguments()[0];

					if (_messageHandlerTypes.ContainsKey(messageType))
					{
						return;
					}

					_messageHandlerTypes.Add(messageType, containerRegistration.MappedToType);

					_log.Information(string.Format(EsbResources.MessageHandlerFactoryHandlerRegistered, messageType.FullName,
						containerRegistration.MappedToType.FullName));
				}
			}
		}

		public override IMessageHandlerFactory RegisterHandlers(Assembly assembly)
		{
			foreach (var type in new ReflectionService().GetTypes(typeof (IMessageHandler<>), assembly))
			{
				var handlerInterfaces = type.InterfacesAssignableTo(Generic);

				foreach (var handlerInterface in handlerInterfaces)
				{
					_container.RegisterType(handlerInterface, type, new ContainerControlledLifetimeManager(), EmptyInjectionMembers);
				}
			}

			RefreshHandledTypes();

			return this;
		}
	}
}