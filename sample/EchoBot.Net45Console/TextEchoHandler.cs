using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace EchoBot.Net45Console
{
    public class TextEchoHandler : IUpdateHandler
    {
        private readonly TimeService _timeService;

        public TextEchoHandler(TimeService timeService)
        {
            _timeService = timeService;
        }

        public bool CanHandleUpdate(IBot bot, Update update) => update.Message?.Text != null;

        public async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            string text = $"**ECHO**:\n" +
                          $"```\n{update.Message.Text}```\n\n" +
                          $"`{_timeService.Time}`";

            await bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                text,
                ParseMode.Markdown
            );

            return UpdateHandlingResult.Handled;
        }
    }
}