using System.Collections.ObjectModel;
using System.Linq;
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

        public static void Initialize()
        {
        }

        public static void Destory()
        {
            Instance.Repositories.ForEach(x => x.Dispose());
            Instance.Dispose();
        }

        private App()
        {
            new AnonymousDisposable(() => Repositories.ForEach(x => x.Dispose()))
                .AddTo(MultipleDisposable);

            // test
            Repositories.Add(new Repository(@"C:\Users\yoi\Documents\Anne"));
            Repositories.Add(new Repository(@"C:\Users\yoi\Documents\Wox"));
            Repositories.Add(new Repository(@"C:\Users\yoi\Documents\libgit2sharp_test"));
        }
    }
}