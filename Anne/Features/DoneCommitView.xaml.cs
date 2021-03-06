﻿namespace Anne.Features
{
    /// <summary>
    /// DoneCommitView.xaml の相互作用ロジック
    /// </summary>
    public partial class DoneCommitView
    {
        public DoneCommitView()
        {
            InitializeComponent();
        }

        private void Grid_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as DoneCommitVm;
            vm?.TwoPaneLayout.UpdateLayout(DiffGrid.ActualWidth, DiffGrid.ActualHeight);
        }

        private void DiffGrid_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            var vm = DataContext as DoneCommitVm;
            vm?.TwoPaneLayout.UpdateLayout(DiffGrid.ActualWidth, DiffGrid.ActualHeight);
        }
    }
}
