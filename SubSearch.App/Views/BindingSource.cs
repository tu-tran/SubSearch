// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BindingSource.cs" company="">
//   
// </copyright>
// <summary>
//   The binding source.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SubSearch.WPF.Views
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>The binding source.</summary>
    public abstract class BindingSource : INotifyPropertyChanged
    {
        /// <summary>Occurs when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Raises the <see cref="PropertyChanged" /> event.</summary>
        /// <param name="propertyName">The property name. If not specified, defaults to the caller member name.</param>
        public virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>Sets the property value. Raises the <see cref="PropertyChanged" /> event, if needed.</summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="field">The property backing field.</param>
        /// <param name="value">The new property value.</param>
        /// <param name="propertyName">The property name. If not specified, defaults to the caller member name.</param>
        /// <returns>True if the property value was modified, otherwise false.</returns>
        protected bool SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                if (!string.IsNullOrEmpty(propertyName))
                {
                    this.RaisePropertyChanged(propertyName);
                }

                return true;
            }

            return false;
        }
    }
}