using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;

namespace SampleBots.Bots.GreeterBot
{
    public class PhotoForwarder : IUpdateHandler
    {
        public bool CanHandleUpdate(IBot bot, Update update) => update.Message?.Photo != null;

        public async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            await bot.Client.ForwardMessageAsync(update.Message.Chat.Id,
                update.Message.Chat.Id,
                update.Message.MessageId);

            return UpdateHandlingResult.Handled;
        }
    }
}
