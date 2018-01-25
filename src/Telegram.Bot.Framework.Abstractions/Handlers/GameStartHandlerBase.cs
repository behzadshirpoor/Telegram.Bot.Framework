using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

// ReSharper disable once CheckNamespace
namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Base class for bot's games
    /// </summary>
    public abstract class GameStartHandlerBase : IGameStartHandler
    {
        /// <summary>
        /// Game's short name
        /// </summary>
        public string ShortName { get; }

        protected string Url;

        protected GameStartHandlerBase(string shortName, string url)
        {
            ShortName = shortName;
            Url = url;
        }

        public bool CanHandleUpdate(IBot bot, Update update)
        {
            return update.CallbackQuery?.GameShortName?
                       .Equals(ShortName, StringComparison.OrdinalIgnoreCase) ==
                   true;
        }

        public virtual async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            await bot.Client.AnswerCallbackQueryAsync(update.CallbackQuery.Id, url: Url);
            return UpdateHandlingResult.Handled;
        }
    }
}