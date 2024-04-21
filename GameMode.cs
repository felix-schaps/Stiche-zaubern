using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Stiche_zaubern
{
    public enum RoundMode { CHOOSING_TRUMP, GUESSING, TRICKING, JUGGLING, END }

    public abstract class GameModeManager
    {
        public abstract void begin();
        public abstract Task resume();
        protected abstract void procAITurn(AIPlayer player);
        protected abstract void prepareForActivePlayer();
        protected abstract Task end();

        protected Queue<Player> _players;

        protected GameModeManager()
        {
            _players = Game.getActiveRound().getActivePlayerQueue();
            DisplayManager.displayTexte();
        }
    }
    public abstract class GameModeLoopManager : GameModeManager
    {
        public override void begin()
        {
            while (_players.Count > 0)
            {
                Player player = _players.Dequeue();

                if (player is ActivePlayer)
                {
                    prepareForActivePlayer();
                    return;
                }
                else if (player is AIPlayer)
                {
                    procAITurn((AIPlayer)player);
                }
                else
                {
                    throw new Exception("Unknown instance of player!");
                }
            }
        }
        public override async Task resume()
        {
            while (_players.Count > 0)
            {
                Player player = _players.Dequeue();
                if (player is AIPlayer)
                {
                    procAITurn((AIPlayer)player);
                }
                else
                {
                    throw new Exception("Unknown instance of player!");
                }
            }
            await end();
        }
        protected override async Task end()
        {
            await Task.Run(() => CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => GameManager.procNewTrick()));
        }
    }

    public abstract class GameModeSimpleManager : GameModeManager
    {
        public override void begin()
        {
            Player player = _players.Dequeue();

            if (player is ActivePlayer)
            {
                prepareForActivePlayer();
                return;
            }
            else if (player is AIPlayer)
            {
                procAITurn((AIPlayer)player);
            }
            else
            {
                throw new Exception("Unknown instance of player!");
            }
        }
        public override async Task resume()
        {
            await end();
        }
    }

    public class TrickingManager : GameModeLoopManager
    {
        protected override async Task end()
        {
            Game.getActiveRound().procTrick();
            DisplayManager.displayTexte();

            await Task.Delay(5000);


            foreach (Player player in Game.getPlayers())
            {
                string name = player.display.grid.Name;
                name = name.Replace("grid_", "stich_");
                Image image = DisplayManager.GridGameBoard.Children.OfType<Image>().First(x => x.Name == name);
                image.Visibility = Visibility.Collapsed;
            }

            await base.end();
        }
        public override async Task resume()
        {
            foreach (Button button in DisplayManager.getActivePlayerButtons())
            {
                button.IsEnabled = false;
            }
            await base.resume();
        }

        protected override void prepareForActivePlayer()
        {
            DisplayManager.getGameTextBlock().Text = "Trumpffarbe: " + Game.getActiveRound().TrumpColor;

            PlayerInRound player = Game.getActiveRound().getPlayerInRound(Game.getActivePlayer());
            foreach (Button button in DisplayManager.getActivePlayerButtons())
            {
                button.IsEnabled = player.isLegalMove(Game.getActiveRound().ActiveTrick)[(Card)button.Tag];
            }
        }

        protected override void procAITurn(AIPlayer player)
        {
            Trick trick = Game.getActiveRound().ActiveTrick;
            trick.addCardToTrick(player, player.chooseCard(trick));
        }
    }
    public class GuessingManager : GameModeLoopManager
    {
        public override async Task resume()
        {
            Button fertig = DisplayManager.getGameBoardButton();
            fertig.Visibility = Visibility.Collapsed;

            ComboBox ComboBoxRaten = DisplayManager.getGameBoardComboBox();
            ComboBoxRaten.Visibility = Visibility.Collapsed;

            Game.getActiveRound().procGuess(Game.getActivePlayer(), (int)((ComboBoxItem)(DisplayManager.getGameBoardComboBox().SelectedItem)).Tag);

            await base.resume();
        }

        protected override void prepareForActivePlayer()
        {
            DisplayManager.getGameTextBlock().Visibility = Visibility.Visible;
            DisplayManager.getGameTextBlock().Text = "Trumpffarbe: " + Game.getActiveRound().TrumpColor;

            DisplayManager.displayTexte();

            ComboBox ComboBoxRaten = DisplayManager.getGameBoardComboBox();
            ComboBoxRaten.Items.Clear();
            ComboBoxRaten.Visibility = Visibility.Visible;
            ComboBoxRaten.Header = "Ich erreiche so viele Stiche:";

            Button fertig = DisplayManager.getGameBoardButton();
            fertig.Visibility = Visibility.Visible;

            for (int i = 0; i <= Game.getActiveRound().NumberRound; i++)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = i;
                item.Tag = i;
                item.IsEnabled = Game.getActiveGuessHelper().isGuessLegal(i);
                ComboBoxRaten.Items.Add(item);
            }
            ComboBoxRaten.SelectedItem = ComboBoxRaten.Items.FirstOrDefault(k => ((ComboBoxItem)k).IsEnabled == true);
        }

        protected override void procAITurn(AIPlayer player)
        {
            List<int> possibleGuesses = new List<int>();
            for (int i = 0; i <= Game.getActiveRound().NumberRound; i++)
            {
                if (Game.getActiveGuessHelper().isGuessLegal(i))
                    possibleGuesses.Add(i);
            }
            int guess = player.guess(possibleGuesses);
            Game.getActiveRound().procGuess(player, guess);
        }
    }

    public class JugglingManager : GameModeLoopManager
    {
        protected override async Task end()
        {
            _players = Game.getActiveRound().getActivePlayerQueue();
            Player first_player = _players.Dequeue();
            Player player = first_player;
            Player next_player;
            do
            {
                next_player = _players.Dequeue();
                juggleCard(player, next_player);
                player = next_player;
            }
            while (_players.Count > 0);

            juggleCard(player, first_player);

            await base.end();
        }

        private void juggleCard(Player from, Player to)
        {
            Card juggledCard = Game.getActiveRound().getPlayerInRound(from).chosenJugglingCard;
            to.giveJugglingCard(juggledCard);
            to.display.displayGivenCards(GenericHelper<Card>.makeSet(juggledCard));
        }

        protected override void prepareForActivePlayer()
        {
            DisplayManager.getGameTextBlock().Text = "Zeit zu jonglieren!";
            foreach (Button button in DisplayManager.getActivePlayerButtons())
            {
                button.IsEnabled = true;
            }
        }

        protected override void procAITurn(AIPlayer player)
        {
            Game.getActiveRound().procJuggle(player, player.chooseJugglingCard());
        }
    }

    public class TrumpChoosingManager : GameModeSimpleManager
    {
        public void procTrumpChosen(CardColor color)
        {
            Game.getActiveRound().TrumpColor = color;
        }

        protected override void prepareForActivePlayer()
        {
            DisplayManager.getGameTextBlock().Visibility = Visibility.Collapsed;

            ComboBox ComboBoxRaten = DisplayManager.getGameBoardComboBox();
            ComboBoxRaten.Items.Clear();
            ComboBoxRaten.Visibility = Visibility.Visible;
            ComboBoxRaten.Header = "Wir spielen folgende Trumpffarbe:";

            foreach (CardColor color in Enum.GetValues(typeof(CardColor)))
            {
                if (color != CardColor.SPECIAL)
                {
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = color.ToString();
                    item.Tag = color;
                    ComboBoxRaten.Items.Add(item);
                }

            }

            Button fertig = DisplayManager.getGameBoardButton();
            fertig.Visibility = Visibility.Visible;
        }

        public override async Task resume()
        {
            Button fertig = DisplayManager.getGameBoardButton();
            fertig.Visibility = Visibility.Collapsed;
            procTrumpChosen((CardColor)((ComboBoxItem)(DisplayManager.getGameBoardComboBox().SelectedItem)).Tag);
            await base.resume();
        }

        protected override void procAITurn(AIPlayer player)
        {
            procTrumpChosen(player.chooseTrump());
            end();
        }

        protected override async Task end()
        {
            GameManager.createGameModeManager<GuessingManager>();
        }
    }

    public class GameEndingManager : GameModeSimpleManager
    {
        protected override async Task end()
        {
            throw new NotImplementedException();
        }

        protected override void prepareForActivePlayer()
        {
            throw new NotImplementedException();
        }

        protected override void procAITurn(AIPlayer player)
        {
            throw new NotImplementedException();
        }

        public override void begin()
        {
            DisplayManager.displayTexte();
            DisplayManager.getGameTextBlock().Text = "Spielende!";

            int pointsBest = -1000;
            int pointsLast = 1000;
            foreach (Player player in Game.getPlayers())
            {
                if (player.points > pointsBest)
                    pointsBest = player.points;
                if (player.points < pointsLast)
                    pointsLast = player.points;
            }

            new HighscoreCsv().WriteHighscore(Game.getActivePlayer().name,
                Game.getActivePlayer().points,
                Game.getPlacement(Game.getActivePlayer()),
                Game.getNumPlayers() - 1,
                "Computergegner",
                pointsBest,
                pointsLast
                );

            Button fertig = DisplayManager.getGameBoardButton();
            fertig.Visibility = Visibility.Visible;
        }
    }
}

