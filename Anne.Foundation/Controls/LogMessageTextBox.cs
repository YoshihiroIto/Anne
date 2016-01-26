using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Anne.Foundation.Controls
{
    public class LogMessageTextBox : RichTextBox
    {
        #region Source

        public string Source
        {
            get { return (string) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof (string), typeof (LogMessageTextBox),
                new UIPropertyMetadata(string.Empty, (d, e) =>
                {
                    var self = d as LogMessageTextBox;
                    Debug.Assert(self != null);

                    self.ToolTip = e.NewValue;

                    self.UpdateText();
                }));

        #endregion

        private void UpdateText()
        {
            if (string.IsNullOrEmpty(Source))
            {
                try
                {
                    Document = new FlowDocument();
                }
                catch
                {
                    // ignored
                }
                return;
            }

            var lines = Source.Split('\n');
            var firstLine = lines.FirstOrDefault();
            var secondLines = string.Join("\n", lines.Skip(1));

            if (string.IsNullOrEmpty(secondLines) == false)
                secondLines = "\n" + secondLines;

            if ((Document.Blocks.FirstBlock as Paragraph)?.Inlines.Count != 2)
            {
                var doc = new FlowDocument();
                var para = new Paragraph();

                para.Inlines.Add(new Run(firstLine) {FontWeight = FontWeights.Bold});
                para.Inlines.Add(new Run(secondLines));

                doc.Blocks.Add(para);
                Document = doc;
            }
            else
            {
                var para = (Paragraph) Document.Blocks.FirstBlock;

                var first = para.Inlines.FirstInline as Run;
                var last = para.Inlines.LastInline as Run;

                Debug.Assert(first != null);
                Debug.Assert(last != null);

                if (first.Text != firstLine)
                    first.Text = firstLine;

                if (last.Text != secondLines)
                    last.Text = secondLines;
            }
        }
    }
}