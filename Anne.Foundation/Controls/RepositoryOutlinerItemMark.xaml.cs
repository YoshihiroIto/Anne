using System.Windows;

namespace Anne.Foundation.Controls
{
    /// <summary>
    /// RepositoryOutlinerItemMark.xaml の相互作用ロジック
    /// </summary>
    public partial class RepositoryOutlinerItemMark
    {
        #region Type

        public RepositoryOutlinerItemType Type
        {
            get { return (RepositoryOutlinerItemType) GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register(
                "Type",
                typeof (RepositoryOutlinerItemType),
                typeof (RepositoryOutlinerItemMark),
                new FrameworkPropertyMetadata
                {
                    BindsTwoWayByDefault = false
                }
                );

        #endregion

        public RepositoryOutlinerItemMark()
        {
            InitializeComponent();
        }
    }
}