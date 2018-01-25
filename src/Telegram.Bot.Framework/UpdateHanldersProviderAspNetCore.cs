using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Provides a list of update handlers for the bot
    /// </summary>
    /// <typeparam name="TBot">Type of bot</typeparam>
    public class UpdateHanldersProviderAspNetCore<TBot> : IUpdateHandlersProvider<TBot>
        where TBot : class, IBot
    {
        public IEnumerable<IUpdateHandler> UpdateHandlers =>
            _types
                .Select(t => _serviceProvider.GetRequiredService(t) as IUpdateHandler);

        private readonly IServiceProvider _serviceProvider;

        private readonly IEnumerable<Type> _types;

        public UpdateHanldersProviderAspNetCore(IServiceProvider serviceProvider, IEnumerable<Type> types)
        {
            _serviceProvider = serviceProvider;
            _types = types;
        }

        public void AddHandler(Func<IUpdateHandler> builder)
        {
            throw new NotImplementedException();
        }
    }
}
