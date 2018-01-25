// ReSharper disable once CheckNamespace
namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Telegram game options
    /// </summary>
    public class GameOptions : GameOptionsBase
    {
        /// <summary>
        /// Game's callback url for getting or setting high scores
        /// </summary>
        public string ScoresUrl { get; set; }
    }
}