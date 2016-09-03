namespace Shuttle.Esb.Unity.Tests.Duplicate
{
    public class DuplicateMessageHandler1 : IMessageHandler<DuplicateCommand>
    {
        public void ProcessMessage(IHandlerContext<DuplicateCommand> context)
        {
        }
    }
}