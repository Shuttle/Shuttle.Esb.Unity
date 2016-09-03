namespace Shuttle.Esb.Unity.Tests.Duplicate
{
    public class DuplicateMessageHandler2 : IMessageHandler<DuplicateCommand>
    {
        public void ProcessMessage(IHandlerContext<DuplicateCommand> context)
        {
        }
    }
}