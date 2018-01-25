using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;

namespace EchoBot.Net45Console
{
    public class SayHelloHandler : IUpdateHandler
    {
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