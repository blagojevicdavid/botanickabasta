using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BotanickaBasta
{
    /// <summary>
    /// Zoom/pan kontrola (točkić + drag) sa min/max zumom i ogradama.
    /// Koristi se kao kontejner:
    ///   <local:ZoomPanBehavior MinScale="0.6" MaxScale="4" FitFactor="1.0" AutoFitOnResize="True">
    ///     <Canvas Width="800" Height="600"> ... </Canvas>
    ///   </local:ZoomPanBehavior>
    /// </summary>
    public class ZoomPanBehavior : Border
    {
        // ----- Konfigurisana svojstva (koristiš iz XAML-a) -----

        public double MinScale
        {
            get => (double)GetValue(MinScaleProperty);
            set => SetValue(MinScaleProperty, value);
        }
        public static readonly DependencyProperty MinScaleProperty =
            DependencyProperty.Register(nameof(MinScale), typeof(double), typeof(ZoomPanBehavior),
                new PropertyMetadata(0.6));

        public double MaxScale
        {
            get => (double)GetValue(MaxScaleProperty);
            set => SetValue(MaxScaleProperty, value);
        }
        public static readonly DependencyProperty MaxScaleProperty =
            DependencyProperty.Register(nameof(MaxScale), typeof(double), typeof(ZoomPanBehavior),
                new PropertyMetadata(4.0));

        public double ZoomStep
        {
            get => (double)GetValue(ZoomStepProperty);
            set => SetValue(ZoomStepProperty, value);
        }
        public static readonly DependencyProperty ZoomStepProperty =
            DependencyProperty.Register(nameof(ZoomStep), typeof(double), typeof(ZoomPanBehavior),
                new PropertyMetadata(1.1)); // faktor po zubu točkića

        public double EdgeMargin
        {
            get => (double)GetValue(EdgeMarginProperty);
            set => SetValue(EdgeMarginProperty, value);
        }
        public static readonly DependencyProperty EdgeMarginProperty =
            DependencyProperty.Register(nameof(EdgeMargin), typeof(double), typeof(ZoomPanBehavior),
                new PropertyMetadata(0.0)); // “jastuk” na ivicama; 0 = nema “vazduha”

        /// <summary>
        /// Ako je True: pri svakom resajzu prozora/kolone radi fit+center (po FitFactor).
        /// Ako je False: zadržava trenutni zoom; centriranje samo kada je sadržaj manji od viewport-a.
        /// </summary>
        public bool AutoFitOnResize
        {
            get => (bool)GetValue(AutoFitOnResizeProperty);
            set => SetValue(AutoFitOnResizeProperty, value);
        }
        public static readonly DependencyProperty AutoFitOnResizeProperty =
            DependencyProperty.Register(nameof(AutoFitOnResize), typeof(bool), typeof(ZoomPanBehavior),
                new PropertyMetadata(true));

        /// <summary>
        /// Množilac fit skale (1.0 = tačan fit; 0.95 = malo “više u prozoru”; 1.2 = malo uvećano).
        /// </summary>
        public double FitFactor
        {
            get => (double)GetValue(FitFactorProperty);
            set => SetValue(FitFactorProperty, value);
        }
        public static readonly DependencyProperty FitFactorProperty =
            DependencyProperty.Register(nameof(FitFactor), typeof(double), typeof(ZoomPanBehavior),
                new PropertyMetadata(1.0));

        // ----- Interno stanje -----
        private FrameworkElement? _content;
        private ScaleTransform? _scale;
        private TranslateTransform? _translate;

        private bool _panning;
        private Point _panStart;

        public ZoomPanBehavior()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            Background = Brushes.Transparent;  // prima mouse evente
            ClipToBounds = true;               // ne crtaj van kolone
            SnapsToDevicePixels = true;        // stabilnije poravnanje
            UseLayoutRounding = true;          // izbegni polu-piksele
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            _content = Child as FrameworkElement;
            if (_content == null) return;

            EnsureTransforms();

            // reaguji i na promenu dimenzija sadržaja (ako ikada zatreba)
            _content.SizeChanged += Content_SizeChanged;

            // početno stanje: fit + center po FitFactor
            Fit(FitFactor);
        }

        private void OnUnloaded(object? sender, RoutedEventArgs e)
        {
            if (_content != null)
                _content.SizeChanged -= Content_SizeChanged;
        }

        private void Content_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (AutoFitOnResize)
                Fit(FitFactor);
            else
                FitAndClamp(); // uskladi min-fit, pa ogradi
        }

        private void EnsureTransforms()
        {
            if (_content == null) return;

            if (_content.RenderTransform is not TransformGroup tg)
            {
                tg = new TransformGroup();
                _content.RenderTransform = tg;
            }

            _scale = tg.Children.OfType<ScaleTransform>().FirstOrDefault();
            if (_scale == null)
            {
                _scale = new ScaleTransform(1, 1);
                tg.Children.Add(_scale);
            }

            _translate = tg.Children.OfType<TranslateTransform>().FirstOrDefault();
            if (_translate == null)
            {
                _translate = new TranslateTransform(0, 0);
                tg.Children.Add(_translate);
            }
        }

        // ----- Miš (zoom/pan) -----

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_content == null || _scale == null || _translate == null) return;

            double fitMin = ComputeFitScale();
            double effMin = MinScale > fitMin ? MinScale : fitMin;

            double oldScale = _scale.ScaleX;
            double delta = e.Delta > 0 ? ZoomStep : (1.0 / ZoomStep);
            double newScale = Clamp(oldScale * delta, effMin, MaxScale);

            // zoom oko kursora (koordinate: viewport = ovaj Border)
            Point m = e.GetPosition(this);
            _translate.X = m.X - (newScale / oldScale) * (m.X - _translate.X);
            _translate.Y = m.Y - (newScale / oldScale) * (m.Y - _translate.Y);

            _scale.ScaleX = _scale.ScaleY = newScale;

            ClampToBounds();
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (_content == null || _scale == null) return;

            var (cw, ch) = GetContentSize();
            double sw = cw * _scale.ScaleX, sh = ch * _scale.ScaleY;

            // pan ima smisla ako je sadržaj veći makar u jednoj dimenziji
            if (sw <= ActualWidth && sh <= ActualHeight) return;

            _panning = true;
            _panStart = e.GetPosition(this);
            CaptureMouse();
            Cursor = Cursors.Hand;
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_panning || _translate == null) return;

            Point p = e.GetPosition(this);
            Vector d = p - _panStart;
            _panStart = p;

            _translate.X += d.X;
            _translate.Y += d.Y;

            ClampToBounds();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (!_panning) return;

            _panning = false;
            ReleaseMouseCapture();
            Cursor = null;

            ClampToBounds();
            e.Handled = true;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (_content == null || _scale == null || _translate == null) return;

            if (AutoFitOnResize)
            {
                // fit + center na novu veličinu (po FitFactor)
                Fit(FitFactor);
            }
            else
            {
                // zadrži trenutnu skalu; centriraj samo ako je sadržaj manji
                var (cw, ch) = GetContentSize();
                double cwS = cw * _scale.ScaleX, chS = ch * _scale.ScaleY;

                if (cwS <= ActualWidth) _translate.X = (ActualWidth - cwS) / 2.0;
                if (chS <= ActualHeight) _translate.Y = (ActualHeight - chS) / 2.0;

                ClampToBounds();
            }
        }

        // ----- Public API -----

        /// <summary>
        /// Fit na viewport (cela mapa staje), sa faktorom (npr. 0.95 = malo manja od fit-a).
        /// </summary>
        public void Fit(double factor = 1.0)
        {
            if (_content == null || _scale == null || _translate == null) return;

            double fit = ComputeFitScale();
            double target = Clamp(fit * factor, MinScale, MaxScale);

            _scale.ScaleX = _scale.ScaleY = target;

            var (cw, ch) = GetContentSize();
            double cwS = cw * target, chS = ch * target;

            // centriraj
            _translate.X = (ActualWidth - cwS) / 2.0;
            _translate.Y = (ActualHeight - chS) / 2.0;

            ClampToBounds();
        }

        /// <summary>
        /// Vraća minimalno neophodno stanje (poštuje MinScale i fit granice).
        /// </summary>
        public void Reset()
        {
            Fit(1.0);
        }

        // ----- Helpers -----

        private (double w, double h) GetContentSize()
        {
            if (_content == null) return (0, 0);

            double w = _content.Width;
            if (double.IsNaN(w) || w <= 0) w = _content.ActualWidth;

            double h = _content.Height;
            if (double.IsNaN(h) || h <= 0) h = _content.ActualHeight;

            return (w, h);
        }

        private double ComputeFitScale()
        {
            if (_content == null) return 0.01;

            var (cw, ch) = GetContentSize();
            if (cw <= 0 || ch <= 0 || ActualWidth <= 0 || ActualHeight <= 0)
                return 0.01;

            double sx = ActualWidth / cw;
            double sy = ActualHeight / ch;
            double fit = sx < sy ? sx : sy; // manji od ta dva
            if (double.IsInfinity(fit) || double.IsNaN(fit)) fit = 0.01;
            return fit;
        }

        /// <summary>
        /// Uskladi minimalnu skalu (fit) i ogradi prevod (translate) prema viewport-u.
        /// </summary>
        private void FitAndClamp()
        {
            if (_content == null || _scale == null || _translate == null) return;

            double fitMin = ComputeFitScale();
            double effMin = MinScale > fitMin ? MinScale : fitMin;

            if (_scale.ScaleX < effMin)
                _scale.ScaleX = _scale.ScaleY = effMin;

            ClampToBounds();
        }

        private void ClampToBounds()
        {
            if (_content == null || _scale == null || _translate == null) return;

            double edge = EdgeMargin;

            var (cw0, ch0) = GetContentSize();
            double cw = cw0 * _scale.ScaleX;
            double ch = ch0 * _scale.ScaleY;

            double vw = ActualWidth;
            double vh = ActualHeight;

            // X
            if (cw <= vw)
            {
                _translate.X = (vw - cw) / 2.0; // centriraj kad je manje
            }
            else
            {
                double minX = vw - cw + edge; // najdalje ulevo (negativno)
                double maxX = -edge;          // najdalje udesno (blizu 0)
                if (_translate.X < minX) _translate.X = minX;
                if (_translate.X > maxX) _translate.X = maxX;
            }

            // Y
            if (ch <= vh)
            {
                _translate.Y = (vh - ch) / 2.0;
            }
            else
            {
                double minY = vh - ch + edge;
                double maxY = -edge;
                if (_translate.Y < minY) _translate.Y = minY;
                if (_translate.Y > maxY) _translate.Y = maxY;
            }
        }
        public void Center()
        {
            if (_content == null || _scale == null || _translate == null) return;

            var (cw, ch) = GetContentSize();
            double cwS = cw * _scale.ScaleX;
            double chS = ch * _scale.ScaleY;

            _translate.X = (ActualWidth - cwS) / 2.0;
            _translate.Y = (ActualHeight - chS) / 2.0;

            ClampToBounds();
        }


        private static double Clamp(double v, double min, double max) =>
            v < min ? min : (v > max ? max : v);


    }
}
