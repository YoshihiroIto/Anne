using System.Linq;
using Anne.Features;
using Anne.Foundation.Mvvm;
using Anne.Model;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Windows
{
    public class MainWindowVm : ViewModelBase
    {
        public ReactiveProperty<RepositoryVm> SelectedRepository { get; } = new ReactiveProperty<RepositoryVm>();

        public ReactiveProperty<string> Title { get; } = new ReactiveProperty<string>("Anne");

        public MainWindowVm()
        {
            SelectedRepository
                .AddTo(MultipleDisposable);

            var reposVm = new RepositoryVm(App.Instance.Repositories.First())
                .AddTo(MultipleDisposable);

            SelectedRepository.Value = reposVm;
        }
    }
}