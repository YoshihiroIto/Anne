using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows;
using Anne.Foundation.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Anne.Features
{
    public class TwoPaneLayoutVm : ViewModelBase
    {
        public ReactiveProperty<Visibility> IsVerticalSplitterVisibility { get; } = new ReactiveProperty<Visibility>(Visibility.Collapsed);
        public ReactiveProperty<Visibility> IsHorizontalSplitterVisibility { get; } = new ReactiveProperty<Visibility>(Visibility.Visible);

        public ReactiveProperty<int> FirstRow { get; } = new ReactiveProperty<int>();
        public ReactiveProperty<int> FirstColumn { get; } = new ReactiveProperty<int>();
        public ReactiveProperty<int> FirstRowSpan { get; } = new ReactiveProperty<int>();
        public ReactiveProperty<int> FirstColumnSpan { get; } = new ReactiveProperty<int>();

        public ReactiveProperty<int> SecondRow { get; } = new ReactiveProperty<int>();
        public ReactiveProperty<int> SecondColumn { get; } = new ReactiveProperty<int>();
        public ReactiveProperty<int> SecondRowSpan { get; } = new ReactiveProperty<int>();
        public ReactiveProperty<int> SecondColumnSpan { get; } = new ReactiveProperty<int>();

        public ReactiveProperty<double> BasePaneWidth { get; } = new ReactiveProperty<double>();
        public ReactiveProperty<double> BasePaneHeight { get; } = new ReactiveProperty<double>();

        public TwoPaneLayoutVm()
        {
            FirstRow.AddTo(MultipleDisposable);
            FirstRowSpan.AddTo(MultipleDisposable);
            FirstColumn.AddTo(MultipleDisposable);
            FirstColumnSpan.AddTo(MultipleDisposable);

            SecondRow.AddTo(MultipleDisposable);
            SecondRowSpan.AddTo(MultipleDisposable);
            SecondColumn.AddTo(MultipleDisposable);
            SecondColumnSpan.AddTo(MultipleDisposable);

            BasePaneWidth.AddTo(MultipleDisposable);
            BasePaneHeight.AddTo(MultipleDisposable);

            BasePaneHeight.Subscribe(x =>
            {
                Debug.WriteLine(x);
            });

            BasePaneWidth
                .CombineLatest(BasePaneHeight, (width, height) => new {width, height})
                .Subscribe(x => UpdateLayout(x.width, x.height))
                .AddTo(MultipleDisposable);
        }

        public void UpdateLayout(double width, double height)
        {
            var isVertically = width < height;

            if (isVertically)
            {
                IsVerticalSplitterVisibility.Value = Visibility.Collapsed;
                IsHorizontalSplitterVisibility.Value = Visibility.Visible;

                FirstRow.Value = 0;
                FirstColumn.Value = 0;
                FirstRowSpan.Value = 1;
                FirstColumnSpan.Value = 3;

                SecondRow.Value = 2;
                SecondColumn.Value = 0;
                SecondRowSpan.Value = 1;
                SecondColumnSpan.Value = 3;
            }
            else
            {
                IsVerticalSplitterVisibility.Value = Visibility.Visible;
                IsHorizontalSplitterVisibility.Value = Visibility.Collapsed;

                FirstRow.Value = 0;
                FirstColumn.Value = 0;
                FirstRowSpan.Value = 3;
                FirstColumnSpan.Value = 1;

                SecondRow.Value = 0;
                SecondColumn.Value = 2;
                SecondRowSpan.Value = 3;
                SecondColumnSpan.Value = 1;
            }
        }
    }
}