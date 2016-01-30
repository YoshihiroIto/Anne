using System;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Windows;
using System.Windows.Media;
using Anne.Foundation;
using MetroRadiance.UI;

namespace Anne
{
    /// <summary>
    ///     Entry.xaml の相互作用ロジック
    /// </summary>
    public partial class Entry
    {
        [STAThread]
        public static void Main(string[] args)
        {
            ProfileOptimization.SetProfileRoot(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            ProfileOptimization.StartProfile("Startup.Profile");

            DisposableChecker.Start();
            {
                var e = new Entry();
                e.InitializeComponent();
                e.Run();

                Model.App.Destory();
                MigemoHelper.Instance.Destory();
            }
            DisposableChecker.End();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ThemeService.Current.Register(this, Theme.Light, Color.FromRgb(0xFF, 0x20, 0x50).ToAccent());

            Livet.DispatcherHelper.UIDispatcher = Dispatcher;
            Reactive.Bindings.UIDispatcherScheduler.Initialize();

            MigemoHelper.Instance.Initialize();
            Model.App.Initialize();
        }
    }
}