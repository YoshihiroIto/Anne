using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using MetroRadiance.Core.Win32;

namespace Anne.Windows
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // http://grabacr.net/archives/1585
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            WindowSettings = new MainWindowSettings();
            WindowSettings.Reload();

            if (WindowSettings.Placement.HasValue == false)
                return;

            var placement = WindowSettings.Placement.Value;
            placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
            placement.flags = 0;
            placement.showCmd = placement.showCmd == SW.SHOWMINIMIZED ? SW.SHOWNORMAL : placement.showCmd;

            var hwnd = new WindowInteropHelper(this).Handle;
            NativeMethods.SetWindowPlacement(hwnd, ref placement);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (e.Cancel)
                return;

            WINDOWPLACEMENT placement;
            var hwnd = new WindowInteropHelper(this).Handle;
            NativeMethods.GetWindowPlacement(hwnd, out placement);

            WindowSettings.Placement = placement;
            WindowSettings.Save();
        }
    }
}
