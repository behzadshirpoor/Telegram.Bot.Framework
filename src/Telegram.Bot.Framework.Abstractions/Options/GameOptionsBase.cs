// ReSharper disable once CheckNamespace
namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Telegram game options
    /// </summary>
    public class GameOptionsBase
    {
        /// <summary>
        /// Game's short name
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Url to the game's HTML5 page
        /// </summary>
        public string Url { get; set; }
    }
}