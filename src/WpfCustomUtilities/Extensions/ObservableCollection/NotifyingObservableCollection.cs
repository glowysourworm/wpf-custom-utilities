﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using WpfCustomUtilities.Extensions.Event;

namespace WpfCustomUtilities.Extensions.ObservableCollection
{
    /// <summary>
    /// Collection that can be overridden to provide extra notification for certain collection events that
    /// don't get propagated by an ObservableCollection. Also provides a custom event for item properties updating.
    /// </summary>
    public class NotifyingObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public event SimpleEventHandler<NotifyingObservableCollection<T>, T, PropertyChangedEventArgs> ItemPropertyChanged;

        public NotifyingObservableCollection()
        {

        }

        public NotifyingObservableCollection(IEnumerable<T> collection) : base(collection)
        {
            Hook();
        }

        public NotifyingObservableCollection(List<T> list) : base(list)
        {
            Hook();
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            item.PropertyChanged += OnItemChanged;
        }

        protected override void RemoveItem(int index)
        {
            this[index].PropertyChanged -= OnItemChanged;

            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            Unhook();

            base.ClearItems();
        }

        private void Unhook()
        {
            foreach (var item in this)
                item.PropertyChanged -= OnItemChanged;
        }

        private void Hook()
        {
            foreach (var item in this)
                item.PropertyChanged += OnItemChanged;
        }

        private void OnItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.ItemPropertyChanged != null)
                this.ItemPropertyChanged(this, (T)sender, e);
        }
    }
}
