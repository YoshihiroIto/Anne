using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

// http://qiita.com/namoshika/items/e2261d16dba61d4e5f1c を参考にしました

namespace Anne.Diff
{
    public class DiffFileViewListWheelScrollKillBehavior : Behavior<DiffFileView>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.DiffTextEditor.PreviewMouseWheel += AssociatedObject_PreviewMouseWheel;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.DiffTextEditor.PreviewMouseWheel -= AssociatedObject_PreviewMouseWheel;
        }

        void AssociatedObject_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            var newEventArgs = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent
            };

            ((UIElement) AssociatedObject.DiffTextEditor.Parent).RaiseEvent(newEventArgs);
        }
    }
}