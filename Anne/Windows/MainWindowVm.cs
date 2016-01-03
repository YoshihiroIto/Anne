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
        public ReadOnlyReactiveCollection<RepositoryVm> Repositories { get; }

        public ReactiveProperty<RepositoryVm> SelectedRepository { get; }
            = new ReactiveProperty<RepositoryVm>();

        public ReactiveProperty<string> Title { get; } = new ReactiveProperty<string>("Anne");

        public MainWindowVm()
        {
            Title
                .AddTo(MultipleDisposable);

            SelectedRepository
                .AddTo(MultipleDisposable);

            Repositories = App.Instance.Repositories
                .ToReadOnlyReactiveCollection(x => new RepositoryVm(x, this))
                .AddTo(MultipleDisposable);

            SelectedRepository.Value = Repositories.FirstOrDefault();
        }
    }
}