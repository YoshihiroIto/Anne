using System.Linq;
using System.Reactive.Linq;
using Anne.Features;
using Anne.Foundation.Mvvm;
using Anne.Model;
using MetroRadiance.Controls;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Windows
{
    public class MainWindowVm : ViewModelBase
    {
        public ReadOnlyReactiveCollection<RepositoryVm> Repositories { get; }

        public ReactiveProperty<RepositoryVm> SelectedRepository { get; }
            = new ReactiveProperty<RepositoryVm>();

        public ReadOnlyReactiveProperty<string> Title { get; }

        public MainWindowVm()
        {
            SelectedRepository
                .AddTo(MultipleDisposable);

            Repositories = App.Instance.Repositories
                .ToReadOnlyReactiveCollection(x => new RepositoryVm(x, this))
                .AddTo(MultipleDisposable);

            Title = SelectedRepository
                .Select(x => x == null ? "Anne" : "Anne -- " + x.Path)
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            SelectedRepository.Value = Repositories.FirstOrDefault();

// 現在のフォーカスを持っているコントロールを表示する
#if false
            Task.Run(() =>
            {
                while (true)
                {
                    Livet.DispatcherHelper.UIDispatcher.Invoke(() => Debug.WriteLine(Keyboard.FocusedElement));
                    Thread.Sleep(1000);
                }
            });
#endif
        }
    }
}