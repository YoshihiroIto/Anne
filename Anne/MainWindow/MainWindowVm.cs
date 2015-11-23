using Livet;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.MainWindow
{
    public class MainWindowVm : ViewModel
    {
        public ReactiveProperty<string> Title { get; } = new ReactiveProperty<string>("Anne");

        public MainWindowVm()
        {
            Title.AddTo(MultipleDisposable);
        }
    }
}