using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SampleBots.Bots.EchoBot
{
    public class TextMessageEchoer : IUpdateHandler
    {
        public bool CanHandleUpdate(IBot bot, Update update) =>
            update.Message?.Text != null;

        public async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            string replyText = $"You said:\n`{update.Message.Text.Replace("\n", "`\n`")}`";

            await bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                replyText,
                ParseMode.Markdown,
                replyToMessageId: update.Message.MessageId);

            return UpdateHandlingResult.Continue;
        }
    }
}
