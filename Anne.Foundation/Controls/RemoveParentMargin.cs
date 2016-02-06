using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Anne.Foundation.Controls
{
    // https://www.interact-sw.co.uk/iangblog/2007/05/30/wpf-listview-column-margins
    public class RemoveParentMargin : Decorator
    {
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            var cp = VisualTreeHelper.GetParent(this) as FrameworkElement;

            if (cp != null)
                cp.Margin = new Thickness(0);
        }
    }
}
