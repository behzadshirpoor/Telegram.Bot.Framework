using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;

namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Manages bot and sends updates to handlers
    /// </summary>
    /// <typeparam name="TBot">Type of bot</typeparam>
    public interface IUpdateManager<TBot>
        where TBot : class, IBot
    {
        Task InitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Handle the update
        /// </summary>
        /// <param name="update">Update to be handled</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task HandleUpdateAsync(Update update, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets bot updates from Telegram if any and passes them to handlers
        /// </summary>
        /// <returns></returns>
        Task GetHandleUpdatesAsync(CancellationToken cancellationToken = default);

        Task GetHandleUpdatesAsync(GetUpdatesRequest request, CancellationToken cancellationToken = default);
    }
}
