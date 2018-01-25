using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;

namespace SampleEchoBot
{
    public class SayHelloHandler : IUpdateHandler
    {
        public SayHelloHandler()
        {
        }

        public bool CanHandleUpdate(IBot bot, Update update) => update.Message?.Text != null;

        public async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            await bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                $"Hello, {update.Message.From.FirstName}"
            );

            return UpdateHandlingResult.Continue;
        }
    }
}