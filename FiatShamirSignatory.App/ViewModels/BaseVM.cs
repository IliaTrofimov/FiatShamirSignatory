using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;

namespace FiatShamirSignatory.App.ViewModels
{
    internal abstract class BaseVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected PropertyChangeInfo<T> SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) 
                return new PropertyChangeInfo<T>(this);

            var info = new PropertyChangeInfo<T>(this, value, field, propertyName);
            field = value;
            OnPropertyChanged(propertyName);
            return info;
        }


        protected class PropertyChangeInfo<T>
        {
            string propertyName;
            T? newValue;
            T? oldValue;
            BaseVM context;
            public bool HasChanged { private set; get; }

            public PropertyChangeInfo(BaseVM context, [CallerMemberName] string propertyName = "")
            {
                this.propertyName = propertyName;
                this.HasChanged = false;
                this.context = context;
            }

            public PropertyChangeInfo(BaseVM context, T? newValue, T? oldValue, string propertyName = "")
            {
                this.propertyName = propertyName;
                this.HasChanged = true;
                this.oldValue = oldValue;
                this.newValue = newValue;
                this.context = context;
            }


            /// <summary>
            /// Notifies context that related readonly properties must be updated too.
            /// </summary>
            /// <param name="relatedProperties">Set of readonly properties that use current property</param>
            public PropertyChangeInfo<T> AddRelated(params string[] relatedProperties)
            {
                if (HasChanged)
                {
                    foreach (var property in relatedProperties.Where(p => p != propertyName))
                        context.OnPropertyChanged(property);
                }
                return this;
            }

            /// <summary>
            /// Notifies context that related property must be updated too.
            /// </summary>
            /// <param name="action">Setter function</param>
            public PropertyChangeInfo<T> AddRelated(Action action)
            {
                if (HasChanged)
                    action();
                return this;
            }
        }
    }
}
