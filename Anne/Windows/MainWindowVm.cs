using System;
using System.Linq;
using System.Reactive.Linq;
using Anne.Features;
using Anne.Foundation;
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

        public ReadOnlyReactiveProperty<string> Title { get; }

        public ReactiveProperty<string> Filter { get; }

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

            Filter = new ReactiveProperty<string>(string.Empty)
                .AddTo(MultipleDisposable);

            Filter
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Where(_ => SelectedRepository.Value != null)
                .Subscribe(x => SelectedRepository.Value.WordFilter.Value = new WordFilter(x))
                .AddTo(MultipleDisposable);

            SelectedRepository
                .Subscribe(_ => Filter.Value = string.Empty)
                .AddTo(MultipleDisposable);

            SelectedRepository.Value = Repositories.FirstOrDefault();

            Repositories
                .CollectionChangedAsObservable()
                .Where(_ => SelectedRepository.Value == null)
                .Subscribe(_ => SelectedRepository.Value = Repositories.FirstOrDefault())
                .AddTo(MultipleDisposable);

// 現在のフォーカスを持っているコントロールを表示する
#if false
            Task.Run(() =>
            {
                while (true)
                {
                    Application.Current.Dispatcher.Invoke(() => Debug.WriteLine(Keyboard.FocusedElement));
                    Thread.Sleep(1000);
                }
            });
#endif
        }

        private bool _isInitialized;
        public void OnContentRendered()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            App.Instance.Repositories.ForEach(x => x.StartJob());
        }
    }
}