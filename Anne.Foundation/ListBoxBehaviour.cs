using System.Collections;
using System.Windows;
using System.Windows.Controls;

// http://qiita.com/kiichi54321/items/73c8a0ccf21ce990875d

namespace Anne.Foundation
{
    public class ListBoxBehaviour
    {
        public static readonly DependencyProperty SeletedItemsProperty =
            DependencyProperty.RegisterAttached("SeletedItems", typeof (IList), typeof (ListBoxBehaviour),
                new PropertyMetadata(SeletedItemsChanged));

        static void SeletedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (ListBox) d;
            element.SelectionChanged += Element_SelectionChanged;
        }

        static void Element_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var element = (ListBox) sender;
            element.SetValue(SeletedItemsProperty, element.SelectedItems);
        }

        public static void SetSeletedItems(UIElement element, IList value)
        {
            element.SetValue(SeletedItemsProperty, value);
        }

        public static IList GetSeletedItems(UIElement element)
        {
            return (IList) element.GetValue(SeletedItemsProperty);
        }
    }
}