using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace Stiche_zaubern
{
    public class CardAnimation
    {
        private readonly Card _card;
        private readonly Canvas _canvas;
        private Image _image;
        private readonly RequestHandler _handler;

        public CardAnimation(Card card, RequestHandler rh)
        {
            _card = card;
            _canvas = DisplayManager.GameCanvas;
            _handler = rh;
        }

        public async Task think(Player player)
        {
            _image = GetImage(GetUnknownCard());
            _canvas.Children.Add(_image);
            Point startCenter = GetElementCenterRelativeToCanvas(player.display.grid);
            SetOnCanvas(startCenter);
            await Task.Delay(GameManager.WAIT_THINKING);
            _canvas.Children.Remove(_image);
        }

        public async Task animateIn(Player player)
        {
            _image = GetImage(_card.getPictureSource());
            _canvas.Children.Add(_image);

            UIElement fromElement = player.display.grid;
            UIElement toElement = DisplayManager.GridGameBoard;

            // Start- und Zielpunkt relativ zum AnimationCanvas (zentriert auf das Element)
            Point startCenter = GetElementCenterRelativeToCanvas(fromElement);
            Point endCenter = GetElementCenterRelativeToCanvas(toElement);

            SetOnCanvas(startCenter);
            await Animate((a, b, c) => 0.0,
                (a, b, c) => (b - a) / 2.0,
                startCenter, endCenter, new Point());
            
        }

        public async Task animateOut(Player player)
        {
            await Task.Delay(GameManager.WAIT_THINKING);
            if (player != null)
            {
                UIElement toElement = player.display.grid;
                UIElement fromElement = DisplayManager.GridGameBoard;

                // Start- und Zielpunkt relativ zum AnimationCanvas (zentriert auf das Element)
                Point startCenter = new Point(Canvas.GetLeft(_image), Canvas.GetTop(_image));
                Point middleCenter = GetElementCenterRelativeToCanvas(fromElement);
                Point endCenter = GetElementCenterRelativeToCanvas(toElement);

                await Animate((a, b, c) => (b - a) / 2.0,
                    (a, b, c) => c - a,
                    startCenter, middleCenter, endCenter);
            }

            // Entferne temporäres Image
            _canvas.Children.Remove(_image);
        }

        public async Task animateJuggle(Player from, Player to)
        {
            _image = GetImage(GetUnknownCard());
            // In AnimationCanvas einfügen
            _canvas.Children.Add(_image);

            UIElement fromElement = from.display.grid;
            UIElement toElement = to.display.grid;

            // Start- und Zielpunkt relativ zum AnimationCanvas (zentriert auf das Element)
            Point startCenter = GetElementCenterRelativeToCanvas(fromElement);
            Point endCenter = GetElementCenterRelativeToCanvas(toElement);
            SetOnCanvas(startCenter);
            
            await Animate((a,b,c)=>0.0,
                (a,b,c) => b-a, 
                startCenter, endCenter, new Point());

            _canvas.Children.Remove(_image);
        }

        private Point GetElementCenterRelativeToCanvas(UIElement element)
        {
            // Punkt (0,0) des Elements in Canvas-Koordinaten
            var transform = element.TransformToVisual(_canvas);
            var topLeft = transform.TransformPoint(new Point(0, 0));

            // Größe des Elements
            double w = (element as FrameworkElement)?.ActualWidth ?? (element as FrameworkElement)?.RenderSize.Width ?? 0;
            double h = (element as FrameworkElement)?.ActualHeight ?? (element as FrameworkElement)?.RenderSize.Height ?? 0;

            return new Point(topLeft.X + w / 2, topLeft.Y + h / 2);
        }

        private Image GetImage(ImageSource imageSource)
        {
            return new Image
            {
                Source = imageSource,
                Width = 60,
                Height = 90,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
        }

        private ImageSource GetUnknownCard()
        {
            return new BitmapImage(new Uri("ms-appx:///Assets/Unknown.jpg"));
        }

        private void SetOnCanvas(Point point)
        {
            Canvas.SetZIndex(_image, 1000);
            Canvas.SetLeft(_image, point.X - _image.Width / 2);
            Canvas.SetTop(_image, point.Y - _image.Height / 2);
        }

        private async Task Animate(Func<double,double,double,double> fromFunc, Func<double, double, double,double> toFunc, Point a, Point b, Point c)
        {
            // TransformGroup: [0]=Rotate, [1]=Translate, [2]=Scale (optional)
            var tg = new TransformGroup();
            var rotate = new RotateTransform();
            var translate = new TranslateTransform();
            var scale = new ScaleTransform { ScaleX = 1.0, ScaleY = 1.0 };
            tg.Children.Add(rotate);
            tg.Children.Add(translate);
            tg.Children.Add(scale);
            _image.RenderTransform = tg;

            // Warte bis Layout gemessen ist
            await Task.Yield();

            var durationMs = _handler.IsToSkip() ? 1 : 800;

            // Animations-Storyboard
            var sb = new Storyboard();
            var duration = new Duration(TimeSpan.FromMilliseconds(durationMs));

            // Translate X/Y animieren (relative Verschiebung)
            var animX = new DoubleAnimation
            {
                From = fromFunc(a.X,b.X,c.X),
                To = toFunc(a.X,b.X,c.X),
                Duration = duration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            var animY = new DoubleAnimation
            {
                From = fromFunc(a.Y,b.Y,c.Y),
                To = toFunc(a.Y, b.Y, c.Y),
                Duration = duration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            // Rotation leicht hinzufügen
            var rotAnim = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = duration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            // Optional: Scale beim Ankommen
            var scaleAnim = new DoubleAnimation
            {
                From = 1.0,
                To = 0.9,
                Duration = new Duration(TimeSpan.FromMilliseconds(durationMs / 2)),
                AutoReverse = true,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(animX, _image);
            Storyboard.SetTarget(animY, _image);
            Storyboard.SetTarget(rotAnim, _image);
            Storyboard.SetTarget(scaleAnim, _image);

            Storyboard.SetTargetProperty(animX, "(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.X)");
            Storyboard.SetTargetProperty(animY, "(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.Y)");
            Storyboard.SetTargetProperty(rotAnim, "(UIElement.RenderTransform).(TransformGroup.Children)[0].(RotateTransform.Angle)");
            Storyboard.SetTargetProperty(scaleAnim, "(UIElement.RenderTransform).(TransformGroup.Children)[2].(ScaleTransform.ScaleX)");

            // Für ScaleY dieselbe Animation wiederverwenden
            var scaleYAnim = new DoubleAnimation
            {
                From = 1.0,
                To = 0.9,
                Duration = new Duration(TimeSpan.FromMilliseconds(durationMs / 2)),
                AutoReverse = true,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(scaleYAnim, _image);
            Storyboard.SetTargetProperty(scaleYAnim, "(UIElement.RenderTransform).(TransformGroup.Children)[2].(ScaleTransform.ScaleY)");

            sb.Children.Add(animX);
            sb.Children.Add(animY);
            sb.Children.Add(rotAnim);
            sb.Children.Add(scaleAnim);
            sb.Children.Add(scaleYAnim);

            var tcs = new TaskCompletionSource<bool>();
            sb.Completed += (s, e) => tcs.TrySetResult(true);
            sb.Begin();

            await tcs.Task;

        }
    }
}
