using Telegram.Bot.Types;

namespace Telegram.Bot.Framework
{
    /// <summary>
    /// A wrapper around TelegramBot class. Used to make calls to the Bot API
    /// </summary>
    public interface IBot
    {
        User User { get; }

        BotOptionsBase Options { get; }

        /// <summary>
        /// Instance of Telegram bot client
        /// </summary>
        ITelegramBotClient Client { get; }
    }
}
