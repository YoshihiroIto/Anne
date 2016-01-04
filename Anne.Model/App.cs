using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Anne.Foundation.Mvvm;
using Anne.Model.Git;
using Reactive.Bindings.Extensions;
using StatefulModel;

namespace Anne.Model
{
    public class App : ModelBase
    {
        public static App Instance { get; } = new App();

        public ObservableCollection<Repository> Repositories { get; }
            = new ObservableCollection<Repository>();

        private readonly AppConfig _config;

        private static string ConfigFilePath
        {
            get
            {
                var loc = Assembly.GetEntryAssembly().Location;
                var dir = Path.GetDirectoryName(loc) ?? string.Empty;

                return Path.Combine(dir, "Anne.config.yml");
            }
        }

        public static void Initialize()
        {
        }

        public static void Destory()
        {
            AppConfig.SaveToFile(ConfigFilePath, Instance._config);

            Instance.Repositories.ForEach(x => x.Dispose());
            Instance.Dispose();
        }

        private App()
        {
            _config = AppConfig.LoadFromFile(ConfigFilePath);

            new AnonymousDisposable(() => Repositories.ForEach(x => x.Dispose()))
                .AddTo(MultipleDisposable);

            _config.Repositories.ForEach(r => Repositories.Add(new Repository(r)));
        }
    }
}