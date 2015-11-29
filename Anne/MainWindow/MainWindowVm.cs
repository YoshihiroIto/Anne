using System.Reactive.Linq;
using Anne.Foundation.Mvvm;
using Anne.Model.Git;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.MainWindow
{
    public class MainWindowVm : ViewModelBase
    {
        public ReadOnlyReactiveProperty<string> Title { get; private set; }

        public Repository Repository { get; } = new Repository();

        public MainWindowVm()
        {
            Repository.AddTo(MultipleDisposable);

            Title = Repository.Path
                .Select(path => string.IsNullOrEmpty(path) ? "Anne" : "Anne -- " + path)
                .ToReadOnlyReactiveProperty()
                .AddTo(MultipleDisposable);

            Test();
        }

        private void Test()
        {
            Repository.Path.Value = @"C:\Users\yoi\Documents\Wox";
        }
    }
}