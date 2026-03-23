using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stiche_Zaubern_MsgpLib;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Stiche_zaubern
{

    public abstract class GameModeManager
    {
        public async Task begin(GameRound activeRound, RequestHandler requestHandler,NetworkTalkManager talkManager)
        {
            ActiveRound = activeRound;
            this.requestHandler = requestHandler;
            TalkManager = talkManager;
            _players = ActiveRound.GetActivePlayerQueue();
            requestHandler.IsCanceled = false;
            await begin();
            requestHandler.IsCanceled = true;
        }

        protected abstract Task begin();
        // Resume after player's input arg
        public abstract Task resume(Player player, object arg);
        protected abstract object procAITurn(AIPlayer player);
        protected abstract void prepareForActivePlayer();
        protected abstract Task procTurn(Player player, object arg, bool activePlayer = false);

        protected RequestHandler requestHandler { get; private set; }

        protected Queue<Player> _players;
        protected GameRound ActiveRound { get; private set; }
        protected NetworkTalkManager TalkManager { get; private set; }
        protected readonly Player activePlayer;

        protected async Task wait(int time, bool skipable=true, bool reset = true)
        {
            requestHandler.SkipRequest = false;
            requestHandler.IsSkipable = skipable;
            for (int i = 0; i < time; i += GameManager.UPDATE_RATE)
            {
                if (requestHandler.IsToSkip())
                {
                    break;
                }
                await Task.Delay(GameManager.UPDATE_RATE);
            }
            if (reset)
            {
                resetSkipable();
            }
        }

        protected void resetSkipable()
        {
            requestHandler.IsSkipable = false;
            requestHandler.SkipRequest = false;
        }

        protected GameModeManager()
        {
            activePlayer = GameInfo.GetActivePlayer();
            DisplayManager.displayTexts();
        }
    }
    public abstract class GameModeLoopManager : GameModeManager
    {
        protected async Task doTurn(bool activePlayerAlreadyPlayer = false)
            {
            while (_players.Count > 0 && !requestHandler.CancelRequest)
            {
                Player player = _players.Dequeue();
                DisplayManager.getGameTextBlock().Text = "Spieler " + player.name + " ist am Zug.";
                if (player is ActivePlayer && !activePlayerAlreadyPlayer)
                {
                    prepareForActivePlayer();
                    break;
                }
                else if (player is AIPlayer _player)
                {     
                    object arg = procAITurn(_player);
                    await procTurn(player, arg);
                }
                else if (player is RemotePlayer _rplayer)
                {
                    object arg = await _rplayer.Listener.TaskToGetMessage(ActiveRound.RoundMode,player);
                    await procTurn(player, arg);
                }
                else
                {
                    throw new Exception("Unknown instance of player!");
                }
            }
        }

        protected override async Task begin()
        {
            await doTurn();
        }
        public override async Task resume(Player activePlayer, object arg)
        {
            requestHandler.IsCanceled = false;
            foreach (Button button in DisplayManager.getActivePlayerButtons())
            {
                button.IsEnabled = false;
            }

            await procTurn(activePlayer, arg, true);
            await doTurn(true);
            if(!requestHandler.CancelRequest)
            {
                await end();
            }
            requestHandler.IsCanceled = true;
        }
        protected virtual async Task end()
        {
            GameManager.procNewTrick();
        }
    }

    public abstract class GameModeSimpleManager : GameModeManager
    {
        protected override async Task begin()
        {
            requestHandler.IsCanceled = false;
            Player player = _players.Dequeue();

            if (player is ActivePlayer)
            {
                prepareForActivePlayer();
            }
            else if (player is AIPlayer _player)
            {
                object arg = procAITurn(_player);
                await procTurn(player, arg);
            }
            else if (player is RemotePlayer _rplayer)
            {
                object arg = await _rplayer.Listener.TaskToGetMessage(ActiveRound.RoundMode, player);
                await procTurn(player, arg);
            }
            else
            {
                throw new Exception("Unknown instance of player!");
            }
            requestHandler.IsCanceled = true;
        }
    }

    public class TrickingManager : GameModeLoopManager
    {
        private List<CardAnimation> _animations = new List<CardAnimation>();

        protected override async Task end()
        {
            ActiveRound.ProcTrick();
            DisplayManager.displayTexts();
            Player lucky = ActiveRound.ActiveTrick.whoHasWonTrick();

            await wait(GameManager.WAIT_TRICK_SHOW, reset: false);

            List<Task> tasks = new List<Task>();
            _animations.ForEach(a => tasks.Add(a.animateOut(lucky)));

            await Task.WhenAll(tasks);
            _animations.Clear();

            resetSkipable();
            if (!requestHandler.CancelRequest)
            {
                await base.end();
            }
        }

        protected override void prepareForActivePlayer()
        {
            DisplayManager.getGameTextBlock().Text = "Trumpffarbe: " + ActiveRound.TrumpColor;

            PlayerInRound player = ActiveRound.GetPlayerInRound(activePlayer);
            foreach (Button button in DisplayManager.getActivePlayerButtons())
            {
                button.IsEnabled = player.isLegalMove(ActiveRound.ActiveTrick)[(Card)button.Tag];
            }
        }

        protected override object procAITurn(AIPlayer player)
        {
            Trick trick = ActiveRound.ActiveTrick;
            return player.chooseCard(trick);
        }

        protected override async Task procTurn(Player player, object arg, bool activePlayer)
        {
            Trick trick = ActiveRound.ActiveTrick;
            Card card = arg is Card _card ? _card : throw new Exception("Expected a card and got something different.");
            TalkManager.Talk(GamingMessageDecoder.CreateTrickingMessage(player, card));
            trick.addCardToTrick(player, card);
            await displayCardAtTrickBoard(player, card, !activePlayer);
        }

        private async Task displayCardAtTrickBoard(Player player, Card card, bool show)
        {
            var cardAnim = new CardAnimation(card, requestHandler);
            if (show)
            {
                await cardAnim.think(player);
            }
            await cardAnim.animateIn(player);
            _animations.Add(cardAnim);
        }
    }
    public class GuessingManager : GameModeLoopManager
    {
        public override async Task resume(Player activePlayer, object arg)
        {
            requestHandler.IsCanceled = false;

            Button fertig = DisplayManager.getGameBoardButton();
            fertig.Visibility = Visibility.Collapsed;

            ComboBox ComboBoxRaten = DisplayManager.getGameBoardComboBox();
            ComboBoxRaten.Visibility = Visibility.Collapsed;

            await base.resume(activePlayer, arg);
        }

        protected override void prepareForActivePlayer()
        {
            DisplayManager.getGameTextBlock().Visibility = Visibility.Visible;
            DisplayManager.getGameTextBlock().Text = "Trumpffarbe: " + ActiveRound.TrumpColor;

            DisplayManager.displayTexts();

            ComboBox ComboBoxRaten = DisplayManager.getGameBoardComboBox();
            ComboBoxRaten.Items.Clear();
            ComboBoxRaten.Visibility = Visibility.Visible;
            ComboBoxRaten.Header = "Ich erreiche so viele Stiche:";

            Button fertig = DisplayManager.getGameBoardButton();
            fertig.Visibility = Visibility.Visible;

            for (int i = 0; i <= ActiveRound.NumberRound; i++)
            {
                ComboBoxItem item = new ComboBoxItem
                {
                    Content = i,
                    Tag = i,
                    IsEnabled = ActiveRoundInfo.getActiveGuessHelper().isGuessLegal(i)
                };
                ComboBoxRaten.Items.Add(item);
            }
            ComboBoxRaten.SelectedItem = ComboBoxRaten.Items.FirstOrDefault(k => ((ComboBoxItem)k).IsEnabled);
        }

        protected override object procAITurn(AIPlayer player)
        {
            List<int> possibleGuesses = new List<int>();
            for (int i = 0; i <= ActiveRound.NumberRound; i++)
            {
                if (ActiveRoundInfo.getActiveGuessHelper().isGuessLegal(i))
                {
                    possibleGuesses.Add(i);
                }
            }
            return player.guess(possibleGuesses);
        }

        protected override async Task procTurn(Player player, object arg, bool activePlayer = false)
        {
            int guess = arg is int g ? g : throw new Exception("Expected guess, got something different.");
            TalkManager.Talk(GamingMessageDecoder.CreateGuessingMessage(player, guess));
            ActiveRound.ProcGuess(player, guess);
            DisplayManager.displayTexts();
            await wait(GameManager.WAIT_THINKING, false);
        }
    }

    public class JugglingManager : GameModeLoopManager
    {
        protected override async Task end()
        {
            List<Task> animations = new List<Task>();
            _players = ActiveRound.GetActivePlayerQueue();
            Player first_player = _players.Dequeue();
            Player player = first_player;
            Player next_player;
            do
            {
                next_player = _players.Dequeue();
                juggleCard(player, next_player);
                animations.Add(new CardAnimation(null, requestHandler).animateJuggle(player, next_player));
                player = next_player;
            }
            while (_players.Count > 0);

            juggleCard(player, first_player);
            animations.Add(new CardAnimation(null, requestHandler).animateJuggle(player, first_player));
            await Task.WhenAll(animations);
            animations.Clear();

            await base.end();
        }

        private void juggleCard(Player from, Player to)
        {
            Card juggledCard = ActiveRound.GetPlayerInRound(from).chosenJugglingCard;
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

        protected override object procAITurn(AIPlayer player)
        {
            return player.chooseJugglingCard();
        }

        protected override async Task procTurn(Player player, object arg, bool activePlayer = false)
        {
            Card card = arg is Card _card ? _card : throw new Exception("Expected a card, got something different.");
            TalkManager.Talk(GamingMessageDecoder.CreateJugglingMessage(player, card));
            ActiveRound.ProcJuggle(player, card);
        }
    }

    public class TrumpChoosingManager : GameModeSimpleManager
    {

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
                    ComboBoxItem item = new ComboBoxItem
                    {
                        Content = color.ToString(),
                        Tag = color
                    };
                    ComboBoxRaten.Items.Add(item);
                }

            }

            Button fertig = DisplayManager.getGameBoardButton();
            fertig.Visibility = Visibility.Visible;
        }

        public override async Task resume(Player activePlayer, object arg)
        {
            Button fertig = DisplayManager.getGameBoardButton();
            fertig.Visibility = Visibility.Collapsed;
            await procTurn(activePlayer, (CardColor)arg);
        }

        protected override object procAITurn(AIPlayer player)
        {
            return player.chooseTrump();
        }

        /* WARNING ASYNC */
        protected void end()
        {
            GameManager.createGameModeManager<GuessingManager>();
        }

        protected override async Task procTurn(Player player, object arg, bool activePlayer = false)
        {
            TalkManager.Talk(GamingMessageDecoder.CreateChoosingTrumpMessage(player, (CardColor)arg));
            ActiveRound.TrumpColor = (CardColor)arg;
            end();
        }
    }

    public class GameEndingManager : GameModeSimpleManager
    {

        protected override void prepareForActivePlayer()
        {
            throw new NotImplementedException();
        }

        protected override object procAITurn(AIPlayer player)
        {
            throw new NotImplementedException();
        }

        protected override async Task begin()
        {
            requestHandler.IsCanceled = false;

            DisplayManager.displayTexts();
            DisplayManager.getGameTextBlock().Text = "Spielende!";

            int pointsBest = -1000;
            int pointsLast = 1000;
            foreach (Player player in GameInfo.GetPlayers())
            {
                if (player.points > pointsBest)
                {
                    pointsBest = player.points;
                }

                if (player.points < pointsLast)
                {
                    pointsLast = player.points;
                }
            }

            new HighscoreManager().WriteHighscore(activePlayer.name,
                activePlayer.points,
                GameInfo.GetPlacement(activePlayer),
                GameInfo.GetNumPlayers() - 1,
                GameInfo.GetGameType(),
                pointsBest,
                pointsLast
                );

            Button fertig = DisplayManager.getGameBoardButton();
            fertig.Visibility = Visibility.Visible;

            requestHandler.IsCanceled = true;
        }

        public override Task resume(Player player, object arg)
        {
            throw new NotImplementedException();
        }

        protected override Task procTurn(Player player, object arg, bool activePlayer)
        {
            throw new NotImplementedException();
        }
    }
}

