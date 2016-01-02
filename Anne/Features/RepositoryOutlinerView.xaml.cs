using System.ComponentModel;
using System.Windows;

namespace Anne.Features
{
    /// <summary>
    /// RepositoryOutlinerView.xaml の相互作用ロジック
    /// </summary>
    public partial class RepositoryOutlinerView
    {
        #region SelectedItem

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                "SelectedItem",
                typeof (object),
                typeof (RepositoryOutlinerView),
                new FrameworkPropertyMetadata
                {
                    DefaultValue = default(object),
                    BindsTwoWayByDefault = false
                }
                );

        #endregion

        public RepositoryOutlinerView()
        {
            InitializeComponent();

            var dpd = DependencyPropertyDescriptor.FromProperty(LastSelectedItemProperty,
                typeof (RepositoryOutlinerView));
            dpd.AddValueChanged(this, (s, e) =>
            {
                if (LastSelectedItem == null)
                    return;

                SelectedItem = LastSelectedItem;

                // todo:バインディングが働いてくれないので、VMに直接書き込んでいる。要調査
                var vm = DataContext as RepositoryOutlinerVm;
                if (vm != null)
                    vm.SelectedItem.Value = (RepositoryOutlinerItemVm)SelectedItem;
            });
        }
    }
}