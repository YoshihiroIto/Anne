using Anne.Foundation.Mvvm;
using Anne.Model.Git;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.MainWindow
{
    public class MainWindowVm : ViewModelBase
    {
        public ReactiveProperty<string> Title { get; } = new ReactiveProperty<string>("Anne");

        public Repository Repository { get; } = new Repository();

        public MainWindowVm()
        {
            Title.AddTo(MultipleDisposable);
            Repository.AddTo(MultipleDisposable);

            Test();
        }

        private void Test()
        {
            Repository.Path.Value = @"C:\Users\yoi\Documents\Wox";
        }
    }
}