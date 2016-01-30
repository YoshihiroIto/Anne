using System.IO;
using System.Reflection;
using Jil;
using MetroRadiance.Interop.Win32;
using MetroRadiance.UI.Controls;

namespace Anne.Windows
{
    public class MainWindowSettings : IWindowSettings
    {
        public WINDOWPLACEMENT? Placement { get; set; }
        public double[] Columns { get; set; }

        private static string ConfigFilePath
        {
            get
            {
                var loc = Assembly.GetEntryAssembly().Location;
                var dir = Path.GetDirectoryName(loc) ?? string.Empty;

                return Path.Combine(dir, "Anne.MainWindow.json");
            }
        }

        public void Reload()
        {
            try
            {
                var json = File.ReadAllText(ConfigFilePath);

                var config = JSON.Deserialize<MainWindowSettings>(json);
                Placement = config.Placement;
                Columns = config.Columns;
            }
            catch
            {
                // ignored
            }
        }

        public void Save()
        {
            File.WriteAllText(ConfigFilePath, JSON.Serialize(this, Options.PrettyPrint));
        }
    }
}