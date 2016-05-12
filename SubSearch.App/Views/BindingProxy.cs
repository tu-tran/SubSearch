// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BindingProxy.cs" company="">
//   
// </copyright>
// <summary>
//   The binding proxy.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SubSearch.WPF.View
{
    using System.Windows;

    /// <summary>The binding proxy.</summary>
    public class BindingProxy : Freezable
    {
        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        /// <summary>The data property.</summary>
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            "Data",
            typeof(object),
            typeof(BindingProxy),
            new UIPropertyMetadata(null));

        /// <summary>Gets or sets the data.</summary>
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

        /// <summary>The create instance core.</summary>
        /// <returns>The <see cref="Freezable" />.</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
    }
}