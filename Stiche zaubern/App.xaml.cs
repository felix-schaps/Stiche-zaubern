using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Gaming.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Stiche_zaubern
{
    /// <summary>
    /// Stellt das anwendungsspezifische Verhalten bereit, um die Standardanwendungsklasse zu ergänzen.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initialisiert das Singletonanwendungsobjekt. Dies ist die erste Zeile von erstelltem Code
        /// und daher das logische Äquivalent von main() bzw. WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Anwendung durch den Endbenutzer normal gestartet wird. Weitere Einstiegspunkte
        /// werden z. B. verwendet, wenn die Anwendung gestartet wird, um eine bestimmte Datei zu öffnen.
        /// </summary>
        /// <param name="e">Details über Startanforderung und -prozess.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // App-Initialisierung nicht wiederholen, wenn das Fenster bereits Inhalte enthält.
            // Nur sicherstellen, dass das Fenster aktiv ist.
            if (!(Window.Current.Content is Frame rootFrame))
            {
                // Frame erstellen, der als Navigationskontext fungiert und zum Parameter der ersten Seite navigieren
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Zustand von zuvor angehaltener Anwendung laden
                }

                // Den Frame im aktuellen Fenster platzieren
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // Wenn der Navigationsstapel nicht wiederhergestellt wird, zur ersten Seite navigieren
                    // und die neue Seite konfigurieren, indem die erforderlichen Informationen als Navigationsparameter
                    // übergeben werden
                    _ = rootFrame.Navigate(typeof(MainMenuPage), e.Arguments);
                }
                // Sicherstellen, dass das aktuelle Fenster aktiv ist
                Window.Current.Activate();
            }

            Window.Current.CoreWindow.KeyDown += OnKeyDown;
            Gamepad.GamepadAdded += OnGamepadAdded;
            Gamepad.GamepadRemoved += OnGamepadRemoved;
        }

        private Gamepad gamepad = null;
        private GamepadReading previousReading;

        private void OnGamepadAdded(object sender, Gamepad e)
        {
            gamepad = e;
            GamepadInputHandler();
        }

        private void OnGamepadRemoved(object sender, Gamepad e)
        {
            gamepad = null;
        }

        private async void GamepadInputHandler()
        {
            while(gamepad != null)
            {
                GamepadReading currentReading = gamepad.GetCurrentReading();

                if (!GameManager.IsInitialized())
                { await Task.Delay(1000); continue; }

                
                if (IsButtonPressed(GamepadButtons.B, previousReading, currentReading))
                {
                    GameManager.cancelGame();
                }
                if (IsButtonPressed(GamepadButtons.Y, previousReading, currentReading))
                {
                    GameManager.skipAnimation();
                }
                await Task.Delay(GameManager.UPDATE_RATE);
            }
        }

        private bool IsButtonPressed(GamepadButtons button, GamepadReading previous, GamepadReading current)
        {
            // Überprüft, ob die Taste im aktuellen Zustand gedrückt ist, aber nicht im vorherigen
            return ((current.Buttons & button) == button) && ((previous.Buttons & button) != button);
        }

        private void OnKeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (!GameManager.IsInitialized())
            {  return; }

            switch (args.VirtualKey)
            {
                case Windows.System.VirtualKey.S:
                    args.Handled = true;
                    GameManager.skipAnimation();
                    break;
                case Windows.System.VirtualKey.Escape:
                    args.Handled = true;
                    DisplayManager.GridGameBoard.Visibility = Visibility.Collapsed;
                    DisplayManager.getGameTextBlock().Text = "Spiel wird beendet.";
                    GameManager.cancelGame();
                    _ = Window.Current.Content is Frame frame ? frame.Navigate(typeof(MainMenuPage)) : throw new Exception("Cannot navigate");
                    break;
                default:
                    break;
            }
            
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Navigation auf eine bestimmte Seite fehlschlägt
        /// </summary>
        /// <param name="sender">Der Rahmen, bei dem die Navigation fehlgeschlagen ist</param>
        /// <param name="e">Details über den Navigationsfehler</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Ausführung der Anwendung angehalten wird.  Der Anwendungszustand wird gespeichert,
        /// ohne zu wissen, ob die Anwendung beendet oder fortgesetzt wird und die Speicherinhalte dabei
        /// unbeschädigt bleiben.
        /// </summary>
        /// <param name="sender">Die Quelle der Anhalteanforderung.</param>
        /// <param name="e">Details zur Anhalteanforderung.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Anwendungszustand speichern und alle Hintergrundaktivitäten beenden
            deferral.Complete();
        }
    }
}
