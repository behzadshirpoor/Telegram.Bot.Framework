using System.Threading.Tasks;
using Telegram.Bot.Types;

// ReSharper disable once CheckNamespace
namespace Telegram.Bot.Framework
{
    public interface IGameHandler : IGameStartHandler
    {
        GameOptions Options { get; }

        Task SetGameScoreAsync(IBot bot, string playerid, int score);

        Task<GameHighScore[]> GetHighestScoresAsync(IBot bot, string playerid);
    }
}