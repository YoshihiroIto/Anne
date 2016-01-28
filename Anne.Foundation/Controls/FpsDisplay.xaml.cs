using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace Anne.Foundation.Controls
{
    /// <summary>
    /// FpsDisplay.xaml の相互作用ロジック
    /// </summary>
    public partial class FpsDisplay
    {
        #region Fps
        public string Fps
        {
            get { return (string)GetValue(FpsProperty); }
            set { SetValue(FpsProperty, value); }
        }

        public static readonly DependencyProperty FpsProperty =
            DependencyProperty.Register(
                "Fps",
                typeof(string),
                typeof(FpsDisplay),
                new FrameworkPropertyMetadata
                {
                    DefaultValue            = default(string),
                    BindsTwoWayByDefault    = true
                }
            );
        #endregion

        private int _count;
        private readonly Timer _timer;

        public FpsDisplay()
        {
            InitializeComponent();

            _timer = new Timer(OnSecond, null, 0, 1000);
            CompositionTarget.Rendering += CompositionTargetOnRendering;
        }

        private void OnSecond(object state)
        {
            var fps = $"FPS:{_count}";
            _count = 0;

            Dispatcher.BeginInvoke(new Action(() => Fps = fps));
        }

        private void CompositionTargetOnRendering(object sender, EventArgs eventArgs)
        {
            ++_count;
        }

        private void render_Unloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= CompositionTargetOnRendering;
            _timer.Dispose();
        }
    }
}
