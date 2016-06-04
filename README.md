Shuttle.Esb.Unity
=================

`UnityContainer` implementation of the `IMessageHandlerFactory` for use with Shuttle.Esb.

# UnityMessageHandlerFactory

The `UnityMessageHandlerFactory` inherits from the abstract `MessageHandlerFactory` class in order to implement the `IMessageHandlerFactory` interface.  This class will provide the message handlers from the `UnityContainer`.

~~~c#
	bus = ServiceBus
		.Create
		(
			c => c.MessageHandlerFactory(new UnityMessageHandlerFactory(new UnityContainer()))
		)
		.Start();
~~~

## Note on dependency injection

**Note**: *This applies only to version prior to v6.1.1*.  From v6.1.1 this registration takes place automatically.

The `DefaultMessageHandlerFactory` registers all `IMessageHandler<>` implementations in the current `AppDomain`.  As soon as you use a container this responsibility falls on the implementer.

The message distribution makes use of `IMessageHandler<>` implementations in the core and there may be one or more modules, if used, that have message handlers.

You can use the `RegisterHandlers` method of the `UnityMessageHandlerFactory` instance to perform this registration for you:

~~~c#
	bus = ServiceBus
		.Create.Create
		(
			c => c.MessageHandlerFactory(new UnityMessageHandlerFactory(new WindsorContainer()).RegisterHandlers())
		)
		.Start();
~~~

You can also pass a specific `Assembly` to the `RegisterHandlers` method to register only message handlers in the specified assembly.
