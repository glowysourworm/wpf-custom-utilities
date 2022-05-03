using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using WpfCustomUtilities.Extensions.Event;
using WpfCustomUtilities.Extensions.ObservableCollection.Interface;

namespace WpfCustomUtilities.Extensions.ObservableCollection
{
    public class MultiObservableCollection<T> : ObservableCollection<T> where T : ISelectableViewModel, INotifyPropertyChanged
    {
        public event SimpleEventHandler<MultiObservableCollection<T>, T, PropertyChangedEventArgs> ItemPropertyChanged;

        bool _isUpdating = false;

        public MultiObservableCollection()
        {

        }

        public MultiObservableCollection(IEnumerable<T> collection) : base(collection)
        {
            Hook();
        }

        public MultiObservableCollection(List<T> list) : base(list)
        {
            Hook();
        }

        /// <summary>
        /// De-selects all items in the collection
        /// </summary>
        public void Reset()
        {
            _isUpdating = true;

            foreach (var item in this)
                item.IsSelected = false;

            _isUpdating = false;
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            item.PropertyChanged += OnItemChanged;
            item.IsSelectedChanged += OnItemSelected;
        }

        protected override void RemoveItem(int index)
        {
            this[index].PropertyChanged -= OnItemChanged;
            this[index].IsSelectedChanged -= OnItemSelected;

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
            {
                item.IsSelectedChanged -= OnItemSelected;
                item.PropertyChanged -= OnItemChanged;
            }
        }

        private void Hook()
        {
            foreach (var item in this)
            {
                item.IsSelectedChanged -= OnItemSelected;
                item.PropertyChanged += OnItemChanged;
            }
        }

        private void OnItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.ItemPropertyChanged != null)
                this.ItemPropertyChanged(this, (T)sender, e);
        }
        private void OnItemSelected(ISelectableViewModel sender)
        {
            if (_isUpdating)
                return;

            _isUpdating = true;

            foreach (var item in this)
                item.IsSelected = ReferenceEquals(item, sender);

            _isUpdating = false;
        }
    }
}
