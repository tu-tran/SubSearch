// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utils.cs" company="">
//   
// </copyright>
// <summary>
//   The <see cref="Extensions" /> class provides the custom attached properties for the framework elements.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SubSearch.WPF
{
    using System.Windows;
    using System.Windows.Input;

    /// <summary>The <see cref="Extensions" /> class provides the custom attached properties for the framework elements.</summary>
    public static class Extensions
    {
        /// <summary>The attached property determining whether the element will lose focus whenever the Enter key is hit.</summary>
        public static readonly DependencyProperty FocusNextOnEnterProperty = DependencyProperty.RegisterAttached(
            "FocusNextOnEnter", 
            typeof(bool), 
            typeof(Extensions), 
            new UIPropertyMetadata(false, OnFocusNextOnEnterPropertyChanged));

        /// <summary>Gets the value of the <see cref="FocusNextOnEnterProperty"/> property attached to the <paramref name="element"/>.</summary>
        /// <param name="element">The attached element.</param>
        /// <returns>The value of the <see cref="FocusNextOnEnterProperty"/> property attached to the <paramref name="element"/>.</returns>
        public static bool GetFocusNextOnEnter(FrameworkElement element)
        {
            return (bool)element.GetValue(FocusNextOnEnterProperty);
        }

        /// <summary>Sets the <paramref name="value"/> for the <see cref="FocusNextOnEnterProperty"/> attached to the <paramref name="element"/>.</summary>
        /// <param name="element">The attached element.</param>
        /// <param name="value">The new value.</param>
        public static void SetFocusNextOnEnter(FrameworkElement element, bool value)
        {
            element.SetValue(FocusNextOnEnterProperty, value);
        }

        /// <summary>Handles the <see cref="UIElement.KeyUp"/> event for element that has attached the <see cref="FocusNextOnEnterProperty"/>.</summary>
        /// <param name="sender">The attached element.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private static void OnElementKeyUp(object sender, KeyEventArgs eventArgs)
        {
            if (eventArgs.Key != Key.Return)
            {
                return;
            }

            var element = Keyboard.FocusedElement as UIElement;
            if (element != null)
            {
                element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }

            eventArgs.Handled = true;
        }

        /// <summary>Occurs when the value of the <see cref="FocusNextOnEnterProperty"/> is changed.</summary>
        /// <param name="sender">The attached object.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private static void OnFocusNextOnEnterPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            var element = sender as FrameworkElement;
            if (element == null)
            {
                return;
            }

            if (eventArgs.NewValue is bool == false)
            {
                return;
            }

            if ((bool)eventArgs.NewValue)
            {
                element.KeyUp += OnElementKeyUp;
            }
            else
            {
                element.KeyUp -= OnElementKeyUp;
            }
        }
    }
}