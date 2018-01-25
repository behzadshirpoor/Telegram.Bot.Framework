using System;
using System.Collections.Generic;
using System.Linq;

namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Provides a list of update handlers for the bot
    /// </summary>
    /// <typeparam name="TBot">Type of bot</typeparam>
    public class UpdateHanldersProvider<TBot> : IUpdateHandlersProvider<TBot>
        where TBot : class, IBot
    {
        public IEnumerable<IUpdateHandler> UpdateHandlers =>
            _context is null
                ? _builders.Select(f => f())
                : _contextualBuilders.Select(f => f(_context));

        private readonly List<Func<IUpdateHandler>> _builders;

        private readonly object _context;

        private readonly List<Func<object, IUpdateHandler>> _contextualBuilders;

        public UpdateHanldersProvider(params Func<IUpdateHandler>[] builders)
        {
            _builders = new List<Func<IUpdateHandler>>(builders);
        }

        public UpdateHanldersProvider(object context, params Func<object, IUpdateHandler>[] builders)
        {
            _context = context;
            _contextualBuilders = new List<Func<object, IUpdateHandler>>(builders);
        }

        public void AddHandler(Func<IUpdateHandler> builder)
        {
            _builders.Add(builder);
        }

        public void AddHandler(Func<object, IUpdateHandler> builder)
        {
            if (_context is null)
                throw new InvalidOperationException();

            _contextualBuilders.Add(builder);
        }
    }
}
