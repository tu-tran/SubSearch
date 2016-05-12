// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BindingProxy.cs" company="">
//   
// </copyright>
// <summary>
//   The binding proxy.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SubSearch.WPF.Views
{
    using System.Windows;

    /// <summary>
    /// The <see cref="BindingProxy"/> class.
    /// </summary>
    /// <seealso cref="System.Windows.Freezable" />
    public class BindingProxy : Freezable
    {
        /// <summary>
        /// The data property.
        /// </summary>
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            "Data",
            typeof(object),
            typeof(BindingProxy),
            new UIPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public object Data
        {
            get
            {
                return this.GetValue(DataProperty);
            }

            set
            {
                this.SetValue(DataProperty, value);
            }
        }

        /// <summary>
        /// When implemented in a derived class, creates a new instance of the <see cref="T:System.Windows.Freezable" /> derived class.
        /// </summary>
        /// <returns>The new instance.</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
    }
}