using System.Linq;
using Anne.Foundation.Mvvm;
using Reactive.Bindings.Extensions;
using StatefulModel;

namespace Anne.Model
{
    public class App : ModelBase
    {
        public static App Instance { get; } = new App();

        public ObservableSynchronizedCollection<Repository> Repositories { get; } =
            new ObservableSynchronizedCollection<Repository>();

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
            Repositories.AddTo(MultipleDisposable);

            // test
            Repositories.Add(new Repository(@"C:\Users\yoi\Documents\Wox"));
        }
    }
}