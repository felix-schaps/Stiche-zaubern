using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stiche_zaubern
{
    public class GameManager
    {
        public static GameManager Instance { get; private set; }

        public Task CurrentTask { get; private set; }

        private GameModeManager _gameModeManager;


        public static void createGameModeManager<T>() where T : GameModeManager, new()
        {
            Instance._gameModeManager = new T();
            Instance._gameModeManager.begin();
        }

        public static void procInput()
        {
            Instance.CurrentTask = Instance.procTask();
        }

        private async Task procTask()
        {
            await _gameModeManager.resume();
        }

        public static void procNewRound()
        {
            if (!Game.beginNewRound())
            {
                createGameModeManager<GameEndingManager>();
                return;
            }

            if (Game.getActiveRound().isChoosableTrumpf())
            {
                createGameModeManager<TrumpChoosingManager>();
                return;
            }

            createGameModeManager<GuessingManager>();
        }

        public static void procNewTrick()
        {
            if (!Game.getActiveRound().beginTrick())
            {
                if (Game.getActiveRound().RoundMode == RoundMode.JUGGLING)
                {
                    createGameModeManager<JugglingManager>();
                    return;
                }
                else
                {
                    procNewRound();
                    return;
                }
            }
            createGameModeManager<TrickingManager>();
        }
        public GameManager()
        {
            if (Instance != null)
            {
                Dispose();
            }
            Instance = this;
            procNewRound();
        }
        public void Dispose()
        {
            Instance = null;
        }


    }

    public class GenericHelper<T>
    {
        public static SortedSet<T> makeSet(params T[] values)
        {
            return new SortedSet<T>(values.ToHashSet());
        }
    }
}