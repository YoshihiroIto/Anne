using System.IO;
using System.Reflection;
using MetroRadiance.Controls;
using MetroRadiance.Core.Win32;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Anne.Windows
{
    public class MainWindowSettings : IWindowSettings
    {
        public WINDOWPLACEMENT? Placement { get; set; }

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

                var config = JsonConvert.DeserializeObject<MainWindowSettings>(json);
                Placement = config.Placement;
            }
            catch
            {
                // ignored
            }
        }

        public void Save()
        {
            File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}