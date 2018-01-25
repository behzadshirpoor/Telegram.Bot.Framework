using System;
using System.Collections.Generic;

namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Provides a list of update handlers for the bot
    /// </summary>
    /// <typeparam name="TBot">Type of bot</typeparam>
    public interface IUpdateHandlersProvider<TBot>
        where TBot : class, IBot
    {
        /// <summary>
        /// Gets a list of update handlers for the bot
        /// </summary>
        IEnumerable<IUpdateHandler> UpdateHandlers { get; }

        void AddHandler(Func<IUpdateHandler> builder);
    }
}
