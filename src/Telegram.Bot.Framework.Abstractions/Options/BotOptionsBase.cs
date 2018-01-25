// ReSharper disable once CheckNamespace
namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Configurations for the bot
    /// </summary>
    public abstract class BotOptionsBase
    {
        /// <summary>
        /// Telegram API token
        /// </summary>
        public string ApiToken { get; set; }

        /// <summary>
        /// Array of options for this bot's games
        /// </summary>
        public GameOptionsBase[] GameOptions { get; set; }
    }
}