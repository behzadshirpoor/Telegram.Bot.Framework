using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram.Bot.Framework.Tests.Helpers
{
    public class TestCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }

        public string ArgsInput { get; set; }
    }

    public class TestCommand : CommandBase<TestCommandArgs>
    {
        public TestCommand(string name) : base(name)
        {
        }

        public override Task<UpdateHandlingResult> HandleCommand(IBot bot, Update update, TestCommandArgs args)
        {
            throw new System.NotImplementedException();
        }
    }
}