﻿namespace Shuttle.Esb.Unity.Tests
{
    public class SimpleMessageHandler :
        IMessageHandler<SimpleCommand>,
        IMessageHandler<SimpleEvent>
    {
        public void ProcessMessage(IHandlerContext<SimpleCommand> context)
        {
        }

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessMessage(IHandlerContext<SimpleEvent> context)
        {
        }
    }
}