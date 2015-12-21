using System.Linq;
using Anne.Features;
using Anne.Model;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.MainWindow
{
    public class MainWindowVm : ViewModelBase
    {
        public ReactiveProperty<RepositoryVm> Repository { get; } = new ReactiveProperty<RepositoryVm>();

        public MainWindowVm()
        {
            Repository
                .AddTo(MultipleDisposable);

            var reposVm = new RepositoryVm(App.Instance.Repositories.First())
                .AddTo(MultipleDisposable);

            Repository.Value = reposVm;
        }
    }
}