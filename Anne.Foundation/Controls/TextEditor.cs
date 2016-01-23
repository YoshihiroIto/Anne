using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Anne.Foundation.Controls
{
    public class TextEditor : ICSharpCode.AvalonEdit.TextEditor, INotifyPropertyChanged
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof (string), typeof (TextEditor),
                new PropertyMetadata((obj, args) =>
                {
                    var target = (TextEditor) obj;
                    target.Text = (string) args.NewValue;
                })
                );

        public new string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TextEditor()
        {
            TextArea.SelectionBrush = Application.Current.FindResource("CurrentBackgroundBrush") as Brush;
            TextArea.SelectionBorder = Application.Current.FindResource("CurrentBorderPen") as Pen;
            TextArea.SelectionCornerRadius = 0;
            TextArea.SelectionForeground = Brushes.Black;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            RaisePropertyChanged(nameof(Text));
            base.OnTextChanged(e);
        }

        public void RaisePropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}