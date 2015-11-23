using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.MainWindow
{
    public class MainWindowVm : ViewModelBase
    {
        public ReactiveProperty<string> Title { get; } = new ReactiveProperty<string>("Anne");

        public MainWindowVm()
        {
            Title.AddTo(MultipleDisposable);
        }
    }
}