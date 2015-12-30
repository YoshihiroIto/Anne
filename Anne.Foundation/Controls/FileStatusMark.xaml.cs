using System.Windows;

namespace Anne.Foundation.Controls
{
    /// <summary>
    /// MarkAdded.xaml の相互作用ロジック
    /// </summary>
    public partial class FileStatusMark
    {
        #region Status
        public LibGit2Sharp.ChangeKind Status
        {
            get { return (LibGit2Sharp.ChangeKind)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(
                "Status",
                typeof(LibGit2Sharp.ChangeKind),
                typeof(FileStatusMark),
                new FrameworkPropertyMetadata
                {
                    BindsTwoWayByDefault = false
                }
            );
        #endregion

        public FileStatusMark()
        {
            InitializeComponent();
        }
    }
}
