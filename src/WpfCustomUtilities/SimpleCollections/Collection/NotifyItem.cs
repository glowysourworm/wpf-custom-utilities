using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.SimpleCollections.Collection.Interface;

namespace WpfCustomUtilities.SimpleCollections.Collection
{
    public abstract class NotifyItem : INotifyPropertyChanged, INotifyDictionaryKey
    {
        /// <summary>
        /// Event that is fired when a property in the view model is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Event that is fired when a hash code change is triggered by a property change
        /// </summary>
        public event NotifyHashCodeChanged HashCodeChangedEvent;

        private string _propertyUpdatingName;
        private int _propertyUpdatingHashCode;

        /// <summary>
        /// Call to begin an update to any property. Must call EndUpdate to complete this property update
        /// </summary>
        protected void BeginUpdate(string propertyName)
        {
            if (!string.IsNullOrEmpty(_propertyUpdatingName))
                throw new Exception("Must call EndUpdate() before beginning a new property change:  NotifyViewModel.cs");

            _propertyUpdatingName = propertyName;

            _propertyUpdatingHashCode = this.GetHashCode();
        }
        protected void EndUpdate()
        {
            if (string.IsNullOrEmpty(_propertyUpdatingName))
                throw new FormattedException("Must first call BeginUpdate to edit a property:  NotifyViewModel.cs");

            var newHashCode = this.GetHashCode();

            OnPropertyChanged(_propertyUpdatingName);

            if (_propertyUpdatingHashCode != newHashCode &&
                this.HashCodeChangedEvent != null)
                this.HashCodeChangedEvent(this, _propertyUpdatingHashCode, newHashCode);

            _propertyUpdatingName = null;
        }

        /// <summary>
        /// Raised INotifyPropertyChanged event if there's a change to the property. Returns true if there was
        /// a change
        /// </summary>
        protected virtual bool RaiseAndSetIfChanged<T>(ref T field, T value, [CallerMemberName] string memberName = "")
        {
            var changed = false;

            if (field == null)
                changed = value != null;
            else
                changed = !field.Equals(value);

            if (changed)
            {
                var oldHashCode = this.GetHashCode();

                // REF FIELD -> CHECK THAT HASH CODE IS MODIFIED BEFORE RETURN!
                field = value;

                var newHashCode = this.GetHashCode();

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(memberName));

                if (oldHashCode != newHashCode &&
                    this.HashCodeChangedEvent != null)
                    this.HashCodeChangedEvent(this, oldHashCode, newHashCode);
            }

            return changed;
        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
