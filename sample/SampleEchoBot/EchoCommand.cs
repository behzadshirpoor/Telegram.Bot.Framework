using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;

namespace SampleEchoBot
{
    public class EchoCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }

        public string ArgsInput { get; set; }
    }

    public class EchoCommand : CommandBase<EchoCommandArgs>
    {
        public EchoCommand() : base(name: "echo")
        {
        }

        public override async Task<UpdateHandlingResult> HandleCommand(IBot bot, Update update, EchoCommandArgs args)
        {
            string replyText = string.IsNullOrWhiteSpace(args.ArgsInput) ? "Echo What?" : args.ArgsInput;

            await bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                replyText,
                replyToMessageId: update.Message.MessageId);

            return UpdateHandlingResult.Handled;
        }
    }
}
