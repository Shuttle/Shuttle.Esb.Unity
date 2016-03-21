using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Practices.Unity;
using Shuttle.Core.Infrastructure;
using Shuttle.Esb;

namespace Shuttle.Esb.Unity
{
	public class UnityMessageHandlerFactory : MessageHandlerFactory
	{
		private readonly IUnityContainer _container;
		private static readonly Type _generic = typeof (IMessageHandler<>);
		private static readonly InjectionMember[] _emptyInjectionMembers = {};
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
			return (IMessageHandler) _container.Resolve(_generic.MakeGenericType(message.GetType()));
		}

		public override IEnumerable<Type> MessageTypesHandled
		{
			get { return _messageHandlerTypes.Keys; }
		}

		public override void Initialize(IServiceBus bus)
		{
			Guard.AgainstNull(bus, "bus");

			_messageHandlerTypes.Clear();

			RefreshHandledTypes();
		}

		private void RefreshHandledTypes()
		{
			foreach (var containerRegistration in _container.Registrations)
			{
				if (containerRegistration.RegisteredType.IsGenericType &&
				    containerRegistration.RegisteredType.GetGenericTypeDefinition() == _generic)
				{
					var messageType = containerRegistration.RegisteredType.GetGenericArguments()[0];

					if (_messageHandlerTypes.ContainsKey(messageType))
					{
						return;
					}

					_messageHandlerTypes.Add(messageType, containerRegistration.MappedToType);

					_log.Information(string.Format(ESBResources.MessageHandlerFactoryHandlerRegistered, messageType.FullName, containerRegistration.MappedToType.FullName));
				}
			}
		}

		public UnityMessageHandlerFactory RegisterHandlers()
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				RegisterHandlers(assembly);
			}

			return this;
		}

		public UnityMessageHandlerFactory RegisterHandlers(Assembly assembly)
		{
			foreach (var type in new ReflectionService().GetTypes(typeof (IMessageHandler<>), assembly))
			{
				var handlerInterfaces = type.InterfacesAssignableTo(_generic);

				foreach (var handlerInterface in handlerInterfaces)
				{
					_container.RegisterType(handlerInterface, type, new ContainerControlledLifetimeManager(), _emptyInjectionMembers);
				}
			}

			RefreshHandledTypes();

			return this;
		}
	}
}