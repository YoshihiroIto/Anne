using System.Threading.Tasks;
using Anne.Foundation.Mvvm;
using Anne.Model.Git;
using Reactive.Bindings.Extensions;

namespace Anne.MainWindow
{
    public class MainWindowVm : ViewModelBase
    {
        public Repository Repository { get; }

        public MainWindowVm()
        {
            Repository = new Repository(@"C:\Users\yoi\Documents\Wox")
                .AddTo(MultipleDisposable);
        }

        public void CheckoutTest()
        {
            Task.Run(() => Repository.CheckoutTest());
        }

        public void RemoveTest()
        {
            Task.Run(() => Repository.RemoveTest());
        }

        public void SwitchTest(string branchName)
        {
            Task.Run(() => Repository.SwitchTest(branchName));
        }

        public void FetchTest(string remoteName)
        {
            Task.Run(() => Repository.FetchTest(remoteName));
        }
    }
}
