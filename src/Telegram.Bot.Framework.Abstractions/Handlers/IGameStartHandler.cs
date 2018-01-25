using System.Threading.Tasks;
using Telegram.Bot.Types;

// ReSharper disable once CheckNamespace
namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Update handler for Telegram game updates and game Score requests from game HTML5 page
    /// </summary>
    public interface IGameStartHandler : IUpdateHandler
    {
        /// <summary>
        /// Game's short name
        /// </summary>
        string ShortName { get; }
    }
}