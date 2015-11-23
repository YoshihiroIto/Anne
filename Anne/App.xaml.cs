using System;
using System.IO;
using System.Reflection;
using System.Runtime;
using Anne.Foundation;

namespace Anne
{
    /// <summary>
    ///     App.xaml の相互作用ロジック
    /// </summary>
    public partial class App
    {
        [STAThread]
        public static void Main(string[] args)
        {
            ProfileOptimization.SetProfileRoot(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            ProfileOptimization.StartProfile("Startup.Profile");

            DisposableChecker.Start();
            {
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }

            DisposableChecker.End();
        }
    }
}