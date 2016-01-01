using System.Windows;

namespace Anne.Foundation.Controls
{
    /// <summary>
    /// CommitLabelMark.xaml の相互作用ロジック
    /// </summary>
    public partial class CommitLabelMark
    {
        #region Type

        public CommitLabelType Type
        {
            get { return (CommitLabelType) GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register(
                "Type",
                typeof (CommitLabelType),
                typeof (CommitLabelMark),
                new FrameworkPropertyMetadata
                {
                    BindsTwoWayByDefault = false
                }
                );

        #endregion

        public CommitLabelMark()
        {
            InitializeComponent();
        }
    }
}