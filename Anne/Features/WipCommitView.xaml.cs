using System.Windows.Input;

namespace Anne.Features
{
    /// <summary>
    /// WipCommitView.xaml の相互作用ロジック
    /// </summary>
    public partial class WipCommitView
    {
        public WipCommitView()
        {
            InitializeComponent();
        }

        private void ListViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                (DataContext as WipCommitVm)?.ToggleStaging();
        }
    }
}
