using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;

namespace Anne.Features
{
    public class FileDiffTextEditorLeftMargin : AbstractMargin
    {
        private readonly FileDiffTextEditor _editor;
        private FileDiffVm.DiffLine[] DiffLines => ((FileDiffVm) _editor?.DataContext)?.DiffLines;

        private double _width = 80.0;

        public FileDiffTextEditorLeftMargin(FileDiffTextEditor editor)
        {
            Debug.Assert(editor != null);

            _editor = editor;
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(_width, 0);
        }

        protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
        {
            if (oldTextView != null)
            {
                oldTextView.VisualLinesChanged -= VisualLinesChanged;
                oldTextView.ScrollOffsetChanged -= ScrollOffsetChanged;
            }

            base.OnTextViewChanged(oldTextView, newTextView);

            if (newTextView != null)
            {
                newTextView.VisualLinesChanged += VisualLinesChanged;
                newTextView.ScrollOffsetChanged += ScrollOffsetChanged;
            }
        }

        private void VisualLinesChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        private void ScrollOffsetChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            FileDiffTextEditorHelper.DrawBackground(TextView, drawingContext, _width, DiffLines);
        }
    }
}